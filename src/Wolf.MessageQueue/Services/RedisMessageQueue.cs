using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks; 
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace Wolf.MessageQueue.Services
{
    public class RedisMessageQueue : IQueueService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly CancellationToken _cancellationToken;
        private readonly QueueOptions _queueOptions;
        private readonly ILogger _logger;
        private object _listenTask;

        private IDatabase Database => _connectionMultiplexer.GetDatabase();

        private Func<RedisValue, Task<bool>> _messageRecievedHandler { get; set; }

        public RedisMessageQueue(IOptions<QueueOptions> queueOptions, ILogger<RedisMessageQueue> logger, CancellationToken cancellationToken = new CancellationToken())
        {
            _queueOptions = queueOptions.Value;
            _logger = logger;
            var configurationOptions = new ConfigurationOptions
            {
                ConnectRetry = _queueOptions.ConnectRetry,
                ReconnectRetryPolicy = new ExponentialRetry(_queueOptions.ReconnectDeltaBackOffMilliseconds),
                EndPoints = { _queueOptions.ConnectionString }
            };

            this._cancellationToken = cancellationToken;
            this._connectionMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);
        }

        public async Task AddMessageAsync(string channel, string message)
        {
            var id = await this.Database.StringIncrementAsync($"{channel}:jobid");
            var key = $"{channel}:{id}";
            var hashEntries = new HashEntry[] {
                new HashEntry("key", key),
                new HashEntry("message", message),
                new HashEntry("attempts", 0),
                new HashEntry("firstattempt", DateTimeOffset.UtcNow.ToString("O")),
            };

            await this.Database.HashSetAsync(key, hashEntries);
            await this.Database.ListRightPushAsync($"{channel}:message", key);

            await _connectionMultiplexer.GetSubscriber().PublishAsync($"{channel}:channel", "");
        }

        public async Task Subscribe(Func<RedisValue, Task<bool>> messageRecievedHandler, bool shouldRestoreUnprcessedOnReconnect=true, bool shouldEnableFiledMessageHandler=true)
        {
            _messageRecievedHandler = messageRecievedHandler;
            await Subscribe(_queueOptions.ChannelName);
            if (shouldRestoreUnprcessedOnReconnect)
            {
                _connectionMultiplexer.ConnectionRestored += ConnectionRestoredHandler;
            }
            if (shouldEnableFiledMessageHandler)
            {
                FailedMessagesHandler();
            }
        }

        private async Task Subscribe(string channelName)
        {
            var sub = _connectionMultiplexer.GetSubscriber();
            await sub.SubscribeAsync($"{channelName}:channel", async (c, v) => await HandleJobAsync());
        }

        protected async void ConnectionRestoredHandler(object source, ConnectionFailedEventArgs eventArgs)
        {
            try
            {
                await Subscribe(_queueOptions.ChannelName);
                await RestoreUnprocessedMessageAsync();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error in ConnectionRestoredHandler");
            }
        }

        public async Task RestoreUnprocessedMessageAsync()
        {
            string channelName = _queueOptions.ChannelName;
            try
            {
                RedisValue[] values = await Database.ListRangeAsync($"{channelName}:message");
                foreach (var value in values)
                {
                    await _connectionMultiplexer.GetSubscriber().PublishAsync($"{channelName}:channel", "");
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogDebug("RestoreUnprocessedMessageAsync Management Thread Finished.");
            }
        }
        protected void FailedMessagesHandler()
        {
            string channel = _queueOptions.ChannelName;
            _listenTask = Task.Factory.StartNew(async () =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(_queueOptions.RedeliveryFailDelayMilliseconds, _cancellationToken);
                    try
                    {
                        RedisValue[] values = Database.ListRange($"{channel}:failed");
                        foreach (var value in values)
                        {
                            int attempts = (int)Database.HashGet((string)value, "attempts");
                            if (attempts >= _queueOptions.RedeliveryMaxAttempts)
							{
                                await this.Database.HashDeleteAsync(value.ToString(), new RedisValue[] { "key", "message", "attempts", "firstattempt" });
                                this.Database.ListRemove($"{channel}:failed", value);
                                _logger.LogWarning($"Message {value} failed all {attempts} redelivery attempts.");
                                continue;
                            }
                            DateTimeOffset timeNow = DateTimeOffset.UtcNow;
                            string strFirstAttempt = Database.HashGet((string)value, "firstattempt");
                            if(!DateTimeOffset.TryParse(strFirstAttempt, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTimeOffset firstAttempt)) //this should never happen uless some code added a message without firstattempt
                            {
                                _logger.LogWarning($"firstattempt is not set for {value}");
                                firstAttempt = timeNow;
                                await Database.HashSetAsync((string)value, "firstattempt", firstAttempt.ToString("O"));
                            }
                            DateTimeOffset nextAttempt = CalculateNextAttemptTime(firstAttempt, attempts);
                            _logger.LogDebug($"message:{value}; firstAttempt: {firstAttempt}; nextAttempt: {nextAttempt}; timeNow: {timeNow};");
                            if (nextAttempt <= timeNow)
                            {
                                this.Database.ListRemove($"{channel}:failed", value);
                                this.Database.ListRightPush($"{channel}:message", value);
                                await this.HandleJobAsync();
							}
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        _logger.LogInformation("EnableFailedMessagesHandler Management Thread Finished.");
                    }
                    catch(Exception ex)
					{
                        _logger.LogError(ex, "Unexpected exception in EnableFailedMessagesHandler");
					}
                }
            }, TaskCreationOptions.LongRunning);
        }

        protected async Task HandleJobAsync()
        {
            string channelName = _queueOptions.ChannelName;
            var messageHeader = await Database.ListRightPopLeftPushAsync($"{channelName}:message", $"{channelName}:process");
            if (messageHeader.IsNullOrEmpty)
            {
                return;
            }
            string msgHeaderStr = messageHeader.ToString();
            await Database.HashIncrementAsync(msgHeaderStr, "attempts");

            bool isProcessed = false;
			if (null != _messageRecievedHandler)
			{
                var messageContent = await Database.HashGetAsync(msgHeaderStr, "message");
                isProcessed =await _messageRecievedHandler(messageContent);
            }
            if (!isProcessed)
            {
                await Database.ListRightPushAsync($"{channelName}:failed", messageHeader);
            }
            else
            {
                await Database.HashDeleteAsync(msgHeaderStr, new RedisValue[] { "key", "message", "attempts", "firstattempt" });
                await Database.ListRightPopAsync($"{channelName}:process");
            }
        }

        #region Implement IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connectionMultiplexer.ConnectionRestored -= ConnectionRestoredHandler;
                    _connectionMultiplexer.Close();
                }

                _disposed = true;
            }
        } 

        #endregion // Implement IDisposable

        private DateTimeOffset CalculateNextAttemptTime(DateTimeOffset firstAttempt, int attempts)
		{
            if (attempts < 1) return firstAttempt; //this is unexpected for retries becase at least one attempt should have happened already
            var delayMss = _queueOptions.RedeliveryFailDelayMilliseconds * Math.Pow(_queueOptions.RedeliveryExponentBase,  attempts-1);
            return firstAttempt.AddMilliseconds(delayMss);
        }
    }
}
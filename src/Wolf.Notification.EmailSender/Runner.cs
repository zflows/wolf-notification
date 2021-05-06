using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wolf.MessageQueue;
using Wolf.MessageQueue.Services;
using Wolf.Notification.EmailSender.OpenAPIs;
using Wolf.Notification.EmailSender.Services;

namespace Wolf.Notification.EmailSender
{
	public class Runner : BackgroundService, IDisposable
    {
        private readonly IQueueService _queueService;
        private readonly IMailService _mailService;
        private readonly IMessageService _messageService; 
        private readonly ILogger _logger;

        public Runner(IMailService mailService, IMessageService messageService, IQueueService queueService, ILogger<Runner> logger)
        {
            _queueService = queueService;
            _mailService = mailService;
            _messageService = messageService; 
            _logger = logger;
        }

        #region Implement IDisposable

        private bool _disposed;

        public override void Dispose()
        {
            base.Dispose();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _logger.LogInformation($"Disposing {disposing}");
            if (!_disposed)
            {
                if (disposing)
                {  
                    DisposeQueueService();
                }

                _disposed = true;
            }
        }


        #endregion // Implement IDisposable
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(10);
            _logger.LogInformation("Starting ExecuteAsync");

            await _queueService.Subscribe(MessageAction);
            await _queueService.RestoreUnprocessedMessageAsync();
            _logger.LogInformation("Finished ExecuteAsync");
        }

        async Task<bool> MessageAction(RedisValue msgId)
        {
            try
            {
                _logger.LogInformation($"Received: {msgId}");
                MessageDto message = await this._messageService.GetMessageAsync(Guid.Parse(msgId));
                _logger.LogDebug($"GetMessage returned {message.MessageId}");

                this._mailService.Send(message);
                _logger.LogDebug($"Message {message.MessageId} was sent successfully");

                var result = await this._messageService.SetDateOfSending(message.MessageId, DateTime.UtcNow);
                _logger.LogInformation($"Message with ID = {message.MessageId} was processed successfully");
                return true;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Error acting on message {msgId}");
                return false;
            }
        }
         
        private void DisposeQueueService()
        {
            if (_queueService != null)
            {
                this._queueService.Dispose();
            }
        }
    } 
}
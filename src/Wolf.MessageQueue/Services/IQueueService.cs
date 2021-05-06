using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wolf.MessageQueue.Services
{
	public interface IQueueService : IDisposable
    {
        //Func<RedisValue, Task<bool>> MessageRecievedHandler { get; set; }

        Task RestoreUnprocessedMessageAsync();

        Task AddMessageAsync(string channel, string message);

        Task Subscribe(Func<RedisValue, Task<bool>> messageRecievedHandler, bool shouldRestoreUnprcessedOnReconnect = true, bool shouldEnableFiledMessageHandler = true);

       // void EnableFailedMessagesHandler();
    }
}
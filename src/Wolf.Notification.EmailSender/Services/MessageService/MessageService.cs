using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Wolf.Notification.EmailSender.Config;
using Wolf.Notification.EmailSender.OpenAPIs;

namespace Wolf.Notification.EmailSender.Services
{
    public class MessageService : IMessageService
    {
        private readonly NotifMessageClient _notifMessageClient;

        public MessageService(HttpClient httpClient, IOptions<NotifApiOptions> notifApiOptions)
        {
            NotifApiOptions notifOptions=notifApiOptions.Value;
            _notifMessageClient = new NotifMessageClient(notifOptions, httpClient);
            _notifMessageClient.BaseUrl = notifOptions.BaseUrl;
        }

        public async Task<MessageDto> GetMessageAsync(Guid id)
        {
            return await _notifMessageClient.MessageGetAsync(id);
        }

        public async Task<MessageDto> SetDateOfSending(Guid id, DateTime date)
        {
            return await _notifMessageClient.CompleteAsync(id, new UpdateMessageRequest() { DateOfSending = date });
        }
    }
}
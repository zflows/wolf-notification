using System;
using System.Threading.Tasks;
using Wolf.Notification.EmailSender.OpenAPIs;

namespace Wolf.Notification.EmailSender.Services
{
    public interface IMessageService
    {
        Task<MessageDto> GetMessageAsync(Guid id);

        Task<MessageDto> SetDateOfSending(Guid id, DateTime date);
    }
}
using Wolf.Notification.EmailSender.OpenAPIs;

namespace Wolf.Notification.EmailSender.Services
{
    public interface IMailService
    {
        void Send(MessageDto msg);
    }
}
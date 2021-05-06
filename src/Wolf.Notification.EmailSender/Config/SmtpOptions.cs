namespace Wolf.Notification.EmailSender.Config
{
    public class SmtpOptions
    {
        public string Server { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string FromEmail { get; set; }
    }
}
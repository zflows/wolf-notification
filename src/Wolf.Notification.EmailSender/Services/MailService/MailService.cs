using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System;
using System.Linq;
using Wolf.Notification.EmailSender.Config;
using Wolf.Notification.EmailSender.OpenAPIs;

namespace Wolf.Notification.EmailSender.Services
{
    public class MailService : IMailService
    {
        private readonly SmtpOptions _smtpOptions;
        private readonly ILogger<MailService> _logger;

        public MailService(IOptions<SmtpOptions> options, ILogger<MailService> logger)
        {
            _smtpOptions = options.Value;
            _logger = logger;
            _logger.LogInformation($"Default Email from: {_smtpOptions.FromEmail}");
        }

        public void Send(MessageDto msg)
        {
            var message = new MimeMessage();
            if (null == msg.Sender || null == msg.Sender.Address)
            {
                message.From.Add(MailboxAddress.Parse(this._smtpOptions.FromEmail));
            }
            else
            {
                message.From.Add(new MailboxAddress(msg.Sender.Name, msg.Sender.Address));
            }

            foreach (var addr in msg.Recipients)
            {
                if (IsValidEmail(addr))
                {
                    InternetAddressList listToAdd= addr.TypeCode?.ToLower() switch
                    {
                        "to" => message.To,
                        "cc" => message.Cc,
                        "bcc" => message.Bcc,
                        _ => throw new InvalidOperationException($"Invalid address type {addr.TypeCode}")
                    };
                    listToAdd.Add(new MailboxAddress(addr.Name, addr.Address));
                }
                else
                {
                    _logger.LogInformation($"Invalid address {addr.Address} was found for message {msg.MessageId}");
                }
            }

            int recipientCount = message.To.Count + message.Cc.Count + message.Bcc.Count;
            if (recipientCount==0)
            {
                _logger.LogInformation($"Message with ID = {msg.MessageId} doesn't have any recipients");
                return;
            }

            message.Subject = msg.Subject;
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = msg.Body
            };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                    string userName = _smtpOptions.Username;
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.Connect(_smtpOptions.Server, _smtpOptions.Port, false);
                    if (!string.IsNullOrWhiteSpace(userName))
                    {
                        client.Authenticate(userName, _smtpOptions.Password);
                    }
                    client.Send(message);
                    client.Disconnect(true);

                    _logger.LogInformation($"Message with ID = {msg.MessageId} was sent to {recipientCount} recipients successfully");
                }
                catch (Exception exc)
                {
                    _logger.LogCritical(exc, exc.Message);
                    throw exc;
                }
            }
        }

        public bool IsValidEmail(RecipientWTypeDto addr)
        {
            try
            {
                var m = new System.Net.Mail.MailAddress(addr.Address, addr.Name);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

    }
}
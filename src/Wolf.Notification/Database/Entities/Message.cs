using System;
using System.Collections.Generic;

#nullable disable

namespace Wolf.Notification.Database.Entities
{
    public partial class Message
    {
        public Message()
        {
            MessageRecipients = new HashSet<MessageRecipient>();
        }

        public Guid MessageId { get; set; }
        public Guid TemplateId { get; set; }
        public string TokenData { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateProcessed { get; set; }
        public DateTime? DateSent { get; set; }
        public string ProviderCode { get; set; }
        public long FromRecipientId { get; set; }

        public virtual Recipient FromRecipient { get; set; }
        public virtual Provider ProviderCodeNavigation { get; set; }
        public virtual Template Template { get; set; }
        public virtual GeneratedMessage GeneratedMessage { get; set; }
        public virtual ICollection<MessageRecipient> MessageRecipients { get; set; }
    }
}

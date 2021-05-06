using System;
using System.Collections.Generic;

#nullable disable

namespace Wolf.Notification.Database.Entities
{
    public partial class Recipient
    {
        public Recipient()
        {
            MessageRecipients = new HashSet<MessageRecipient>();
            Messages = new HashSet<Message>();
            TemplateRecipients = new HashSet<TemplateRecipient>();
            Templates = new HashSet<Template>();
        }

        public long RecipientId { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }

        public virtual ICollection<MessageRecipient> MessageRecipients { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<TemplateRecipient> TemplateRecipients { get; set; }
        public virtual ICollection<Template> Templates { get; set; }
    }
}

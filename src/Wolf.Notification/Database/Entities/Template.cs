using System;
using System.Collections.Generic;

#nullable disable

namespace Wolf.Notification.Database.Entities
{
    public partial class Template
    {
        public Template()
        {
            Messages = new HashSet<Message>();
            TemplateRecipients = new HashSet<TemplateRecipient>();
        }

        public Guid TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string TemplateSubject { get; set; }
        public string TemplateBody { get; set; }
        public string DefaultProviderCode { get; set; }
        public long? DefaultFromRecipientId { get; set; }

        public virtual Recipient DefaultFromRecipient { get; set; }
        public virtual Provider DefaultProviderCodeNavigation { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<TemplateRecipient> TemplateRecipients { get; set; }
    }
}

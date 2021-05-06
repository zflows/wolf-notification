using System;
using System.Collections.Generic;

#nullable disable

namespace Wolf.Notification.Database.Entities
{
    public partial class RecipientType
    {
        public RecipientType()
        {
            MessageRecipients = new HashSet<MessageRecipient>();
            TemplateRecipients = new HashSet<TemplateRecipient>();
        }

        public string TypeCode { get; set; }
        public string Description { get; set; }

        public virtual ICollection<MessageRecipient> MessageRecipients { get; set; }
        public virtual ICollection<TemplateRecipient> TemplateRecipients { get; set; }
    }
}

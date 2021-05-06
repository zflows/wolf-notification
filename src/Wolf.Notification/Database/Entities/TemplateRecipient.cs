using System;
using System.Collections.Generic;

#nullable disable

namespace Wolf.Notification.Database.Entities
{
    public partial class TemplateRecipient
    {
        public long TrId { get; set; }
        public Guid TemplateId { get; set; }
        public long RecipientId { get; set; }
        public string TypeCode { get; set; }

        public virtual Recipient Recipient { get; set; }
        public virtual Template Template { get; set; }
        public virtual RecipientType TypeCodeNavigation { get; set; }
    }
}

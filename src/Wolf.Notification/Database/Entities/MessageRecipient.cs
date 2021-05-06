using System;
using System.Collections.Generic;

#nullable disable

namespace Wolf.Notification.Database.Entities
{
    public partial class MessageRecipient
    {
        public long MrId { get; set; }
        public Guid MessageId { get; set; }
        public long RecipientId { get; set; }
        public string TypeCode { get; set; }

        public virtual Message Message { get; set; }
        public virtual Recipient Recipient { get; set; }
        public virtual RecipientType TypeCodeNavigation { get; set; }
    }
}

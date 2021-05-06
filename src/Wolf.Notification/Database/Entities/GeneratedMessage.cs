using System;
using System.Collections.Generic;

#nullable disable

namespace Wolf.Notification.Database.Entities
{
    public partial class GeneratedMessage
    {
        public Guid MessageId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public virtual Message Message { get; set; }
    }
}

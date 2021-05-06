using System;
using System.Collections.Generic;

#nullable disable

namespace Wolf.Notification.Database.Entities
{
    public partial class Provider
    {
        public Provider()
        {
            Messages = new HashSet<Message>();
            Templates = new HashSet<Template>();
        }

        public string ProviderCode { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<Template> Templates { get; set; }
    }
}

using System.Collections.Generic;

namespace Wolf.Notification.Models
{
	public class MessagePreviewDto
    {
        public ProviderDto Provider { get; set; }
        public TemplateIdNameDto Template { get; set; }

        public string TokenData { get; set; }

        public RecipientDto Sender { get; set; }

        public IEnumerable<RecipientWTypeDto> Recipients { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
    }

}
using System;

namespace Wolf.Notification.Models
{
	public class MessageDto: MessagePreviewDto
    {
        public Guid MessageId { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime? DateProcessed { get; set; }

        public DateTime? DateSent { get; set; }

    }

}
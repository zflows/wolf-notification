using System;

namespace Wolf.Notification.Models
{
	public class MsgTimeSeria
	{
		public Guid TemplateId { get; set; }
		public DateTime DayDate { get; set; }
		public int MessageCount { get; set; }
	}
}

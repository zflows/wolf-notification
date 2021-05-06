using System.Collections.Generic;

namespace Wolf.Notification.Models
{
	public class DashboardTimeSeries
	{
		public IEnumerable<MsgTimeSeria> MsgTimeSerias { get; set; }
		public IEnumerable<TemplateIdNameDto> Templates { get; set; }
	}
}

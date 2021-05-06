namespace Wolf.Notification.Models
{
	public class DashboardSummary
	{
		public int TotalTemplates { get; set; }
		public int TotalMessages { get; set; }

		public int SuccessMessages { get; set; }

		public int PendingMessages { get; set; }
	}
}

namespace Wolf.Notification.Models
{
	public class DashboardTemplate: TemplateIdNameDto
	{
		public int TotalMessages { get; set; }
		public int SuccessMessages { get; set; }
		public int PendingMessages { get; set; }
	}
}

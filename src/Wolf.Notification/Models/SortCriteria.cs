namespace Wolf.Notification.Models
{
	public enum SortOrder
	{
		Asc,
		Dec
	}

	public enum SortField
	{
		Id,
		TemplateName,
		Subject,
		SenderAddress,
		SenderName,
		DateCreated,
		DateProcessed,
		DateSent
	}

	public class SortCriteria
	{
		public SortField Field { get; set; }
		public SortOrder Order { get; set; }
	}
}

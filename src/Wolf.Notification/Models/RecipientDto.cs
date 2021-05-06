namespace Wolf.Notification.Models
{
	public class RecipientDto
	{
		public string Address { get; set; }
		public string Name { get; set; }
	}

	public class RecipientWIdDto : RecipientDto
	{
		public long RecipientId { get; set; }
	}

	public class RecipientWTypeDto : RecipientDto
	{
		public string TypeCode { get; set; }
	}

	public class RecipientWIdTypeDto : RecipientWIdDto
	{
		public string TypeCode { get; set; }
	}
}

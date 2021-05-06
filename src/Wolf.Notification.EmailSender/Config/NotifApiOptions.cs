using System;
using System.Collections.Generic;
using System.Text;

namespace Wolf.Notification.EmailSender.Config
{
	public class NotifApiOptions
	{
		public string BaseUrl { get; set; }
		public string UserAgentName { get; set; }
		public AuthenticationOptions AuthOptions { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Wolf.Notification.EmailSender.Config
{
	public class AuthenticationOptions
	{
		public string StsUrl { get; set; }
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }
		public string Scope { get; set; }
	}
}

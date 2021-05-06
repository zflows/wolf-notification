using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wolf.Notification.Config
{
	public class JwtAuthenticationOptions
	{
		public string Authority { get; set; }
		public string Audience { get; set; }
		public bool RequireHttpsMetadata { get; set; }
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }
		public string[] TrustedClientIds { get; set; }
		public string TrustedClientEnvironment { get; set; }
	}
}

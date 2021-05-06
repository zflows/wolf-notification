using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wolf.Notification.Config
{
	public class RequestResponseLoggerOptions
	{
		public bool LogRequest { get; set; } = false;
		public bool LogResponse { get; set; } = false;

		public int MaxResponseLength { get; set; } = -1;

		public bool ShouldEnable { get { return (LogRequest || LogResponse); } }
	}
}

using Xunit;
using Wolf.Notification.EmailSender.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Wolf.Notification.EmailSender.OpenAPIs;
using Microsoft.Extensions.Options;
using System.Net.Http;
using Wolf.Notification.EmailSender.Config;

namespace Wolf.Notification.EmailSender.Services.Tests
{
	public class MessageServiceTests
	{
		//[Fact()]
		public async System.Threading.Tasks.Task GetMessageAsyncTestAsync()
		{
			var notifApiOptions = new NotifApiOptions() {
				BaseUrl = "http://localhost:5380/",
				AuthOptions = new AuthenticationOptions()
				{
					ClientId = "notif_sender",
					ClientSecret = "s123",
					Scope = "notif_api",
					StsUrl = "https://devid.mycompany.com"
				}
			};
			MessageService svc = new MessageService(new HttpClient(), Options.Create<NotifApiOptions>(notifApiOptions));
			MessageDto msg= await svc.GetMessageAsync(new Guid("c4e41563-24a1-4593-901f-c15eccbe26b0"));
			Assert.NotNull(msg);
			msg = await svc.GetMessageAsync(new Guid("c4e41563-24a1-4593-901f-c15eccbe26b0"));
			Assert.NotNull(msg);
		}
	}
}
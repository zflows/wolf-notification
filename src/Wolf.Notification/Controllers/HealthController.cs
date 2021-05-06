using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Wolf.MessageQueue.Services;
using Wolf.Notification.Database.Entities;

namespace Wolf.Notification.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController
    {
        private readonly NotifDbContext _dbContext;
        //private IQueueService _queueService;

        public HealthController( NotifDbContext dbContext) //, IQueueService queueService)
        {
            this._dbContext = dbContext;
            //this._queueService = queueService;
        }

        [HttpGet()]
        public dynamic Check()
        {
            return new
            {
                status = "UP"
            };
        }

        [HttpGet("details")]
        public dynamic DetailedCheck()
        {
            var canConnectToSqlServer = this._dbContext.Database.CanConnect();

            return new
            {
                SqlServer = canConnectToSqlServer == true ? "UP" : "DOWN",
            };
        }
    }
}
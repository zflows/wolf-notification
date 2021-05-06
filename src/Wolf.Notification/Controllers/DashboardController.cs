using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolf.Notification.Database.Entities;
using Wolf.Notification.Models;

namespace Wolf.Notification.Controllers
{
	[Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "NotificationManager")]
    public class DashboardController : ControllerBase
    {
        private readonly NotifDbContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="mapper"></param>
        public DashboardController(NotifDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<DashboardSummary> Get()
        {
            return new DashboardSummary()
            {
                TotalTemplates = await _context.Templates.CountAsync(),
                TotalMessages = await _context.Messages.CountAsync(),
                SuccessMessages = await _context.Messages.CountAsync(w => w.DateSent != null),
                PendingMessages = await _context.Messages.CountAsync(w => w.DateSent == null && w.DateProcessed!=null)
            };
        }

        [HttpGet("timeseries")]
        public async Task<DashboardTimeSeries> GetTimeSeries()
        {
            var msgTimeSeris = await _context.MsgTimeSeries.FromSqlInterpolated($"notif.stats_time_series").ToListAsync();
            IEnumerable<Guid> templateIds = msgTimeSeris.Select(s => s.TemplateId).Distinct();
            IEnumerable<TemplateIdNameDto> templates = await _context.Templates
                .Where(t => templateIds.Contains(t.TemplateId))
                .Select(s => new TemplateIdNameDto() { TemplateId = s.TemplateId, TemplateName = s.TemplateName })
                .ToListAsync();

            return new DashboardTimeSeries() { MsgTimeSerias = msgTimeSeris, Templates = templates };
        }

        [HttpGet("templates")]
        public async Task<IEnumerable<DashboardTemplate>> GetTemplates()
        {
            var popularTemplIds = await _context.Messages
                .Where(w => w.DateCreated > DateTime.Now.AddDays(-30))
                .GroupBy(g => g.TemplateId)
                .Select(s => new DashboardTemplate() { TemplateId = s.Key, TotalMessages= s.Count() })
                .OrderByDescending(r => r.TotalMessages)
                .Take(5).Select(s => s.TemplateId).ToListAsync();

            var templates=from m in _context.Messages.Where(w => w.DateCreated > DateTime.Now.AddDays(-30) && popularTemplIds.Contains(w.TemplateId))
                group m by m.TemplateId into g
                select new DashboardTemplate() { 
                    TemplateId = g.Key, 
                    TotalMessages = g.Count(), 
                    SuccessMessages = g.Count(w => w.DateSent != null), 
                    PendingMessages = g.Count(w => w.DateSent == null && w.DateProcessed != null),
                    TemplateName=_context.Templates.Single(s=> s.TemplateId==g.Key).TemplateName
                };

            return templates;
        }
    }
}

using AutoMapper;
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
	[ApiController]
    [Route("template")]
    [Authorize(Roles = "NotificationManager")]
    public class TemplateController : ControllerBase
    {
        private readonly NotifDbContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="mapper"></param>
        public TemplateController(NotifDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Get All Templates
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IEnumerable<TemplateWIdDto>> Get() {
            var templates = await _context.Templates
                .Include(t => t.DefaultFromRecipient)
                .Include(t=>t.TemplateRecipients).ThenInclude(tr=>tr.Recipient)
                .ToListAsync();
            return templates.Select(s => _mapper.Map<TemplateWIdDto>(s));
        }

        /// <summary>
        /// Get Template by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TemplateWIdDto>> Get(Guid id)
        {
            Template templateDb = await _context.Templates
                .Include(t => t.DefaultFromRecipient)
                .Include(t => t.TemplateRecipients).ThenInclude(tr => tr.Recipient)
                .FirstOrDefaultAsync(p => p.TemplateId == id);
            if (null == templateDb) return NotFound();
            return _mapper.Map<TemplateWIdDto>(templateDb);
        }

        /*[HttpGet("{id}/recipient/{recipientId}")]
        public async Task<ActionResult<RecipientWIdDto>> Get(Guid id, long recipientId) {
            TemplateRecipient templateRecipient= await _context.TemplateRecipient.Include(tr => tr.Recipient).FirstOrDefaultAsync(tr => tr.TemplateId==id && tr.RecipientId==recipientId);
            if (null == templateRecipient) return NotFound();
            return _mapper.Map<RecipientWIdDto>(templateRecipient);
        }*/

        /// <summary>
        /// Update Template
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public virtual async Task<ActionResult<TemplateWIdDto>> Put(Guid id, [FromBody] TemplateDto templateDto)
        {
            if (null == templateDto) return BadRequest("templateDto can not be null");
            Template templateDb = await _context.Templates
                .Include(t=>t.DefaultFromRecipient)
                .Include(t=>t.TemplateRecipients).ThenInclude(tr=>tr.Recipient)
                .FirstOrDefaultAsync(t=> t.TemplateId== id);
            if (null == templateDb) return NotFound($"Template id {id} was not found");

            Recipient dbRecipeint = templateDb.DefaultFromRecipient;
            Recipient inRecipient = _mapper.Map<Recipient>(templateDto.DefaultFromRecipient);
            if (null != dbRecipeint && (dbRecipeint.Address != inRecipient?.Address || dbRecipeint.Name != inRecipient?.Name)) //we used to have recipient and it's not the same as new one
            {
                await _context.PossiblyDeleteRecipientForTemplateSender(dbRecipeint, id);
            }
            foreach (var trDb in templateDb.TemplateRecipients)
            {
                _context.TemplateRecipients.Remove(trDb);
                dbRecipeint = trDb.Recipient;
                if (!templateDto.DefaultRecipients.Any(t => t.TypeCode == trDb.TypeCode && t.Name == dbRecipeint.Name && t.Address == dbRecipeint.Address))
                {
                    await _context.PossiblyDeleteRecipientForTemplate(dbRecipeint, id);
                }
            }

            _mapper.Map(templateDto, templateDb); //copy all to Db-structure

            dbRecipeint = templateDb.DefaultFromRecipient;
            if (null != dbRecipeint)
            {
                 templateDb.DefaultFromRecipient = await _context.FindOrAddRecipientAsync(dbRecipeint);
            }
            foreach (var trDb in templateDb.TemplateRecipients)
            {
                trDb.Recipient = await _context.FindOrAddRecipientAsync(trDb.Recipient);
            }

            _context.Templates.Update(templateDb);
            await this._context.SaveChangesAsync();

            return await Get(id);
        }

        /// <summary>
        /// Create new Template
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<ActionResult<TemplateWIdDto>> Add([FromBody] TemplateDto template)
        {
            Template templateDb = _mapper.Map<Template>(template);
            Recipient sender = templateDb.DefaultFromRecipient;
            if (null != sender)
            {
                templateDb.DefaultFromRecipient = await _context.FindRecipientOrUseThis(sender);
            }
            foreach(var rec in templateDb.TemplateRecipients)
            {
                rec.Recipient = await _context.FindRecipientOrUseThis(rec.Recipient);
            }
            if (Guid.Empty == templateDb.TemplateId) templateDb.TemplateId = Guid.NewGuid();
            await _context.Templates.AddAsync(templateDb);
            await this._context.SaveChangesAsync();
            return await Get(templateDb.TemplateId);
        }

        /// <summary>
        /// Add a recipient to Template
        /// </summary>
        /// <param name="id"></param>
        /// <param name="recipient"></param>
        /// <returns></returns>
        [HttpPost("{id}/recipient")]
        public virtual async Task<ActionResult<RecipientWIdDto>> AddRecipient(Guid id, [FromBody] RecipientWTypeDto recipient)
        {
            TemplateRecipient tr;

            Recipient recipientDb = await _context.Recipients.FirstOrDefaultAsync(r => r.Address == recipient.Address && r.Name == recipient.Name);
            if (null == recipientDb)
            {
                recipientDb = _mapper.Map<Recipient>(recipient);
                await _context.Recipients.AddAsync(recipientDb);
            }
            else 
            {
                tr = _context.TemplateRecipients.FirstOrDefault(tr => tr.TemplateId == id && tr.RecipientId==recipientDb.RecipientId && tr.TypeCode == recipient.TypeCode);
                if (tr != null)
                {
                    return _mapper.Map<RecipientWIdDto>(tr);
                }
            }
            tr = new TemplateRecipient() { Recipient = recipientDb, TemplateId = id, TypeCode = recipient.TypeCode };
            await _context.TemplateRecipients.AddAsync(tr);
            await this._context.SaveChangesAsync();
            return _mapper.Map<RecipientWIdDto>(tr);
        }

        /// <summary>
        /// Delete Template by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var template = await _context.Templates.Include(t => t.TemplateRecipients).FirstOrDefaultAsync(t=>t.TemplateId==id);
            if (template == null)
            {
                NotFound($"Template with ID = {id} not found");
            }

            _context.TemplateRecipients.RemoveRange(template.TemplateRecipients);

            foreach (var tr in template.TemplateRecipients)
            {
                long recipientId = tr.RecipientId;
                if (!await _context.MessageRecipients.AnyAsync(p => p.RecipientId == recipientId) &&
                    !await _context.Messages.AnyAsync(p => p.FromRecipientId == recipientId) &&
                    !await _context.TemplateRecipients.AnyAsync(p => p.RecipientId == recipientId && p.TrId != tr.TrId) &&
                    !await _context.Templates.AnyAsync(p => p.DefaultFromRecipientId == recipientId))
                {
                    _context.Recipients.Remove(tr.Recipient);
                }
            }

            _context.Templates.Remove(template);

            long? senderId = template.DefaultFromRecipientId;
            if (senderId.HasValue &&
                !await _context.Messages.AnyAsync(p => p.FromRecipientId == senderId) &&
                !await _context.MessageRecipients.AnyAsync(p => p.RecipientId == senderId) &&
                !await _context.TemplateRecipients.AnyAsync(p => p.RecipientId == senderId) &&
                !await _context.Templates.AnyAsync(p => p.DefaultFromRecipientId == senderId && p.TemplateId != id))
            {
                _context.Recipients.Remove(template.DefaultFromRecipient);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Delete a Recipient 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="recipientId"></param>
        /// <param name="typeCode"></param>
        /// <returns></returns>
        [HttpDelete("{id}/recipient/{recipientId}/{typeCode}")]
        public async Task<ActionResult> Delete(Guid id, long recipientId, string typeCode)
        {
            var templateRecipient = await _context.TemplateRecipients.Include(t => t.Recipient).FirstOrDefaultAsync(tr=> tr.TemplateId==id && tr.RecipientId==recipientId && tr.TypeCode==typeCode);
            if (templateRecipient == null)
            {
                NotFound($"Template-Recipient with ID = {id}/{recipientId}/{typeCode} not found");
            }

            _context.TemplateRecipients.Remove(templateRecipient);
            if(!_context.TemplateRecipients.Any(tr=>tr.RecipientId== recipientId && tr.TrId!= templateRecipient.TrId)) //nobody else is refering to this recipient - delete it.
            {
                _context.Recipients.Remove(templateRecipient.Recipient);
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }

	}
}
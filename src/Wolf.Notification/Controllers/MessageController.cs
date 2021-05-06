using AutoMapper;
using DotLiquid;
using DotLiquid.Tags;
using DotLiquid.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Wolf.MessageQueue.Services;
using Wolf.Notification.Models;
using Wolf.Notification.Converters;
using Wolf.Notification.Database.Entities;
using Wolf.Notification.Exceptions;
using Wolf.Notification.Models;

namespace Wolf.Notification.Controllers
{
    [ApiController]
    [Route("message")]
    [Authorize("NotificationManagerRoleOrTrustedEnvAndClient")]//, Roles = "NotificationManager")]
    public class MessageController: ControllerBase
    {
        private readonly NotifDbContext _context;
        private readonly IQueueService _queueService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="queueService"></param>
        /// <param name="mapper"></param>
        public MessageController(NotifDbContext context, IQueueService queueService, IMapper mapper)
        {
            _context = context;
            _queueService = queueService;
            _mapper = mapper;
        }

        ///<summary>Returns the compiled body and subject of the message.</summary>
        ///<example>
        ///{
        ///    "tokenValues":{
        ///       "names":[
        ///          {
        ///             "name":"alex",
        ///             "client":"b"
        ///          },
        ///          {
        ///             "name":"c",
        ///             "client":"d"
        ///          }
        ///       ],
        ///       "client":{
        ///          "name":"aaaalex"
        ///      },
        ///       "device":"apple_macbook"
        ///    },
        ///    "recipients":[
        ///       "string"
        ///    ]
        /// }
        ///</example>
        /// <param name="templateId">Template ID (GUID)</param>
        /// <param name="provider">name of sending provider, like 'email' for example</param>
        /// <returns>Response with Message Body, Subject, Recipients and Provider (no ID)</returns>
        [HttpPost("{templateId}/preview")]
        public async Task<MessagePreviewDto> Preview([FromRoute][Required]Guid templateId, [FromBody]SendMessageRequest model)
        {
            return await CompileMessageAsync(templateId, model, false);
        }

        /// <summary>
        /// Returns the compiled body and subject of the message for a given Template Name.
        /// </summary>
        /// <param name="templateName">Template Name</param>
        /// <param name="provider">name of sending provider, like 'email' for example</param>
        /// <param name="model"></param>
        /// <returns>Response with Message Body, Subject, Recipients and Provider (no ID)</returns>
        [HttpPost("{templateName}/preview-by-name")]
        public async Task<MessagePreviewDto> PreviewByName([FromRoute][Required]string templateName, [FromBody]SendMessageRequest model)
        {
            var template= await FindTemplateOrThrow(templateName);
            return await Preview(template.TemplateId, model);
        }

        ///<summary>Compiles subject and body of message.await Saves to database. Posts event to queue</summary>
        /// <param name="templateId">Template ID (GUID)</param>
        /// <param name="provider">name of sending provider, like 'email' for example</param>
        /// <param name="model"></param>
        /// <returns>Response with Message Id, Body, Subject, Recipients and Provider</returns>
        [HttpPost("{templateId}/send")]
        public async Task<MessageDto> Send([FromRoute][Required]Guid templateId, [FromBody]SendMessageRequest model)
        {
            var compiledMessage = await CompileMessageAsync(templateId, model, true);

            if (string.IsNullOrWhiteSpace(compiledMessage.Body))
            {
                throw new Exceptions.IncorrectModelException($"Body cannot be empty");
            }
            if (string.IsNullOrWhiteSpace(compiledMessage.Subject))
            {
                throw new Exceptions.IncorrectModelException($"Subject cannot be empty");
            }
            if (null==compiledMessage.Sender)
            {
                throw new Exceptions.IncorrectModelException($"Sender cannot be empty");
            }

            Message message = new Message
            {
                TemplateId = compiledMessage.Template.TemplateId,
                ProviderCode = compiledMessage.Provider.ProviderCode,
                TokenData = compiledMessage.TokenData
            };
            message.FromRecipient = await GetOrAddDbRecipientAsync(compiledMessage.Sender);
            message.MessageRecipients = compiledMessage.Recipients.Select(r => AddDbMessageRecipient(r).Result).ToList();
            message.GeneratedMessage= _mapper.Map<GeneratedMessage>(compiledMessage);
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            await _queueService.AddMessageAsync(compiledMessage.Provider.ProviderCode, message.MessageId.ToString());
            message.DateProcessed = DateTime.Now;
            await _context.SaveChangesAsync();

            return GetMessage(message.MessageId);
        }

        ///<summary>Compiles subject and body of message.await Saves to database. Posts event to queue</summary>
        /// <param name="templateName">Template Name</param>
        /// <param name="provider">name of sending provider, like 'email' for example</param>
        /// <param name="model"></param>
        /// <returns>Response with Message Id, Body, Subject, Recipients and Provider</returns>
        [HttpPost("{templateName}/send-by-name")]
        public async Task<MessageDto> SendByName([FromRoute][Required]string templateName, [FromBody]SendMessageRequest model)
        {
            var template = await FindTemplateOrThrow(templateName);
            return await Send(template.TemplateId, model);
        }

        ///<summary>Returns list of messages by criteria</summary>
        [HttpPost("filter")]
        public async Task<FilterMessageResponse> Filter(FilterMessageRequest criteria, [FromQuery] int skip = 0, [FromQuery] int? take = null)
        {
            IQueryable<Message> query = GetMessageQuery();

            if (criteria.DateOfProcessingFrom.HasValue) query = query.Where(p => p.DateProcessed.HasValue && p.DateProcessed.Value >= criteria.DateOfProcessingFrom.Value);
            if (criteria.DateOfProcessingTo.HasValue) query = query.Where(p => p.DateProcessed.HasValue && p.DateProcessed.Value <= criteria.DateOfProcessingTo.Value);

            if (criteria.DateOfSendingFrom.HasValue) query = query.Where(p => p.DateSent.HasValue && p.DateSent.Value >= criteria.DateOfSendingFrom.Value);
            if (criteria.DateOfSendingTo.HasValue) query = query.Where(p => p.DateSent.HasValue && p.DateSent.Value <= criteria.DateOfSendingTo.Value);

            if (!string.IsNullOrEmpty(criteria.TemplateNameContains)) query = query.Where(p => p.Template.TemplateName.Contains(criteria.TemplateNameContains));
            if (!string.IsNullOrEmpty(criteria.SubjectContains)) query = query.Where(p => p.GeneratedMessage.Subject.Contains(criteria.SubjectContains));
            if (!string.IsNullOrEmpty(criteria.BodyContains)) query = query.Where(p => p.GeneratedMessage.Body.Contains(criteria.BodyContains));

            if (!string.IsNullOrEmpty(criteria.AllSearchContains)) query = query.Where(
                p => p.Template.TemplateName.Contains(criteria.AllSearchContains)
                || p.GeneratedMessage.Subject.Contains(criteria.AllSearchContains)
                || p.GeneratedMessage.Body.Contains(criteria.AllSearchContains)
                || p.MessageId.ToString().Contains(criteria.AllSearchContains)
                || p.FromRecipient.Name.Contains(criteria.AllSearchContains)
                || p.FromRecipient.Address.Contains(criteria.AllSearchContains)
                || p.MessageRecipients.Any(mr => mr.Recipient.Name.Contains(criteria.AllSearchContains) || mr.Recipient.Address.Contains(criteria.AllSearchContains))
            );

            bool IsSortedOnce = false;
            if (null != criteria.Sorting)
            {
                foreach (var fld in criteria.Sorting)
                {
                    query = fld.Field switch
                    {
                        SortField.DateCreated => AddSortingField(query, fld.Order, p => p.DateCreated, ref IsSortedOnce),
                        SortField.DateProcessed => AddSortingField(query, fld.Order, p => p.DateProcessed, ref IsSortedOnce),
                        SortField.DateSent => AddSortingField(query, fld.Order, p => p.DateSent, ref IsSortedOnce),
                        SortField.Id => AddSortingField(query, fld.Order, p => p.MessageId, ref IsSortedOnce),
                        SortField.SenderAddress => AddSortingField(query, fld.Order, p => p.FromRecipient.Address, ref IsSortedOnce),
                        SortField.SenderName => AddSortingField(query, fld.Order, p => p.FromRecipient.Name, ref IsSortedOnce),
                        SortField.Subject => AddSortingField(query, fld.Order, p => p.GeneratedMessage.Subject, ref IsSortedOnce),
                        SortField.TemplateName => AddSortingField(query, fld.Order, p => p.Template.TemplateName, ref IsSortedOnce),
                        _ => throw new NotImplementedException($"field {fld.Field} is not sortable")
                    };
                }
            }
            var modelOut = new FilterMessageResponse { TotalCount = await query.CountAsync() };
            if (!IsSortedOnce)
            {
                query = query.OrderByDescending(q => q.DateCreated);
            }

            query = query.Skip(skip);
            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            List<Message> messages = query.ToList();
            modelOut.Items = _mapper.Map<IEnumerable<MessageDto>>(messages);
            return modelOut;
        }

        ///<summary>Returns message by ID</summary>
        [HttpGet("{id}")]
        public MessageDto GetMessage([FromRoute]Guid id)
        {
            var message=GetMessageQuery().FirstOrDefault(m => m.MessageId == id);

            if (null == message) throw new Exceptions.NotFoundException($"Message {id} was not found");

            return _mapper.Map<MessageDto>(message);
        }

        /// <summary>
        /// Sets the DateSent on a message
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}/complete")]
        public async Task<MessageDto> SetDateOfSending([FromRoute]Guid id, [FromBody]UpdateMessageRequest model)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(p => p.MessageId == id);
            if (message == null)
            {
                throw new Exceptions.NotFoundException($"Message with ID = {id} not found");
            }
            message.DateSent = model.DateOfSending;
            _context.Messages.Update(message);
            _context.SaveChanges();
            return GetMessage(message.MessageId);
        }

        /// <summary>
        /// Delete message and all unreferenced depenent objects by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var message = GetMessageQuery().FirstOrDefault(t => t.MessageId == id);
            if (message == null)
            {
                NotFound($"Message with ID = {id} not found");
            }

            _context.MessageRecipients.RemoveRange(message.MessageRecipients);
            foreach (var mr in message.MessageRecipients)
            {
                long recipientId = mr.RecipientId;
                if (!await _context.MessageRecipients.AnyAsync(p => p.RecipientId == recipientId && p.MrId != mr.MrId) &&
                    !await _context.Messages.AnyAsync(p => p.FromRecipientId == recipientId) &&
                    !await _context.TemplateRecipients.AnyAsync(p => p.RecipientId == recipientId) &&
                    !await _context.Templates.AnyAsync(p => p.DefaultFromRecipientId == recipientId))
                {
                    _context.Recipients.Remove(mr.Recipient);
                }
            }

            _context.GeneratedMessages.Remove(message.GeneratedMessage);
            _context.Messages.Remove(message);

            long senderId = message.FromRecipientId;
            if (!await _context.Messages.AnyAsync(p => p.FromRecipientId == senderId && p.MessageId != id) && 
                !await _context.MessageRecipients.AnyAsync(p => p.RecipientId == senderId) &&
                !await _context.TemplateRecipients.AnyAsync(p => p.RecipientId == senderId) &&
                !await _context.Templates.AnyAsync(p => p.DefaultFromRecipientId == senderId))
            {
                _context.Recipients.Remove(message.FromRecipient);
            }
            
            await _context.SaveChangesAsync();
            return NoContent();
        }

        #region private
        private IIncludableQueryable<Message, GeneratedMessage> GetMessageQuery()
        {
            return _context.Messages
                .Include(m => m.ProviderCodeNavigation)
                .Include(m => m.Template)
                .Include(m => m.FromRecipient)
                .Include(m => m.MessageRecipients).ThenInclude(mr => mr.Recipient)
                .Include(m => m.GeneratedMessage);
        }

        private IQueryable<Message> AddSortingField<TKey>(IQueryable<Message> query, SortOrder sortOrder, Expression<Func<Message, TKey>> keySelector, ref bool IsSortedOnce)
        {
            if (IsSortedOnce && query is IOrderedQueryable<Message> orderedQuery)
            {
                return sortOrder == SortOrder.Asc ? orderedQuery.ThenBy(keySelector) : orderedQuery.ThenByDescending(keySelector);
            }
            else
            {
                IsSortedOnce = true;
                return sortOrder == SortOrder.Asc ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
            }
        }

        private async Task<MessagePreviewDto> CompileMessageAsync(Guid templateId, SendMessageRequest messageRequest, bool shoulHaveAllFieldsForSending)
        {
            var template = await _context.Templates
                .Include(t => t.DefaultFromRecipient)
                .Include(t=>t.TemplateRecipients).ThenInclude(tr=>tr.Recipient)
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);
            if (template == null)
            {
                throw new Exceptions.NotFoundException($"Template with ID = {templateId} not found");
            }

            if (null == messageRequest) messageRequest = new SendMessageRequest(); //this is just to minimize checking for nulls every time

            //sender
            RecipientDto sender = messageRequest.Sender;
            if (sender == null)
            {
                sender = _mapper.Map<RecipientWIdDto>(template.DefaultFromRecipient);
            }
            //recipients
            IEnumerable<RecipientWTypeDto> recipients = messageRequest.Recipients;
            if (recipients == null || recipients.Count() == 0)
            {
                recipients = _mapper.Map<IEnumerable<RecipientWTypeDto>>(template.TemplateRecipients);
                if (shoulHaveAllFieldsForSending && (recipients == null || recipients.Count() == 0))
                {
                    throw new Exceptions.NullModelException("Recipients are not specified and template does not have defaults");
                }
            }
            if (shoulHaveAllFieldsForSending)
            {
                foreach (var r in recipients)
                {
                    if (string.IsNullOrEmpty(r.Address))
                    {
                        throw new Exceptions.IncorrectModelException($"recipent Address is empty");
                    }
                    if (string.IsNullOrEmpty(r.TypeCode))
                    {
                        throw new Exceptions.IncorrectModelException($"recipent TypeCode is empty");
                    }
                }
            }

            //provider
            string providerCode = messageRequest.ProviderCode;
            if (string.IsNullOrWhiteSpace(providerCode))
            {
                providerCode = template.DefaultProviderCode;
            }
            if (shoulHaveAllFieldsForSending && string.IsNullOrWhiteSpace(providerCode))
            {
                throw new Exceptions.NullModelException("Provider is not specified and template does not have a default provider");
            }
            var provider = await _context.Providers.FindAsync(providerCode);
            if (shoulHaveAllFieldsForSending && null == provider)
            {
                throw new Exceptions.NotFoundException($"Provider code [{providerCode}] was not found");
            }

            string tokenValuesStr = null;
            Hash jsonHash;
            if (null == messageRequest.TokenValues)
            {
                jsonHash = new Hash();
            }
            else
            {
                tokenValuesStr = JsonConvert.SerializeObject(messageRequest.TokenValues);
                //hach to use dynamic object from the request
                IDictionary<string, object> jsonDict = JsonConvert.DeserializeObject<IDictionary<string, object>>(tokenValuesStr, new DictionaryConverter());
                jsonHash = Hash.FromDictionary(jsonDict);
            }

            var tSubject = DotLiquid.Template.Parse(template.TemplateSubject);
            var subject = tSubject.Render(jsonHash);

            var tBody = DotLiquid.Template.Parse(template.TemplateBody);
            var body = tBody.Render(jsonHash);

            return new MessagePreviewDto
            {
                Provider = _mapper.Map<ProviderDto>(provider),
                Template = _mapper.Map<TemplateIdNameDto>(template),
                TokenData = tokenValuesStr,
                Sender = sender,
                Recipients = recipients,
                Subject = subject,
                Body = body
            };
        }

        private async Task<Database.Entities.Template> FindTemplateOrThrow(string templateName)
        {
            var template = await _context.Templates.FirstOrDefaultAsync(w => w.TemplateName == templateName);
            if (null == template)
            {
                throw new NotFoundException($"Template '{templateName}' not found.");
            }
            return template;
        }

        private async Task<Recipient> GetOrAddDbRecipientAsync(RecipientDto recipient)
        {
            Recipient recipientDb = await _context.Recipients.FirstOrDefaultAsync(r => r.Address == recipient.Address && r.Name == recipient.Name);
            if (null == recipientDb)
            {
                recipientDb = _mapper.Map<Recipient>(recipient);
                _context.Recipients.Add(recipientDb);
            }
            return recipientDb;
        }

        private async Task<MessageRecipient> AddDbMessageRecipient(RecipientWTypeDto recipient)
        {
            MessageRecipient recipientDb = _mapper.Map<MessageRecipient>(recipient);
            recipientDb.Recipient = await GetOrAddDbRecipientAsync(recipient);
            _context.MessageRecipients.Add(recipientDb);
            return recipientDb;

        }
		#endregion
	}
}
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wolf.MessageQueue.Services;
using Wolf.Notification.Controllers;
using Wolf.Notification.Database.Entities;
using Wolf.Notification.Models;
using Xunit;

namespace Wolf.Notification.Tests
{
	public class MessageControllerTest: IDisposable
    {
        private readonly MessageController _messageController;

        private readonly NotifDbContext _dbContext;

        private readonly Mock<IQueueService> _mockedQueueService;

        private Guid _templateId = Guid.NewGuid();

        private string TemplateName { get { return "TestTemplate_" + _templateId; } }

        private readonly string _provider = "email";


        public MessageControllerTest()
        {
            var options = new DbContextOptionsBuilder<NotifDbContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;

            _dbContext = new NotifDbContext(options);
            _dbContext.Database.EnsureCreated();

            _mockedQueueService = new Mock<IQueueService>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            var mapper = config.CreateMapper();

            _messageController = new MessageController(_dbContext, _mockedQueueService.Object, mapper);

            _dbContext.Providers.Add(new Provider { ProviderCode = "email" });
            _dbContext.RecipientTypes.AddRange(new RecipientType { TypeCode = "to" }, new RecipientType { TypeCode = "cc" }, new RecipientType { TypeCode = "bcc" });
            _dbContext.Add(new Template
            {
                TemplateId = _templateId,
                TemplateName = TemplateName,
                DefaultProviderCode = "email",
                DefaultFromRecipient= new Recipient { Address="sender@email.com"},
                TemplateRecipients = new TemplateRecipient[]{
                    new TemplateRecipient { TypeCode = "to", Recipient = new Recipient { Address = "default1@email.com" } },
                    new TemplateRecipient { TypeCode = "cc", Recipient = new Recipient { Address = "default2@email.com" } }
                },
                TemplateSubject = "Notification for {{username}}",
                TemplateBody = "<b>Hello {{username}},</b> We have the following applications:<ul>{% for app in apps %}<li>{{app.name}} with ID = {{app.ID}}</li>{% endfor %}</ul>"
            });
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task Preview_should_render_subject_body_recipients()
        {
            var request = new SendMessageRequest
            {
                Recipients = new RecipientWTypeDto[] { new RecipientWTypeDto { TypeCode = "to", Address = "recipient1@email.com" } },
                TokenValues = new Dictionary<string, object>()
            };

            request.TokenValues.Add("username", "testuser1");
            request.TokenValues.Add("apps", new List<dynamic>() {
                new {
                    name = "Wolf Forms",
                    ID = 111
                },

                new {
                    name = "Wolf Flows",
                    ID = 222
                },

                new {
                    name = "Wolf Notifications",
                    ID = 333
                },
            });

            var response = await this._messageController.Preview(_templateId, request);

            Assert.Equal("Notification for testuser1", response.Subject);
            Assert.Equal(request.Recipients, response.Recipients);
            Assert.Equal("<b>Hello testuser1,</b> We have the following applications:<ul><li>Wolf Forms with ID = 111</li><li>Wolf Flows with ID = 222</li><li>Wolf Notifications with ID = 333</li></ul>"
                , response.Body);
        }

        [Fact]
        public async Task Preview_should_return_message_with_default_recipients()
        {
            var request = new SendMessageRequest
            {
                TokenValues = new Dictionary<string, object>()
            };

            request.TokenValues.Add("username", "testuser1");
            var response = await this._messageController.Preview(_templateId, request);

            var expectedStr = JsonConvert.SerializeObject(new List<RecipientWTypeDto>() { new RecipientWTypeDto { TypeCode = "to", Address = "default1@email.com" }, new RecipientWTypeDto { TypeCode = "cc", Address = "default2@email.com" } });
            var actualStr = JsonConvert.SerializeObject(response.Recipients);
            Assert.Equal(expectedStr, actualStr);
        }

        [Fact]
        public async Task Preview_should_return_NotFoundException()
        {
            try
            {
                var request = new SendMessageRequest
                {
                    TokenValues = new Dictionary<string, object>()
                };

                request.TokenValues.Add("username", "testuser1");
                var response = await this._messageController.Preview(Guid.NewGuid(), request);

            }
            catch (Exception nfe)
            {
                Assert.Equal(typeof(Exceptions.NotFoundException), nfe.GetType());
            }
        }

        [Fact]
        public async Task PreviewByname_should_return_NotFoundException()
        {
            try
            {
                var request = new SendMessageRequest
                {
                    TokenValues = new Dictionary<string, object>()
                };

                request.TokenValues.Add("username", "testuser1");
                var response = await this._messageController.PreviewByName("Non-ExistingName", request);

            }
            catch (Exception nfe)
            {
                Assert.Equal(typeof(Exceptions.NotFoundException), nfe.GetType());
                Assert.Contains("Template 'Non-ExistingName' not found", nfe.Message);
            }
        }

        [Fact]
        public async Task Preview_should_return_NullModelException()
        {
            try
            {
                var request = new SendMessageRequest
                {
                    TokenValues = new Dictionary<string, object>()
                };

                request.TokenValues.Add("username", "testuser1");
                var response = await this._messageController.Preview(Guid.NewGuid(), request);

            }
            catch (Exception nme)
            {
                Assert.Equal(typeof(Exceptions.NotFoundException), nme.GetType());
            }
        }

        [Fact]
        public async Task Send_should_return_IncorrectModelException_for_empty_Body()
        {
            try
            {
                var templateId = Guid.NewGuid();
                this._dbContext.Add(new Template
                {
                    TemplateId = templateId,
                    TemplateSubject = "Notification for {{username}}",
                });
                this._dbContext.SaveChanges();

                var request = new SendMessageRequest
                {
                    TokenValues = new Dictionary<string, object>(),
                    Recipients = new RecipientWTypeDto[] { new RecipientWTypeDto { TypeCode = "to", Address = "recipient@email.com" } },
                    ProviderCode="email"
                };

                request.TokenValues.Add("username", "testuser1");
                var response = await this._messageController.Send(templateId, request);

            }
            catch (Exception ime)
            {
                Assert.Equal(typeof(Exceptions.IncorrectModelException), ime.GetType());
                Assert.Equal("Body cannot be empty", ime.Message);
            }
        }

        [Fact]
        public async Task Send_should_return_IncorrectModelException_for_empty_Subject()
        {
            try
            {
                var templateId = Guid.NewGuid();
                this._dbContext.Add(new Template
                {
                    TemplateId = templateId,
                    TemplateBody = "test body 1"
                });
                this._dbContext.SaveChanges();

                var request = new SendMessageRequest
                {
                    TokenValues = new Dictionary<string, object>(),
                    Recipients = new RecipientWTypeDto[] { new RecipientWTypeDto { Address = "recipient@email.com", TypeCode="cc" } },
                    ProviderCode = "email"
                };

                request.TokenValues.Add("username", "testuser1");
                var response = await this._messageController.Send(templateId, request);

            }
            catch (Exception ime)
            {
                Assert.Equal(typeof(Exceptions.IncorrectModelException), ime.GetType());
                Assert.Equal("Subject cannot be empty", ime.Message);
            }
        }

        [Fact]
        public async Task Send_should_return_save_message_to_database_and_return()
        {
            var provider = "email";
            var request = new SendMessageRequest
            {
                Recipients = new RecipientWTypeDto[] { new RecipientWTypeDto { Address = "recipient1@email.com", TypeCode="bcc" } },
                TokenValues = new Dictionary<string, object>()
            };

            request.TokenValues.Add("username", "testuser1");
            request.TokenValues.Add("apps", new List<dynamic>() {
                new {
                    name = "Wolf Forms",
                    ID = 111
                },

                new {
                    name = "Wolf Flows",
                    ID = 222
                },

                new {
                    name = "Wolf Notifications",
                    ID = 333
                },
            });

            var response = await this._messageController.Send(this._templateId, request);

            var messageFromDB = await this._dbContext.Messages.FirstOrDefaultAsync(p => p.MessageId == response.MessageId);

            Assert.Equal("Notification for testuser1", messageFromDB.GeneratedMessage.Subject);
            Assert.Equal("<b>Hello testuser1,</b> We have the following applications:<ul><li>Wolf Forms with ID = 111</li><li>Wolf Flows with ID = 222</li><li>Wolf Notifications with ID = 333</li></ul>"
                , messageFromDB.GeneratedMessage.Body);
            Assert.Equal(provider, messageFromDB.ProviderCode);
            Assert.Equal(this._templateId, messageFromDB.TemplateId);
            Assert.NotNull(messageFromDB.DateProcessed);
        }

        [Fact]
        public async Task Send_should_send_mesage_id_to_queue()
        {
            string actualProvider = string.Empty;
            string actualMessageId = string.Empty;
            this._mockedQueueService.Setup(h => h.AddMessageAsync(
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Callback<string, string>(
            (p, m) =>
            {
                actualProvider = p;
                actualMessageId = m;
            });

            var request = new SendMessageRequest
            {
                Recipients = new RecipientWTypeDto[] { new RecipientWTypeDto { Address = "recipient2@email.com", TypeCode="cc" } },
                TokenValues = new Dictionary<string, object>()
            };
            request.TokenValues.Add("username", "testuser1");

            var response = await this._messageController.Send(_templateId, request);

            Assert.Equal(response.MessageId.ToString(), actualMessageId);
            Assert.Equal(this._provider, actualProvider);
        }

        [Fact]
        public async Task Send_should_send_mesage_id_to_default_provider()
        {
            string actualProvider = string.Empty;
            string actualMessageId = string.Empty;
            this._mockedQueueService.Setup(h => h.AddMessageAsync(
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Callback<string, string>(
            (p, m) =>
            {
                actualProvider = p;
                actualMessageId = m;
            });

            var request = new SendMessageRequest
            {
                Recipients = new RecipientWTypeDto[] { new RecipientWTypeDto { Address = "recipient2@email.com", TypeCode="to" } },
                TokenValues = new Dictionary<string, object>()
            };
            request.TokenValues.Add("username", "testuser1");

            var response = await this._messageController.Send(this._templateId, request);

            Assert.Equal(response.MessageId.ToString(), actualMessageId);
            Assert.Equal("email", actualProvider);
        }


        [Fact]
        public async Task SendByName_should_return_NotFoundException_for_non_existing_template()
        {
            try
            {
                var templateName = "Non-Existing";
                var request = new SendMessageRequest
                {
                    TokenValues = new Dictionary<string, object>(),
                    Recipients = new RecipientWTypeDto[] { new RecipientWTypeDto { Address = "recipient@email.com" } }
                };

                request.TokenValues.Add("username", "testuser1");
                var response = await this._messageController.SendByName(templateName, request);

            }
            catch (Exception ime)
            {
                Assert.Equal(typeof(Exceptions.NotFoundException), ime.GetType());
                Assert.Equal("Template 'Non-Existing' not found.", ime.Message);
            }
        }

        [Fact]
        public async Task SendByName_should_return_save_message_to_database_and_return()
        {
            var provider = "email";
            var request = new SendMessageRequest
            {
                Recipients = new RecipientWTypeDto[] { new RecipientWTypeDto { Address = "recipient1@email.com", TypeCode="bcc" } },
                TokenValues = new Dictionary<string, object>()
            };

            request.TokenValues.Add("username", "testuser1");
            request.TokenValues.Add("apps", new List<dynamic>() {
                new {
                    name = "Wolf Forms",
                    ID = 111
                },

                new {
                    name = "Wolf Flows",
                    ID = 222
                },

                new {
                    name = "Wolf Notifications",
                    ID = 333
                },
            });

            var response = await this._messageController.SendByName(TemplateName, request);

            var messageFromDB = await this._dbContext.Messages.FirstOrDefaultAsync(p => p.MessageId == response.MessageId);

            Assert.Equal("Notification for testuser1", messageFromDB.GeneratedMessage.Subject);
            Assert.Equal("<b>Hello testuser1,</b> We have the following applications:<ul><li>Wolf Forms with ID = 111</li><li>Wolf Flows with ID = 222</li><li>Wolf Notifications with ID = 333</li></ul>"
                , messageFromDB.GeneratedMessage.Body);
            Assert.Equal(provider, messageFromDB.ProviderCode);
            Assert.Equal(this._templateId, messageFromDB.TemplateId);
            Assert.NotNull(messageFromDB.DateProcessed);
        }


        [Fact]
        public async Task GetMessage_should_return_sent_message_by_id()
        {
            var request = new SendMessageRequest
            {
                Recipients = new RecipientWTypeDto[] { new RecipientWTypeDto { Address = "recipient3@email.com", TypeCode="to" } },
                TokenValues = new Dictionary<string, object>()
            };
            request.TokenValues.Add("username", "testuser1");
            var sendMessage = await this._messageController.Send(_templateId, request);
            var response = this._messageController.GetMessage(sendMessage.MessageId);

            Assert.Equal(sendMessage.MessageId, response.MessageId);
            var expectedStr = JsonConvert.SerializeObject(sendMessage.Recipients);
            var actualStr = JsonConvert.SerializeObject(response.Recipients);
            Assert.Equal(expectedStr, actualStr);
            Assert.Equal(sendMessage.Body, response.Body);
            Assert.Equal(sendMessage.Subject, response.Subject);

        }

        [Fact]
        public async Task SetDateOfSending_should_set_date()
        {
            var request = new SendMessageRequest
            {
                Recipients = new RecipientWTypeDto[] { new RecipientWTypeDto { Address = "recipient3@email.com", TypeCode="to" } },
                TokenValues = new Dictionary<string, object>()
            };
            request.TokenValues.Add("username", "testuser1");
            var sentMessage = await this._messageController.Send(_templateId, request);

            var dateOfSending = DateTime.UtcNow;
            await this._messageController.SetDateOfSending(sentMessage.MessageId, new UpdateMessageRequest
            {
                DateOfSending = dateOfSending
            });

            var updatedMessage = await this._dbContext.Messages.FirstOrDefaultAsync(p => p.MessageId == sentMessage.MessageId);
            Assert.Equal(dateOfSending, updatedMessage.DateSent);
        }

        [Fact]
        public async Task SetDateOfSending_should_return_NotFoundException()
        {
            var id = Guid.NewGuid();
            try
            {
                var dateOfSending = DateTime.UtcNow;
                await this._messageController.SetDateOfSending(id, new UpdateMessageRequest
                {
                    DateOfSending = dateOfSending
                });
            }
            catch (Exception nfe)
            {
                Assert.Equal(typeof(Exceptions.NotFoundException), nfe.GetType());
                Assert.Equal($"Message with ID = {id} not found", nfe.Message);
            }

        }

        public void Dispose()
        {
            if(null!=_dbContext)
                _dbContext.Dispose();
        }
    }
}

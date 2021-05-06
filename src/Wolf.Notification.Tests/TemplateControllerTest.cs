using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wolf.Notification.Controllers;
using Wolf.Notification.Database.Entities;
using Wolf.Notification.Models;
using Xunit;

namespace Wolf.Notification.Tests
{
	public class TemplateControllerTest
    {
        private readonly TemplateController _templateController;

        private readonly NotifDbContext _dbContext;

        private readonly IMapper _mapper;

        public TemplateControllerTest()
        {
            var options = new DbContextOptionsBuilder<NotifDbContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;

            _dbContext = new NotifDbContext(options);
            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            _mapper = config.CreateMapper();
            this._templateController = new TemplateController(_dbContext, _mapper);
        }

        [Fact]
        public async Task Post_should_add_template_to_set()
        {
            var templateResponse = await this._templateController.Add(new TemplateDto
            {
                TemplateBody = "TestBody1",
                TemplateName = "TestName1",
                TemplateSubject = "TestSubject1"
            });
            var createdTemplate = templateResponse.Value;
            var template = await this._dbContext.Set<Template>().FirstOrDefaultAsync(p => p.TemplateId == createdTemplate.TemplateId);

            Assert.Equal(createdTemplate.TemplateId, template.TemplateId);
            Assert.Equal(createdTemplate.TemplateBody, template.TemplateBody);
            Assert.Equal(createdTemplate.TemplateName, template.TemplateName);
            Assert.Equal(createdTemplate.TemplateSubject, template.TemplateSubject);
        }

        [Fact]
        public async Task Put_should_return_updated_entity()
        {
            var templateResponse = await this._templateController.Add(new TemplateDto
            {
                TemplateBody = "TestBody2",
                TemplateName = "TestName2",
                TemplateSubject = "TestSubject2"
            });
            var createdTemplate = templateResponse.Value;

            createdTemplate.TemplateBody = "UpdatedBody";
            createdTemplate.TemplateSubject = "UpdatedSubject";
            createdTemplate.TemplateName = "UpdatedName";

            templateResponse = await this._templateController.Put(createdTemplate.TemplateId, _mapper.Map<TemplateDto>(createdTemplate));
            var updatedTemplate = templateResponse.Value;

            Assert.Equal(createdTemplate.TemplateId, updatedTemplate.TemplateId);
            Assert.Equal(createdTemplate.TemplateBody, updatedTemplate.TemplateBody);
            Assert.Equal(createdTemplate.TemplateName, updatedTemplate.TemplateName);
            Assert.Equal(createdTemplate.TemplateSubject, updatedTemplate.TemplateSubject);
        }

        [Fact]
        public async Task Put_should_update_entity_except_id()
        {
            var templateResponse = await this._templateController.Add(new TemplateDto
            {
                TemplateBody = "TestBody2",
                TemplateName = "TestName2",
                TemplateSubject = "TestSubject2"
            });
            var createdTemplate = templateResponse.Value;

            createdTemplate.TemplateBody = "UpdatedBody";
            createdTemplate.TemplateSubject = "UpdatedSubject";
            createdTemplate.TemplateName = "UpdatedName";

            await this._templateController.Put(createdTemplate.TemplateId, _mapper.Map<TemplateDto>(createdTemplate));

            var updatedTemplate = await this._dbContext.Set<Template>().FirstOrDefaultAsync(p => p.TemplateId == createdTemplate.TemplateId);

            Assert.Equal(createdTemplate.TemplateId, updatedTemplate.TemplateId);
            Assert.Equal(createdTemplate.TemplateBody, updatedTemplate.TemplateBody);
            Assert.Equal(createdTemplate.TemplateName, updatedTemplate.TemplateName);
            Assert.Equal(createdTemplate.TemplateSubject, updatedTemplate.TemplateSubject);
        }

        [Fact]
        public async Task Get_should_return_all_entities()
        {
            var count = 10;
            var expected = new List<TemplateDto>();

            for (var i = 0; i < count; i++)
            {
                var tmp = new TemplateDto
                {
                    TemplateBody = "TestBody" + i,
                    TemplateName = "TestName" + i,
                    TemplateSubject = "TestSubject" + i,
                    DefaultRecipients=new List<RecipientWTypeDto>()
                };
                expected.Add(tmp);
                await this._templateController.Add(tmp);
            }
            var getResult = await this._templateController.Get();
            var actual = _mapper.Map<IEnumerable<TemplateDto>>(getResult);
            var expectedStr = JsonConvert.SerializeObject(expected);
            var actualStr = JsonConvert.SerializeObject(actual);
            Assert.Equal(expectedStr, actualStr);
        }

        [Fact]
        public async Task Get_should_return_specific_entity()
        {
            var count = 5;
            var expected = new List<TemplateDto>();
            TemplateWIdDto secondTemplate=null;

            for (var i = 0; i < count; i++)
            {
                var tmp = new TemplateDto
                {
                    TemplateBody = "TestBody" + i,
                    TemplateName = "TestName" + i,
                    TemplateSubject = "TestSubject" + i
                };
                expected.Add(tmp);
                var templateAdded=await this._templateController.Add(tmp);
                if (i == 1) secondTemplate = templateAdded.Value;
            }
            var actual = (await this._templateController.Get(secondTemplate.TemplateId)).Value;

            var expectedStr = JsonConvert.SerializeObject(secondTemplate);
            var actualStr = JsonConvert.SerializeObject(actual);
            Assert.Equal(expectedStr, actualStr);
        }

        [Fact]
        public async Task Get_should_return_404()
        {
            var actual = await this._templateController.Get(Guid.NewGuid());

            Assert.Null(actual.Value);
            Assert.IsType<NotFoundResult>(actual.Result);
        }
    }
}

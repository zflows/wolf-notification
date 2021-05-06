using System;
using System.Collections.Generic;

namespace Wolf.Notification.Models
{
	public class TemplateBaseDto
    {
        public string TemplateName { get; set; }
        public string TemplateSubject { get; set; }
        public string TemplateBody { get; set; }
        public string DefaultProviderCode { get; set; }
    }

    public class TemplateDto : TemplateBaseDto
    {
        public RecipientDto DefaultFromRecipient { get; set; }
        public ICollection<RecipientWTypeDto> DefaultRecipients { get; set; }
    }

    public class TemplateWIdDto : TemplateBaseDto
    {
        public Guid TemplateId { get; set; }

        public RecipientWIdDto DefaultFromRecipient { get; set; }

        public ICollection<RecipientWIdTypeDto> DefaultRecipients { get; set; }
    }

    public class TemplateIdNameDto
    {
        public Guid TemplateId { get; set; }
        public string TemplateName { get; set; }
    }
}
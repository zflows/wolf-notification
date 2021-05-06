using System;
using System.Collections.Generic;

namespace Wolf.Notification.Models
{
	public class SendMessageRequest
    {
        /// <summary>
        /// Optional dictionary of values to replace in the template
        /// </summary>
        public Dictionary<string, object> TokenValues { get; set; }

        /// <summary>
        /// Optional notification provider code. If not specified the one in template is used. If the field is not specified in the template an error is thrown.
        /// </summary>
        public string ProviderCode { get; set; }

        /// <summary>
        /// Optional Sender name/address. If not specified the one in template is used. If the field is not specified in the template an error is thrown.
        /// </summary>
        public RecipientDto Sender { get; set; }

        /// <summary>
        /// Optional list of Recipients. If not specified the one in template is used. If the field is not specified in the template an error is thrown.
        /// </summary>
        public IEnumerable<RecipientWTypeDto> Recipients { get; set; }
    }

    public class FilterMessageRequest
    {
        public string TemplateNameContains { get; set; }

        public string SubjectContains { get; set; }

        public string BodyContains { get; set; }

        public string AllSearchContains { get; set; }

        public DateTime? DateOfProcessingFrom { get; set; }

        public DateTime? DateOfProcessingTo { get; set; }

        public DateTime? DateOfSendingFrom { get; set; }

        public DateTime? DateOfSendingTo { get; set; }

        public IEnumerable<SortCriteria> Sorting { get; set; }
    }

    public class FilterMessageResponse
	{
        public IEnumerable<MessageDto> Items { get; set; }
        public int TotalCount { get; set; }
    }

    public class UpdateMessageRequest
    {
        public DateTime DateOfSending { get; set; }
    }

}
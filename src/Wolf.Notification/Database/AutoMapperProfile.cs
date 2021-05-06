using AutoMapper;
using Wolf.Notification.Database.Entities;

namespace Wolf.Notification.Models
{
	public class AutoMapperProfile:Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<Template, TemplateDto>()
				.ForMember(dest => dest.DefaultRecipients, opt => opt.MapFrom(src => src.TemplateRecipients))
				.ReverseMap();
				//.ForMember(dest => dest.TemplateId, opt => opt.MapFrom(src=> Guid.NewGuid()));
			CreateMap<Template, TemplateWIdDto>()
				.ForMember(dest => dest.DefaultRecipients, opt => opt.MapFrom(src => src.TemplateRecipients))
				.ReverseMap();
			CreateMap<Template, TemplateIdNameDto>().ReverseMap();
			CreateMap<TemplateWIdDto, TemplateDto>();

			CreateMap<Recipient, RecipientDto>().ReverseMap();
			CreateMap<Recipient, RecipientWIdDto>().ReverseMap();
			CreateMap<Recipient, RecipientWTypeDto>().ReverseMap();
			//CreateMap<RecipientWTypeDto, RecipientWIdTypeDto>();
			CreateMap<TemplateRecipient, RecipientWIdDto>()
				.ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Recipient.Address))
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Recipient.Name))
				.ForMember(dest => dest.RecipientId, opt => opt.MapFrom(src => src.Recipient.RecipientId))
				.ReverseMap();
			CreateMap<TemplateRecipient, RecipientWTypeDto>()
				.ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Recipient.Address))
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Recipient.Name))
				.ForMember(dest => dest.TypeCode, opt => opt.MapFrom(src => src.TypeCode))
				.ReverseMap();
			CreateMap<TemplateRecipient, RecipientWIdTypeDto>()
				.ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Recipient.Address))
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Recipient.Name))
				.ForMember(dest => dest.RecipientId, opt => opt.MapFrom(src => src.Recipient.RecipientId))
				.ForMember(dest => dest.TypeCode, opt => opt.MapFrom(src => src.TypeCode))
				.ReverseMap();

			CreateMap<MessageRecipient, RecipientWTypeDto>()
				.ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Recipient.Address))
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Recipient.Name))
				.ReverseMap();

			CreateMap<Provider, ProviderDto>();

			CreateMap<MessagePreviewDto, GeneratedMessage>();

			CreateMap<Message, MessageDto>()
				.ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src.ProviderCodeNavigation))
				.ForMember(dest => dest.Sender, opt => opt.MapFrom(src => src.FromRecipient))
				.ForMember(dest => dest.Recipients, opt => opt.MapFrom(src => src.MessageRecipients))
				.ForMember(dest => dest.Subject, opt => opt.MapFrom(src => src.GeneratedMessage == null ? null : src.GeneratedMessage.Subject))
				.ForMember(dest => dest.Body, opt => opt.MapFrom(src => src.GeneratedMessage == null ? null : src.GeneratedMessage.Body)); // LimitString(src.GeneratedMessage.Body,1024)));
		}

		private static string LimitString(string sIn, int maxLen)
		{
			if (null == sIn || sIn.Length <= maxLen) return sIn;
			return sIn.Substring(0, maxLen);
		}
	}
}

using Application.DTOs.Requests.Tags;
using Application.DTOs.Responses.Tags;
using Domain.Entities;
using Mapster;

namespace Application.Mappings
{
	public class TagMapping : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<Tag, GetResponse>()
				.Map(dest => dest.TagId, src => src.TagId)
				.Map(dest => dest.TagName, src => src.TagName)
				.Map(dest => dest.Note, src => src.Note);

			config.NewConfig<CreateRequest, Tag>()
				.Map(dest => dest.TagName, src => src.TagName)
				.Map(dest => dest.Note, src => src.Note);

			config.NewConfig<UpdateRequest, Tag>()
				.IgnoreNullValues(true)
				.Map(dest => dest.TagName, src => src.TagName)
				.Map(dest => dest.Note, src => src.Note);
		}
	}
}

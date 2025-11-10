using Application.DTOs.Requests.NewsArticles;
using Application.DTOs.Responses.NewsArticles;
using Domain.Entities;
using Mapster;

namespace Application.Mappings
{
	public class NewsArticleMapping : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<NewsArticle, GetResponse>()
				.Map(dest => dest.NewsArticleId, src => src.NewsArticleId)
				.Map(dest => dest.NewsTitle, src => src.NewsTitle)
				.Map(dest => dest.Headline, src => src.Headline)
				.Map(dest => dest.CreatedDate, src => src.CreatedDate)
				.Map(dest => dest.NewsContent, src => src.NewsContent)
				.Map(dest => dest.NewsSource, src => src.NewsSource)
				.Map(dest => dest.CategoryId, src => src.CategoryId)
				.Map(dest => dest.CategoryName, src => src.Category.CategoryName)
				.Map(dest => dest.NewsStatus, src => src.NewsStatus)
				.Map(dest => dest.CreatedById, src => src.CreatedById)
				.Map(dest => dest.UpdatedById, src => src.UpdatedById)
				.Map(dest => dest.ModifiedDate, src => src.ModifiedDate)
				.Map(dest => dest.Tags, src => src.Tags.Select(t => new TagsDTO { TagId = t.TagId, TagName = t.TagName }));

			config.NewConfig<CreateRequest, NewsArticle>()
				.Map(dest => dest.NewsTitle, src => src.NewsTitle)
				.Map(dest => dest.Headline, src => src.Headline)
				.Map(dest => dest.NewsContent, src => src.NewsContent)
				.Map(dest => dest.NewsSource, src => src.NewsSource)
				.Map(dest => dest.CategoryId, src => src.CategoryId)
				.Map(dest => dest.NewsStatus, src => src.NewsStatus)
				.Map(dest => dest.CreatedById, src => src.CreatedById);

			config.NewConfig<UpdateRequest, NewsArticle>()
				.IgnoreNullValues(true)
				.Map(dest => dest.NewsTitle, src => src.NewsTitle)
				.Map(dest => dest.Headline, src => src.Headline)
				.Map(dest => dest.NewsContent, src => src.NewsContent)
				.Map(dest => dest.NewsSource, src => src.NewsSource)
				.Map(dest => dest.CategoryId, src => src.CategoryId)
				.Map(dest => dest.NewsStatus, src => src.NewsStatus)
				.Map(dest => dest.UpdatedById, src => src.UpdatedById);
		}
	}
}

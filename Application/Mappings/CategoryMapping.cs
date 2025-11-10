using Application.DTOs.Requests.Categories;
using Application.DTOs.Responses.Categories;
using Domain.Entities;
using Mapster;

namespace Application.Mappings
{
	public class CategoryMapping : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<Category, GetResponse>()
				.Map(dest => dest.CategoryId, src => src.CategoryId)
				.Map(dest => dest.CategoryName, src => src.CategoryName)
				.Map(dest => dest.CategoryDescription, src => src.CategoryDescription)
				.Map(dest => dest.ParentCategoryId, src => src.ParentCategoryId)
				.Map(dest => dest.IsActive, src => src.IsActive);

			config.NewConfig<CreateRequest, Category>()
				.Map(dest => dest.CategoryName, src => src.CategoryName)
				.Map(dest => dest.CategoryDescription, src => src.CategoryDescription)
				.Map(dest => dest.ParentCategoryId, src => src.ParentCategoryId)
				.Map(dest => dest.IsActive, src => src.IsActive);

			config.NewConfig<UpdateRequest, Category>()
				.IgnoreNullValues(true)
				.Map(dest => dest.CategoryName, src => src.CategoryName)
				.Map(dest => dest.CategoryDescription, src => src.CategoryDescription)
				.Map(dest => dest.ParentCategoryId, src => src.ParentCategoryId)
				.Map(dest => dest.IsActive, src => src.IsActive);
		}
	}
}

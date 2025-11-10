using Application.DTOs.Requests.Categories;
using FluentValidation;

namespace Application.Validators.Categories
{
	public class CreateValidator : AbstractValidator<CreateRequest>
	{
		public CreateValidator()
		{
			RuleFor(x => x.CategoryName).NotEmpty().MaximumLength(200);
			RuleFor(x => x.CategoryDescription).MaximumLength(2000);
		}
	}
}

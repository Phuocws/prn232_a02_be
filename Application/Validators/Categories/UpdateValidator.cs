using Application.DTOs.Requests.Categories;
using FluentValidation;

namespace Application.Validators.Categories
{
	public class UpdateValidator : AbstractValidator<UpdateRequest>
	{
		public UpdateValidator()
		{
			RuleFor(x => x.CategoryName).MaximumLength(200).When(x => x.CategoryName != null);
			RuleFor(x => x.CategoryDescription).MaximumLength(2000).When(x => x.CategoryDescription != null);
		}
	}
}

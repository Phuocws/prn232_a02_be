using Application.DTOs.Requests.NewsArticles;
using FluentValidation;

namespace Application.Validators.NewsArticles
{
	public class CreateValidator : AbstractValidator<CreateRequest>
	{
		public CreateValidator()
		{
			RuleFor(x => x.NewsTitle).NotEmpty().MaximumLength(400);
			RuleFor(x => x.NewsContent).NotEmpty();
			RuleFor(x => x.CategoryId).GreaterThan(0);
		}
	}
}

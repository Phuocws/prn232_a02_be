using Application.DTOs.Requests.NewsArticles;
using FluentValidation;

namespace Application.Validators.NewsArticles
{
	public class UpdateValidator : AbstractValidator<UpdateRequest>
	{
		public UpdateValidator()
		{
			RuleFor(x => x.NewsTitle).MaximumLength(400).When(x => x.NewsTitle != null);
			RuleFor(x => x.NewsContent).NotEmpty().When(x => x.NewsContent != null);
			RuleFor(x => x.CategoryId).GreaterThan(0).When(x => x.CategoryId.HasValue);
		}
	}
}

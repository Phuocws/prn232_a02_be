using Application.DTOs.Requests.Tags;
using FluentValidation;

namespace Application.Validators.Tags
{
	public class CreateValidator : AbstractValidator<CreateRequest>
	{
		public CreateValidator()
		{
			RuleFor(x => x.TagName).NotEmpty().MaximumLength(200);
			RuleFor(x => x.Note).MaximumLength(1000);
		}
	}
}

using Application.DTOs.Requests.Tags;
using FluentValidation;

namespace Application.Validators.Tags
{
	public class UpdateValidator : AbstractValidator<UpdateRequest>
	{
		public UpdateValidator()
		{
			RuleFor(x => x.TagName).MaximumLength(200).When(x => x.TagName != null);
			RuleFor(x => x.Note).MaximumLength(1000).When(x => x.Note != null);
		}
	}
}

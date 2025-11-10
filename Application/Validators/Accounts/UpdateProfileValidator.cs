using Application.DTOs.Requests.Accounts;
using FluentValidation;

namespace Application.Validators.Accounts
{
	public class UpdateProfileValidator : AbstractValidator<UpdateProfileRequest>
	{
		public UpdateProfileValidator()
		{
			RuleFor(x => x.AccountName).MaximumLength(100);
			RuleFor(x => x.AccountEmail).EmailAddress().MaximumLength(256).When(x => !string.IsNullOrEmpty(x.AccountEmail));
			RuleFor(x => x.Password).MinimumLength(6).When(x => !string.IsNullOrEmpty(x.Password));
		}
	}
}

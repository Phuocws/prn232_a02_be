using Application.DTOs.Requests.Accounts;
using FluentValidation;

namespace Application.Validators.Accounts
{
	public class UpdateValidator : AbstractValidator<UpdateRequest>
	{
		public UpdateValidator()
		{
			RuleFor(x => x.AccountName).MaximumLength(100);
			RuleFor(x => x.AccountEmail).EmailAddress().MaximumLength(256).When(x => !string.IsNullOrEmpty(x.AccountEmail));

			RuleFor(x => x.Password).MinimumLength(8).When(x => !string.IsNullOrEmpty(x.Password))
				.WithMessage("Password must be at least 8 characters long.")
				.Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
				.Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
				.Matches("[0-9]").WithMessage("Password must contain at least one digit.")
				.Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

			RuleFor(x => x.AccountRole).IsInEnum();
		}
	}
}

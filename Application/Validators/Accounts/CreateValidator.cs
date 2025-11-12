using Application.DTOs.Requests.Accounts;
using FluentValidation;

namespace Application.Validators.Accounts
{
	public class CreateValidator : AbstractValidator<CreateRequest>
	{
		public CreateValidator()
		{
			RuleFor(x => x.AccountName).NotEmpty().MaximumLength(100);
			RuleFor(x => x.AccountEmail).NotEmpty().EmailAddress().MaximumLength(256);

			// Stronger password policy
			RuleFor(x => x.Password)
				.NotEmpty()
				.MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
				.Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
				.Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
				.Matches("[0-9]").WithMessage("Password must contain at least one digit.")
				.Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

			RuleFor(x => x.AccountRole).IsInEnum();
		}
	}
}

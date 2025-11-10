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
			RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
			RuleFor(x => x.AccountRole).IsInEnum();
		}
	}
}

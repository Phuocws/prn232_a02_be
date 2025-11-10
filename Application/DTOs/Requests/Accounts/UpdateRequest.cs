using Domain.Enums;

namespace Application.DTOs.Requests.Accounts
{
	public class UpdateRequest : UpdateProfileRequest
	{
		public AccountRoles? AccountRole { get; set; }
	}
}

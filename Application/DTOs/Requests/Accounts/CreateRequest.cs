using Domain.Enums;

namespace Application.DTOs.Requests.Accounts
{
	public class CreateRequest
	{
		public string AccountName { get; set; } = string.Empty;
		public string AccountEmail { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public AccountRoles AccountRole { get; set; }
	}
}

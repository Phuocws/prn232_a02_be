using Domain.Enums;

namespace Application.DTOs.Requests.Accounts
{
	public class GetLookupRequest
	{
		public string? AccountName { get; set; }
		public string? AccountEmail { get; set; }
		public AccountRoles? AccountRole { get; set; }
	}
}

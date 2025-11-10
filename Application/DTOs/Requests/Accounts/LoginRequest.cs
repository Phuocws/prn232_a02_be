namespace Application.DTOs.Requests.Accounts
{
	public class LoginRequest
	{
		public string Email { get; set; } = null!;
		public string Password { get; set; } = null!;
	}
}

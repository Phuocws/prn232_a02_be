namespace Application.DTOs.Requests.Accounts
{
	public class UpdateProfileRequest
	{
		public string? AccountName { get; set; }
		public string? AccountEmail { get; set; }
		public string? Password { get; set; }
	}
}

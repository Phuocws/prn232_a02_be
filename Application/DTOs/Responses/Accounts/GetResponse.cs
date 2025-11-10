namespace Application.DTOs.Responses.Accounts
{
	public class GetResponse
	{
		public int AccountId { get; set; }
		public string AccountName { get; set; } = string.Empty;
		public string AccountEmail { get; set; } = string.Empty;
		public int AccountRole { get; set; }
	}
}

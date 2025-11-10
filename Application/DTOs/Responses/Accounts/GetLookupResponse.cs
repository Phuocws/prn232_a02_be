namespace Application.DTOs.Responses.Accounts
{
	public class GetLookupResponse
	{
		public int Id { get; set; }
		public string AccountName { get; set; } = string.Empty;
		public string AccountEmail { get; set; } = string.Empty;
	}
}

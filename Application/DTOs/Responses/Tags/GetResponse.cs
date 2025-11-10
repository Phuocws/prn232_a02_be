namespace Application.DTOs.Responses.Tags
{
	public class GetResponse
	{
		public int TagId { get; set; }
		public string TagName { get; set; } = string.Empty;
		public string? Note { get; set; }
	}
}

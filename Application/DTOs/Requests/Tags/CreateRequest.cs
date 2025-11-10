namespace Application.DTOs.Requests.Tags
{
	public class CreateRequest
	{
		public string TagName { get; set; } = string.Empty;
		public string? Note { get; set; }
	}
}

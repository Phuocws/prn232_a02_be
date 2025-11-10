namespace Application.DTOs.Requests.Tags
{
	public class GetLookupRequest
	{
		/// <summary>
		/// Filter by tag name (partial match)
		/// </summary>
		public string? TagName { get; set; }
	}
}

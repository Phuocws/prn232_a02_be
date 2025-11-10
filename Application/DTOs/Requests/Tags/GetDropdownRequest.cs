namespace Application.DTOs.Requests.Tags
{
	public class GetDropdownRequest
	{
		/// <summary>
		/// Filter by tag name (partial match)
		/// </summary>
		public string? TagName { get; set; }
	}
}

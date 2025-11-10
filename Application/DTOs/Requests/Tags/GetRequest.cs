using Application.DTOs.Requests.Bases;

namespace Application.DTOs.Requests.Tags
{
	public class GetRequest : PagingAndSortingParameters
	{
		/// <summary>
		/// Filter by tag name (partial match)
		/// </summary>
		public string? TagName { get; set; }
	}
}

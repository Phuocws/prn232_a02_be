using Application.DTOs.Requests.Bases;
using Domain.Enums;

namespace Application.DTOs.Requests.NewsArticles
{
	public class GetMineRequest : PagingAndSortingParameters
	{
		public string? NewsTitle { get; set; }
		public string? Headline { get; set; }
		public string? NewsSource { get; set; }
		public int? CategoryId { get; set; }
		public NewsStatuses? NewsStatus { get; set; }
		public DateTime? CreatedDateFrom { get; set; }
		public DateTime? CreatedDateTo { get; set; }
		public bool IncludeInactiveCategories { get; set; }
		public IEnumerable<int>? TagIds { get; set; }
	}
}

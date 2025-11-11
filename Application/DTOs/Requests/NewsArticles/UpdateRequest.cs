using Domain.Enums;

namespace Application.DTOs.Requests.NewsArticles
{
	public class UpdateRequest
	{
		public string? NewsTitle { get; set; }
		public string? Headline { get; set; }
		public string? NewsContent { get; set; }
		public string? NewsSource { get; set; }
		public int? CategoryId { get; set; }
		public NewsStatuses? NewsStatus { get; set; }
		public IEnumerable<int>? TagIds { get; set; }
	}
}

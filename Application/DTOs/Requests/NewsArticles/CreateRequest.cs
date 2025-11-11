namespace Application.DTOs.Requests.NewsArticles
{
	public class CreateRequest
	{
		public string NewsTitle { get; set; } = string.Empty;
		public string? Headline { get; set; }
		public string NewsContent { get; set; } = string.Empty;
		public string? NewsSource { get; set; }
		public int CategoryId { get; set; }
		public IEnumerable<int>? TagIds { get; set; }
	}
}

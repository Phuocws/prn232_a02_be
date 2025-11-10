namespace Application.DTOs.Responses.NewsArticles
{
	public class GetSummaryResponse
	{
		public int NewsArticleId { get; set; }
		public string NewsTitle { get; set; } = string.Empty;
		public string? Headline { get; set; }
		public DateTime CreatedDate { get; set; }
		public string CategoryName { get; set; } = string.Empty;
		public byte NewsStatus { get; set; }
		public string CreatedByAccountName { get; set; } = string.Empty;
	}
}

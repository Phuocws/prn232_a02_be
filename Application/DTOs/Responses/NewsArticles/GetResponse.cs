namespace Application.DTOs.Responses.NewsArticles
{
	public class GetResponse
	{
		public int NewsArticleId { get; set; }
		public string NewsTitle { get; set; } = string.Empty;
		public string? Headline { get; set; }
		public DateTime CreatedDate { get; set; }
		public string NewsContent { get; set; } = string.Empty;
		public string? NewsSource { get; set; }
		public int CategoryId { get; set; }
		public string CategoryName { get; set; } = string.Empty;
		public byte NewsStatus { get; set; }
		public int CreatedById { get; set; }
		public int? UpdatedById { get; set; }
		public DateTime? ModifiedDate { get; set; }
		public IEnumerable<TagsDTO>? Tags { get; set; }
	}
	public class TagsDTO
	{
		public int TagId { get; set; }
		public string TagName { get; set; } = string.Empty;
	}
}

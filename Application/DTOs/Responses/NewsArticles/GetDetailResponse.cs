namespace Application.DTOs.Responses.NewsArticles
{
	public class GetDetailResponse
	{
		public int NewsArticleId { get; set; }
		public string NewsTitle { get; set; } = string.Empty;
		public string? Headline { get; set; }
		public DateTime CreatedDate { get; set; }
		public string NewsContent { get; set; } = string.Empty;
		public string? NewsSource { get; set; }
		public CategoryDTO Category { get; set; } = new CategoryDTO();
		public byte NewsStatus { get; set; }
		public AccountDTO Author { get; set; } = new AccountDTO();
		public AccountDTO? LastModifiedBy { get; set; }
		public DateTime? ModifiedDate { get; set; }
		public IEnumerable<TagsDTO>? Tags { get; set; }
	}
	public class TagsDTO
	{
		public int TagId { get; set; }
		public string TagName { get; set; } = string.Empty;
	}
	public class CategoryDTO
	{
		public int CategoryId { get; set; }
		public string CategoryName { get; set; } = string.Empty;
	}
	public class AccountDTO
	{
		public int SystemAccountId { get; set; }
		public string FullName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
	}
}

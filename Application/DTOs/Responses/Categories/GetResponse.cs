namespace Application.DTOs.Responses.Categories
{
	public class GetResponse
	{
		public int CategoryId { get; set; }
		public string CategoryName { get; set; } = string.Empty;
		public string? CategoryDescription { get; set; }
		public ParentDto? Parent { get; set; }
		public bool IsActive { get; set; }
	}
	public class ParentDto
	{
		public int ParentCategoryId { get; set; }
		public string ParentCategoryName { get; set; } = string.Empty;
	}
}

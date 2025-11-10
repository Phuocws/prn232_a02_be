namespace Application.DTOs.Responses.Categories
{
	public class GetResponse
	{
		public int CategoryId { get; set; }
		public string CategoryName { get; set; } = string.Empty;
		public string? CategoryDescription { get; set; }
		public int? ParentCategoryId { get; set; }
		public bool IsActive { get; set; }
	}
}

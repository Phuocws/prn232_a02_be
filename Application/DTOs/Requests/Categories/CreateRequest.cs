namespace Application.DTOs.Requests.Categories
{
	public class CreateRequest
	{
		public string CategoryName { get; set; } = string.Empty;
		public string? CategoryDescription { get; set; }
		public int? ParentCategoryId { get; set; }
		public bool IsActive { get; set; } = true;
	}
}

namespace Application.DTOs.Requests.Categories
{
	public class UpdateRequest
	{
		public string? CategoryName { get; set; }
		public string? CategoryDescription { get; set; }
		public int? ParentCategoryId { get; set; }
		public bool? IsActive { get; set; }
	}
}

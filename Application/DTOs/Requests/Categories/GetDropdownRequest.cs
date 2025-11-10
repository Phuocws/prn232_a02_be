namespace Application.DTOs.Requests.Categories
{
	public class GetDropdownRequest
	{
		public bool IncludeInactive { get; set; } = false;
		public bool IncludeParentCategoriesOnly { get; set; } = false;
	}
}

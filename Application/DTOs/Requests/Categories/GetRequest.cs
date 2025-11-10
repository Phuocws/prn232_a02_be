using Application.DTOs.Requests.Bases;

namespace Application.DTOs.Requests.Categories
{
	public class GetRequest : PagingAndSortingParameters
	{
		/// <summary>
		/// Filter by category name (partial match)
		/// </summary>
		public string? CategoryName { get; set; }

		/// <summary>
		/// Filter by parent category id
		/// </summary>
		public int? ParentCategoryId { get; set; }

		/// <summary>
		/// Filter by active flag
		/// </summary>
		public bool? IsActive { get; set; }
	}
}

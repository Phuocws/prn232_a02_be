using Application.DTOs.Requests.Bases;

namespace Application.DTOs.Requests.Accounts
{
	public class GetRequest : PagingAndSortingParameters
	{
		/// <summary>
		/// Filter by account name (partial match)
		/// </summary>
		public string? AccountName { get; set; }

		/// <summary>
		/// Filter by account email (partial match)
		/// </summary>
		public string? AccountEmail { get; set; }

		/// <summary>
		/// Filter by account role id
		/// </summary>
		public int? AccountRole { get; set; }
	}
}

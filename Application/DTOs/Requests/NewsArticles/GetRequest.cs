namespace Application.DTOs.Requests.NewsArticles
{
	public class GetRequest : GetMineRequest
	{
		public int? CreatedBy { get; set; }
		public int? UpdatedBy { get; set; }
	}
}

namespace Application.DTOs.Responses.Categories
{
	public class GetDropdownResponse
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public List<GetDropdownResponse> Children { get; set; } = new List<GetDropdownResponse>();
	}
}

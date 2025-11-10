using Application.DTOs.Requests.Categories;
using Application.DTOs.Responses.Bases;
using Application.DTOs.Responses.Categories;

namespace Application.Interfaces.Services
{
	public interface ICategoryService
	{
		Task<BaseResponse<PagedResult<GetResponse>>> GetWithPagingSortFilterAsync(GetRequest request);
		Task<BaseResponse<List<GetDropdownResponse>>> GetDropdownAsync();
		Task<BaseResponse<GetResponse>> GetByIdAsync(int id);
		Task<BaseResponse<string>> CreateAsync(CreateRequest request);
		Task<BaseResponse<string>> UpdateAsync(int id, UpdateRequest request);
		Task<BaseResponse<string>> DeleteAsync(int id);
	}
}

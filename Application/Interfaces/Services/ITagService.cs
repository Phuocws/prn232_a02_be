using Application.DTOs.Requests.Tags;
using Application.DTOs.Responses.Bases;
using Application.DTOs.Responses.Tags;

namespace Application.Interfaces.Services
{
	public interface ITagService
	{
		Task<BaseResponse<PagedResult<GetResponse>>> GetWithPagedSortFilter(GetRequest request);
		Task<BaseResponse<GetResponse>> GetByIdAsync(int id);
		Task<BaseResponse<string>> CreateAsync(CreateRequest request);
		Task<BaseResponse<string>> UpdateAsync(int id, UpdateRequest request);
		Task<BaseResponse<string>> DeleteAsync(int id);
	}
}

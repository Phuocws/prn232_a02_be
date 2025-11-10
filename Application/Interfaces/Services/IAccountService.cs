using Application.DTOs.Requests.Accounts;
using Application.DTOs.Responses.Accounts;
using Application.DTOs.Responses.Bases;

namespace Application.Interfaces.Services
{
	public interface IAccountService
	{
		Task<BaseResponse<PagedResult<GetResponse>>> GetWithPagedSortFilter(GetRequest request);
		Task<BaseResponse<List<GetLookupResponse>>> GetLookupAsync(GetLookupRequest request);
		Task<BaseResponse<GetResponse>> GetByIdAsync(int id);
		Task<BaseResponse<string>> LoginAsync(LoginRequest login);
		Task<BaseResponse<string>> CreateAsync(CreateRequest request);
		Task<BaseResponse<string>> UpdateAsync(int id, UpdateRequest request);
		Task<BaseResponse<string>> UpdateProfileAsync(int userId, UpdateProfileRequest request);
		Task<BaseResponse<string>> DeleteAsync(int id);
	}
}

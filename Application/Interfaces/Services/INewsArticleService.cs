using Application.DTOs.Requests.NewsArticles;
using Application.DTOs.Responses.Bases;
using Application.DTOs.Responses.NewsArticles;

namespace Application.Interfaces.Services
{
	public interface INewsArticleService
	{
		Task<BaseResponse<PagedResult<GetResponse>>> GetWithPagingSortFilterAsync(GetRequest request);
		Task<BaseResponse<PagedResult<GetResponse>>> GetMineWithPagingSortFilterAsync(int ownerId, GetMineRequest request);
		Task<BaseResponse<GetResponse>> GetByIdAsync(int id);
		Task<BaseResponse<string>> CreateAsync(CreateRequest request);
		Task<BaseResponse<string>> UpdateAsync(int id, UpdateRequest request);
		Task<BaseResponse<string>> DeleteAsync(int id);
	}
}

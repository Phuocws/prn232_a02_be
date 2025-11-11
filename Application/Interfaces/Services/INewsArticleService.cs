using Application.DTOs.Requests.NewsArticles;
using Application.DTOs.Responses.Bases;
using Application.DTOs.Responses.NewsArticles;

namespace Application.Interfaces.Services
{
	public interface INewsArticleService
	{
		Task<BaseResponse<PagedResult<GetSummaryResponse>>> GetWithPagingSortFilterAsync(GetRequest request);
		Task<BaseResponse<PagedResult<GetSummaryResponse>>> GetMineWithPagingSortFilterAsync(int ownerId, GetMineRequest request);
		Task<BaseResponse<GetDetailResponse>> GetByIdAsync(int id);
		Task<BaseResponse<string>> CreateAsync(int ownerId, CreateRequest request);
		Task<BaseResponse<string>> UpdateAsync(int ownerId, int id, UpdateRequest request);
		Task<BaseResponse<string>> DeleteAsync(int id);
		Task<BaseResponse<GetStatisticsReportResponse>> GetReportAsync(GetStatisticsReportRequest request);
	}
}

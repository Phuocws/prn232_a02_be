using Application.DTOs.Requests.NewsArticles;
using Application.DTOs.Responses.Bases;
using Application.DTOs.Responses.NewsArticles;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class NewsArticlesController : ControllerBase
	{
		private readonly INewsArticleService _newsArticleService;

		public NewsArticlesController(INewsArticleService newsArticleService)
		{
			_newsArticleService = newsArticleService;
		}

		[HttpGet]
		public async Task<ActionResult<BaseResponse<PagedResult<GetResponse>>>> Get([FromQuery] GetRequest request)
		{
			var result = await _newsArticleService.GetWithPagingSortFilterAsync(request);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpGet("my-news")]
		public async Task<ActionResult<BaseResponse<PagedResult<GetResponse>>>> GetMyNews([FromQuery] GetMineRequest request)
		{
			var idClaim = User.Claims.FirstOrDefault(c => c.Type == "id" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out var ownerId))
			{
				return StatusCode((int)Domain.Enums.StatusCodes.Unauthorized,
					new BaseResponse<GetResponse>("Invalid token or user id missing", Domain.Enums.StatusCodes.Unauthorized, null));
			}
			var result = await _newsArticleService.GetMineWithPagingSortFilterAsync(ownerId, request);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<BaseResponse<GetResponse>>> GetById(int id)
		{
			var result = await _newsArticleService.GetByIdAsync(id);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpPost]
		public async Task<ActionResult<BaseResponse<string>>> Create([FromBody] CreateRequest request)
		{
			var result = await _newsArticleService.CreateAsync(request);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpPut("{id}")]
		public async Task<ActionResult<BaseResponse<string>>> Update(int id, [FromBody] UpdateRequest request)
		{
			var result = await _newsArticleService.UpdateAsync(id, request);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpDelete("{id}")]
		public async Task<ActionResult<BaseResponse<string>>> Delete(int id)
		{
			var result = await _newsArticleService.DeleteAsync(id);
			return StatusCode((int)result.StatusCode, result);
		}
	}
}

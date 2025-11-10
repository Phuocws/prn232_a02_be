using Application.DTOs.Requests.Tags;
using Application.DTOs.Responses.Bases;
using Application.DTOs.Responses.Tags;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class TagsController : ControllerBase
	{
		private readonly ITagService _tagService;

		public TagsController(ITagService tagService)
		{
			_tagService = tagService;
		}

		[HttpGet]
		public async Task<ActionResult<BaseResponse<PagedResult<GetResponse>>>> Get([FromQuery] GetRequest request)
		{
			var result = await _tagService.GetWithPagedSortFilter(request);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<BaseResponse<GetResponse>>> GetById(int id)
		{
			var result = await _tagService.GetByIdAsync(id);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpPost]
		public async Task<ActionResult<BaseResponse<string>>> Create([FromBody] CreateRequest request)
		{
			var result = await _tagService.CreateAsync(request);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpPut("{id}")]
		public async Task<ActionResult<BaseResponse<string>>> Update(int id, [FromBody] UpdateRequest request)
		{
			var result = await _tagService.UpdateAsync(id, request);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpDelete("{id}")]
		public async Task<ActionResult<BaseResponse<string>>> Delete(int id)
		{
			var result = await _tagService.DeleteAsync(id);
			return StatusCode((int)result.StatusCode, result);
		}
	}
}

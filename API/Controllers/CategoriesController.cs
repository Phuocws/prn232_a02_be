using Application.DTOs.Requests.Categories;
using Application.DTOs.Responses.Bases;
using Application.DTOs.Responses.Categories;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class CategoriesController : ControllerBase
	{
		private readonly ICategoryService _categoryService;

		public CategoriesController(ICategoryService categoryService)
		{
			_categoryService = categoryService;
		}

		[HttpGet]
		public async Task<ActionResult<BaseResponse<PagedResult<GetResponse>>>> Get([FromQuery] GetRequest request)
		{
			var result = await _categoryService.GetWithPagingSortFilterAsync(request);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<BaseResponse<GetResponse>>> GetById(int id)
		{
			var result = await _categoryService.GetByIdAsync(id);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpGet("dropdown")]
		public async Task<ActionResult<BaseResponse<List<GetDropdownResponse>>>> GetDropdown([FromQuery] GetDropdownRequest request)
		{
			var result = await _categoryService.GetDropdownAsync(request);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpPost]
		public async Task<ActionResult<BaseResponse<string>>> Create([FromBody] CreateRequest request)
		{
			var result = await _categoryService.CreateAsync(request);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpPut("{id}")]
		public async Task<ActionResult<BaseResponse<string>>> Update(int id, [FromBody] UpdateRequest request)
		{
			var result = await _categoryService.UpdateAsync(id, request);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpDelete("{id}")]
		public async Task<ActionResult<BaseResponse<string>>> Delete(int id)
		{
			var result = await _categoryService.DeleteAsync(id);
			return StatusCode((int)result.StatusCode, result);
		}
	}
}

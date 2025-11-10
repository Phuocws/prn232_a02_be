using Application.DTOs.Requests.Accounts;
using Application.DTOs.Responses.Accounts;
using Application.DTOs.Responses.Bases;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class AccountsController : ControllerBase
	{
		private readonly IAccountService _accountService;

		public AccountsController(IAccountService accountService)
		{
			_accountService = accountService;
		}

		[HttpPost("login")]
		[AllowAnonymous]
		public async Task<ActionResult<BaseResponse<string>>> Login([FromBody] LoginRequest request)
		{
			var result = await _accountService.LoginAsync(request);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpPost]
		public async Task<ActionResult<BaseResponse<string>>> Create([FromBody] CreateRequest request)
		{
			var result = await _accountService.CreateAsync(request);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpPut("{id}")]
		public async Task<ActionResult<BaseResponse<string>>> Update(int id, [FromBody] UpdateRequest request)
		{
			var result = await _accountService.UpdateAsync(id, request);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpPut("profile")]
		public async Task<ActionResult<BaseResponse<string>>> UpdateProfile([FromBody] UpdateProfileRequest request)
		{
			var idClaim = User.Claims.FirstOrDefault(c => c.Type == "id" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out var ownerId))
			{
				return StatusCode((int)Domain.Enums.StatusCodes.Unauthorized,
					new BaseResponse<string>("Invalid token or user id missing", Domain.Enums.StatusCodes.Unauthorized, null));
			}

			var result = await _accountService.UpdateProfileAsync(ownerId, request);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpDelete("{id}")]
		public async Task<ActionResult<BaseResponse<string>>> Delete(int id)
		{
			var result = await _accountService.DeleteAsync(id);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpGet]
		public async Task<ActionResult<BaseResponse<PagedResult<GetResponse>>>> Get([FromQuery] GetRequest request)
		{
			var result = await _accountService.GetWithPagedSortFilter(request);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<BaseResponse<GetResponse>>> GetById(int id)
		{
			var result = await _accountService.GetByIdAsync(id);
			return StatusCode((int)result.StatusCode, result);
		}

		[HttpGet("profile")]
		public async Task<ActionResult<BaseResponse<GetResponse>>> GetProfile()
		{
			var idClaim = User.Claims.FirstOrDefault(c => c.Type == "id" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out var ownerId))
			{
				return StatusCode((int)Domain.Enums.StatusCodes.Unauthorized,
					new BaseResponse<GetResponse>("Invalid token or user id missing", Domain.Enums.StatusCodes.Unauthorized, null));
			}
			var result = await _accountService.GetByIdAsync(ownerId);
			return StatusCode((int)result.StatusCode, result);
		}
	}
}

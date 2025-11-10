using Application.DTOs.Requests.Accounts;
using Application.DTOs.Responses.Accounts;
using Application.DTOs.Responses.Bases;
using Application.Extensions.Queries;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
	public class AccountService : IAccountService
	{
		private readonly IGenericRepository<SystemAccount> _accountRepository;
		private readonly IAuthRepository _authRepository;
		private readonly IMapper _mapper;
		private readonly IValidator<CreateRequest> _createValidator;
		private readonly IValidator<UpdateRequest> _updateValidator;
		private readonly IValidator<UpdateProfileRequest> _updateProfileValidator;


		public AccountService(IGenericRepository<SystemAccount> accountRepository, IAuthRepository authRepository, IMapper mapper, IValidator<CreateRequest> createValidator, IValidator<UpdateRequest> updateValidator, IValidator<UpdateProfileRequest> updateProfileValidator)
		{
			_accountRepository = accountRepository;
			_authRepository = authRepository;
			_mapper = mapper;
			_createValidator = createValidator;
			_updateValidator = updateValidator;
			_updateProfileValidator = updateProfileValidator;
		}

		public async Task<BaseResponse<string>> LoginAsync(LoginRequest request)
		{
			if (request is null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
				return new BaseResponse<string>("Invalid login request", StatusCodes.BadRequest, null);

			var user = await _accountRepository.GetByConditionAsync(u => u.AccountEmail == request.Email);
			if (user == null)
				return new BaseResponse<string>("User not found", StatusCodes.NotFound, null);

			var providedHash = HashPassword(request.Password);
			if (!string.Equals(providedHash, user.AccountPassword, StringComparison.Ordinal))
				return new BaseResponse<string>("Invalid password", StatusCodes.Unauthorized, null);

			var token = GenerateToken(user);
			return new BaseResponse<string>("Login successful", StatusCodes.Ok, token);
		}

		private string GenerateToken(SystemAccount user)
		{
			var role = ((AccountRoles)user.AccountRole) switch
			{
				AccountRoles.Admin => "Admin",
				AccountRoles.Staff => "Staff",
				_ => "Lecturer"
			};

			var token = _authRepository.GenerateJwtToken(user, role);
			return token;
		}

		private static string HashPassword(string password)
		{
			if (password is null) return string.Empty;
			var bytes = Encoding.UTF8.GetBytes(password);
			var hash = SHA256.HashData(bytes);
			return Convert.ToBase64String(hash);
		}

		public async Task<BaseResponse<string>> CreateAsync(CreateRequest request)
		{
			if (request is null)
				return new BaseResponse<string>("Request is null", StatusCodes.BadRequest, null);

			// Validate
			var validation = await _createValidator.ValidateAsync(request);
			if (!validation.IsValid)
			{
				var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
				// Return validation errors in Data instead of Message
				return new BaseResponse<string>("Validation failed", StatusCodes.BadRequest, errors);
			}

			// Check unique email
			if (await _accountRepository.AnyAsync(u => u.AccountEmail == request.AccountEmail))
				return new BaseResponse<string>("An account with the provided email already exists.", StatusCodes.BadRequest, null);

			// Map to entity using injected mapper
			var entity = _mapper.Map<SystemAccount>(request);

			// Hash password before saving
			entity.AccountPassword = HashPassword(request.Password);

			await _accountRepository.AddAsync(entity);

			var saved = await _accountRepository.SaveChangesAsync();
			if (!saved)
				return new BaseResponse<string>("Failed to create account", StatusCodes.InternalServerError, null);

			return new BaseResponse<string>("Account created", StatusCodes.Created, entity.AccountId.ToString());
		}

		public async Task<BaseResponse<string>> UpdateAsync(int accountId, UpdateRequest request)
		{
			if (request is null)
				return new BaseResponse<string>("Request is null", StatusCodes.BadRequest, null);

			var existing = await _accountRepository.GetByIdAsync(accountId);
			if (existing == null)
				return new BaseResponse<string>("Account not found", StatusCodes.NotFound, null);

			// Validate
			var validation = await _updateValidator.ValidateAsync(request);
			if (!validation.IsValid)
			{
				var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
				return new BaseResponse<string>("Validation failed", StatusCodes.BadRequest, errors);
			}

			// If email updated, ensure uniqueness
			if (!string.IsNullOrWhiteSpace(request.AccountEmail) && await _accountRepository.AnyAsync(u => u.AccountEmail == request.AccountEmail && u.AccountId != accountId))
				return new BaseResponse<string>("Another account with the provided email already exists.", StatusCodes.BadRequest, null);

			// Map non-null properties from request onto existing entity using IMapper
			_mapper.Map(request, existing);

			// If password provided, hash it
			if (!string.IsNullOrWhiteSpace(request.Password))
			{
				existing.AccountPassword = HashPassword(request.Password);
			}

			_accountRepository.Update(existing);
			var saved = await _accountRepository.SaveChangesAsync();
			if (!saved)
				return new BaseResponse<string>("Failed to update account", StatusCodes.InternalServerError, null);

			return new BaseResponse<string>("Account updated", StatusCodes.Ok, existing.AccountId.ToString());
		}

		public async Task<BaseResponse<string>> DeleteAsync(int accountId)
		{
			// Load account including created articles to check constraint
			var existing = await _accountRepository.FirstOrDefaultAsync(
				predicate: a => a.AccountId == accountId,
				include: q => q.Include(a => a.NewsArticleCreatedBies),
				asNoTracking: false
			);

			if (existing == null)
				return new BaseResponse<string>("Account not found", StatusCodes.NotFound, null);

			// If account has created any news articles, do not allow deletion
			if (existing.NewsArticleCreatedBies != null && existing.NewsArticleCreatedBies.Any())
				return new BaseResponse<string>("Cannot delete account because it has created news articles.", StatusCodes.BadRequest, null);

			_accountRepository.Remove(existing);
			var saved = await _accountRepository.SaveChangesAsync();
			if (!saved)
				return new BaseResponse<string>("Failed to delete account", StatusCodes.InternalServerError, null);

			return new BaseResponse<string>("Account deleted", StatusCodes.Ok, null);
		}

		public async Task<BaseResponse<PagedResult<GetResponse>>> GetWithPagedSortFilter(GetRequest request)
		{
			if (request is null)
				return new BaseResponse<PagedResult<GetResponse>>("Request is null", StatusCodes.BadRequest, null);

			// Build filter expression
			Expression<Func<SystemAccount, bool>>? filter = null;

			if (!string.IsNullOrWhiteSpace(request.AccountName))
			{
				var keyword = request.AccountName.Trim();
				Expression<Func<SystemAccount, bool>> nameFilter = a =>
					a.AccountName != null && (
						EF.Functions.Collate(a.AccountName, "Vietnamese_CI_AI").Contains(keyword) ||
						EF.Functions.Collate(a.AccountName, "Latin1_General_CI_AI").Contains(keyword)
					);
				filter = filter is null ? nameFilter : filter.AndAlso(nameFilter);
			}

			if (!string.IsNullOrWhiteSpace(request.AccountEmail))
			{
				var emailKeyword = request.AccountEmail.Trim();
				Expression<Func<SystemAccount, bool>> emailFilter = a =>
					a.AccountEmail != null &&
					EF.Functions.Collate(a.AccountEmail, "Latin1_General_CI_AI").Contains(emailKeyword);
				filter = filter is null ? emailFilter : filter.AndAlso(emailFilter);
			}

			if (request.AccountRole.HasValue)
			{
				Expression<Func<SystemAccount, bool>> roleFilter = a => a.AccountRole == request.AccountRole.Value;
				filter = filter is null ? roleFilter : filter.AndAlso(roleFilter);
			}

			// Use repository's paging with sorting extension.
			var (items, totalCount) = await _accountRepository.GetPagedAsync(
				filter: filter,
				include: null,
				orderBy: q => q.ApplySorting(request.SortBy, request.IsDescending),
				pageNumber: request.PageNumber,
				pageSize: request.PageSize,
				asNoTracking: true
			);

			var mapped = _mapper.Map<IEnumerable<GetResponse>>(items);
			var paged = new PagedResult<GetResponse>(mapped, request.PageNumber, request.PageSize, totalCount);

			return new BaseResponse<PagedResult<GetResponse>>("Accounts retrieved", StatusCodes.Ok, paged);
		}

		public async Task<BaseResponse<GetResponse>> GetByIdAsync(int accountId)
		{
			var entity = await _accountRepository.GetByIdAsync(accountId);
			if (entity == null)
				return new BaseResponse<GetResponse>("Account not found", StatusCodes.NotFound, null);

			var dto = _mapper.Map<GetResponse>(entity);
			return new BaseResponse<GetResponse>("Account retrieved", StatusCodes.Ok, dto);
		}

		public async Task<BaseResponse<string>> UpdateProfileAsync(int accountId, UpdateProfileRequest request)
		{
			if (request is null)
				return new BaseResponse<string>("Request is null", StatusCodes.BadRequest, null);

			var existing = await _accountRepository.GetByIdAsync(accountId);
			if (existing == null)
				return new BaseResponse<string>("Profile not found", StatusCodes.NotFound, null);

			// Validate
			var validation = await _updateProfileValidator.ValidateAsync(request);
			if (!validation.IsValid)
			{
				var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
				return new BaseResponse<string>("Validation failed", StatusCodes.BadRequest, errors);
			}

			// If email updated, ensure uniqueness
			if (!string.IsNullOrWhiteSpace(request.AccountEmail) && await _accountRepository.AnyAsync(u => u.AccountEmail == request.AccountEmail && u.AccountId != accountId))
				return new BaseResponse<string>("Another account with the provided email already exists.", StatusCodes.BadRequest, null);

			// Map non-null properties from request onto existing entity using IMapper
			_mapper.Map(request, existing);

			// If password provided, hash it
			if (!string.IsNullOrWhiteSpace(request.Password))
			{
				existing.AccountPassword = HashPassword(request.Password);
			}

			_accountRepository.Update(existing);
			var saved = await _accountRepository.SaveChangesAsync();
			if (!saved)
				return new BaseResponse<string>("Failed to update profile", StatusCodes.InternalServerError, null);

			return new BaseResponse<string>("Profile updated", StatusCodes.Ok, existing.AccountId.ToString());
		}
	}
}

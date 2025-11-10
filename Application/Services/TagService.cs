using Application.DTOs.Requests.Tags;
using Application.DTOs.Responses.Bases;
using Application.DTOs.Responses.Tags;
using Application.Extensions.Queries;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services
{
	public class TagService : ITagService
	{
		private readonly IGenericRepository<Tag> _tagRepository;
		private readonly IMapper _mapper;
		private readonly IValidator<CreateRequest> _createValidator;
		private readonly IValidator<UpdateRequest> _updateValidator;

		public TagService(IGenericRepository<Tag> tagRepository, IMapper mapper, IValidator<CreateRequest> createValidator, IValidator<UpdateRequest> updateValidator)
		{
			_tagRepository = tagRepository;
			_mapper = mapper;
			_createValidator = createValidator;
			_updateValidator = updateValidator;
		}

		public async Task<BaseResponse<string>> CreateAsync(CreateRequest request)
		{
			if (request is null)
				return new BaseResponse<string>("Request is null", StatusCodes.BadRequest, null);

			var validation = await _createValidator.ValidateAsync(request);
			if (!validation.IsValid)
			{
				var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
				return new BaseResponse<string>("Validation failed", StatusCodes.BadRequest, errors);
			}

			if (!string.IsNullOrWhiteSpace(request.TagName) && await _tagRepository.AnyAsync(t => t.TagName == request.TagName))
				return new BaseResponse<string>("Tag with the same name already exists.", StatusCodes.BadRequest, null);

			var entity = _mapper.Map<Tag>(request);
			await _tagRepository.AddAsync(entity);
			var saved = await _tagRepository.SaveChangesAsync();
			if (!saved)
				return new BaseResponse<string>("Failed to create tag", StatusCodes.InternalServerError, null);

			return new BaseResponse<string>("Tag created", StatusCodes.Created, entity.TagId.ToString());
		}

		public async Task<BaseResponse<string>> DeleteAsync(int id)
		{
			var existing = await _tagRepository.FirstOrDefaultAsync(
				predicate: t => t.TagId == id,
				include: q => q.Include(t => t.NewsArticles),
				asNoTracking: false
			);

			if (existing == null)
				return new BaseResponse<string>("Tag not found", StatusCodes.NotFound, null);

			if (existing.NewsArticles != null && existing.NewsArticles.Count != 0)
				return new BaseResponse<string>("Cannot delete tag because it is used by news articles.", StatusCodes.BadRequest, null);

			_tagRepository.Remove(existing);
			var saved = await _tagRepository.SaveChangesAsync();
			if (!saved)
				return new BaseResponse<string>("Failed to delete tag", StatusCodes.InternalServerError, null);

			return new BaseResponse<string>("Tag deleted", StatusCodes.Ok, null);
		}

		public async Task<BaseResponse<GetResponse>> GetByIdAsync(int id)
		{
			var entity = await _tagRepository.GetByIdAsync(id);
			if (entity == null)
				return new BaseResponse<GetResponse>("Tag not found", StatusCodes.NotFound, null);

			var dto = _mapper.Map<GetResponse>(entity);
			return new BaseResponse<GetResponse>("Tag retrieved", StatusCodes.Ok, dto);
		}

		public async Task<BaseResponse<List<GetDropdownResponse>>> GetDropdownAsync(GetDropdownRequest request)
		{
			if (request is null)
				return new BaseResponse<List<GetDropdownResponse>>("Request is null", StatusCodes.BadRequest, null);

			// If no filter provided, return empty list as requested
			if (string.IsNullOrWhiteSpace(request.TagName))
			{
				return new BaseResponse<List<GetDropdownResponse>>("Tags retrieved", StatusCodes.Ok, new List<GetDropdownResponse>());
			}

			var keyword = request.TagName.Trim();

			var items = await _tagRepository.GetAllAsync(
				filter: t => t.TagName != null && EF.Functions.Collate(t.TagName, "Latin1_General_CI_AI").Contains(keyword),
				orderBy: q => q.OrderBy(t => t.TagName),
				asNoTracking: true
			);

			var mapped = _mapper.Map<IEnumerable<GetDropdownResponse>>(items).Take(5).ToList();

			return new BaseResponse<List<GetDropdownResponse>>("Tags retrieved", StatusCodes.Ok, mapped);
		}

		public async Task<BaseResponse<PagedResult<GetResponse>>> GetWithPagedSortFilter(GetRequest request)
		{
			if (request is null)
				return new BaseResponse<PagedResult<GetResponse>>("Request is null", StatusCodes.BadRequest, null);

			Expression<Func<Tag, bool>>? filter = null;
			if (!string.IsNullOrWhiteSpace(request.TagName))
			{
				Expression<Func<Tag, bool>> nameFilter = t => t.TagName.Contains(request.TagName.Trim());
				filter = filter is null ? nameFilter : ExpressionExtensions.AndAlso(filter, nameFilter);
			}

			var (items, totalCount) = await _tagRepository.GetPagedAsync(
				filter: filter,
				include: null,
				orderBy: q => q.ApplySorting(request.SortBy, request.IsDescending),
				pageNumber: request.PageNumber,
				pageSize: request.PageSize,
				asNoTracking: true
			);

			var mapped = _mapper.Map<IEnumerable<GetResponse>>(items);
			var paged = new PagedResult<GetResponse>(mapped, request.PageNumber, request.PageSize, totalCount);

			return new BaseResponse<PagedResult<GetResponse>>("Tags retrieved", StatusCodes.Ok, paged);
		}

		public async Task<BaseResponse<string>> UpdateAsync(int id, UpdateRequest request)
		{
			if (request is null)
				return new BaseResponse<string>("Request is null", StatusCodes.BadRequest, null);

			var existing = await _tagRepository.GetByIdAsync(id);
			if (existing == null)
				return new BaseResponse<string>("Tag not found", StatusCodes.NotFound, null);

			var validation = await _updateValidator.ValidateAsync(request);
			if (!validation.IsValid)
			{
				var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
				return new BaseResponse<string>("Validation failed", StatusCodes.BadRequest, errors);
			}

			if (!string.IsNullOrWhiteSpace(request.TagName) && await _tagRepository.AnyAsync(t => t.TagName == request.TagName && t.TagId != id))
				return new BaseResponse<string>("Another tag with the same name exists.", StatusCodes.BadRequest, null);

			_mapper.Map(request, existing);
			_tagRepository.Update(existing);
			var saved = await _tagRepository.SaveChangesAsync();
			if (!saved)
				return new BaseResponse<string>("Failed to update tag", StatusCodes.InternalServerError, null);

			return new BaseResponse<string>("Tag updated", StatusCodes.Ok, existing.TagId.ToString());
		}
	}
}

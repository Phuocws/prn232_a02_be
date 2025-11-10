using Application.DTOs.Requests.NewsArticles;
using Application.DTOs.Responses.Bases;
using Application.DTOs.Responses.NewsArticles;
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
	public class NewsArticleService : INewsArticleService
	{
		private readonly IGenericRepository<NewsArticle> _newsArticleRepository;
		private readonly IGenericRepository<Tag> _tagRepository;
		private readonly IMapper _mapper;
		private readonly IValidator<CreateRequest> _createRequestValidator;
		private readonly IValidator<UpdateRequest> _updateRequestValidator;

		public NewsArticleService(IGenericRepository<NewsArticle> newsArticleRepository, IGenericRepository<Tag> tagRepository, IMapper mapper, IValidator<CreateRequest> createRequestValidator, IValidator<UpdateRequest> updateRequestValidator)
		{
			_newsArticleRepository = newsArticleRepository;
			_tagRepository = tagRepository;
			_mapper = mapper;
			_createRequestValidator = createRequestValidator;
			_updateRequestValidator = updateRequestValidator;
		}

		public async Task<BaseResponse<string>> CreateAsync(CreateRequest request)
		{
			if (request is null)
				return new BaseResponse<string>("Request is null", StatusCodes.BadRequest, null);

			var validation = await _createRequestValidator.ValidateAsync(request);
			if (!validation.IsValid)
			{
				var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
				return new BaseResponse<string>(errors, StatusCodes.BadRequest, null);
			}

			var entity = _mapper.Map<NewsArticle>(request);

			// Attach tags if provided
			if (request.TagIds != null && request.TagIds.Any())
			{
				var tags = (await _tagRepository.GetAllAsync(t => request.TagIds.Contains(t.TagId))).ToList();
				entity.Tags = tags;
			}

			await _newsArticleRepository.AddAsync(entity);
			var saved = await _newsArticleRepository.SaveChangesAsync();
			if (!saved)
				return new BaseResponse<string>("Failed to create news article", StatusCodes.InternalServerError, null);

			return new BaseResponse<string>("News article created", StatusCodes.Created, entity.NewsArticleId.ToString());
		}

		public async Task<BaseResponse<string>> DeleteAsync(int id)
		{
			var existing = await _newsArticleRepository.FirstOrDefaultAsync(
				predicate: n => n.NewsArticleId == id,
				include: q => q.Include(n => n.Tags),
				asNoTracking: false
			);

			if (existing == null)
				return new BaseResponse<string>("News article not found", StatusCodes.NotFound, null);

			_newsArticleRepository.Remove(existing);
			var saved = await _newsArticleRepository.SaveChangesAsync();
			if (!saved)
				return new BaseResponse<string>("Failed to delete news article", StatusCodes.InternalServerError, null);

			return new BaseResponse<string>("News article deleted", StatusCodes.Ok, null);
		}

		public async Task<BaseResponse<GetDetailResponse>> GetByIdAsync(int id)
		{
			var entity = await _newsArticleRepository.GetByConditionAsync(
										na => na.NewsArticleId == id,
										na => na.Include(na => na.Category)
												.Include(na => na.CreatedBy)
												.Include(na => na.UpdatedBy)
												.Include(na => na.Tags)
							 			);
			if (entity == null)
				return new BaseResponse<GetDetailResponse>("News article not found", StatusCodes.NotFound, null);

			var dto = _mapper.Map<GetDetailResponse>(entity);
			return new BaseResponse<GetDetailResponse>("News article retrieved", StatusCodes.Ok, dto);
		}

		public async Task<BaseResponse<PagedResult<GetSummaryResponse>>> GetWithPagingSortFilterAsync(GetRequest request)
		{
			if (request is null)
				return new BaseResponse<PagedResult<GetSummaryResponse>>("Request is null", StatusCodes.BadRequest, null);

			var filter = BuildFilterFromAdminRequest(request);

			var (items, totalCount) = await _newsArticleRepository.GetPagedAsync(
				filter: filter,
				include: q => q.Include(n => n.Category).Include(n => n.CreatedBy),
				orderBy: q => q.ApplySorting(request.SortBy, request.IsDescending),
				pageNumber: request.PageNumber,
				pageSize: request.PageSize,
				asNoTracking: true
			);

			var mapped = _mapper.Map<IEnumerable<GetSummaryResponse>>(items);
			var paged = new PagedResult<GetSummaryResponse>(mapped, request.PageNumber, request.PageSize, totalCount);

			return new BaseResponse<PagedResult<GetSummaryResponse>>("News articles retrieved", StatusCodes.Ok, paged);
		}

		public async Task<BaseResponse<PagedResult<GetSummaryResponse>>> GetMineWithPagingSortFilterAsync(int ownerId, GetMineRequest request)
		{
			if (request is null)
				return new BaseResponse<PagedResult<GetSummaryResponse>>("Request is null", StatusCodes.BadRequest, null);

			if (ownerId <= 0)
			{
				return new BaseResponse<PagedResult<GetSummaryResponse>>("Invalid owner id", StatusCodes.BadRequest, null);
			}

			Expression<Func<NewsArticle, bool>> ownerFilter = n => n.CreatedById == ownerId;
			var additional = BuildFilterFromOwnerRequest(request);
			Expression<Func<NewsArticle, bool>>? filter = additional is null ? ownerFilter : ExpressionExtensions.AndAlso(ownerFilter, additional);

			var (items, totalCount) = await _newsArticleRepository.GetPagedAsync(
				filter: filter,
				include: q => q.Include(n => n.Category).Include(n => n.CreatedBy),
				orderBy: q => q.ApplySorting(request.SortBy, request.IsDescending),
				pageNumber: request.PageNumber,
				pageSize: request.PageSize,
				asNoTracking: true
			);

			var mapped = _mapper.Map<IEnumerable<GetSummaryResponse>>(items);
			var paged = new PagedResult<GetSummaryResponse>(mapped, request.PageNumber, request.PageSize, totalCount);

			return new BaseResponse<PagedResult<GetSummaryResponse>>("News articles retrieved", StatusCodes.Ok, paged);
		}

		// Shared private helper methods
		private Expression<Func<NewsArticle, bool>>? BuildFilterFromAdminRequest(GetRequest request)
		{
			Expression<Func<NewsArticle, bool>>? filter = null;

			if (request.CreatedBy.HasValue)
			{
				Expression<Func<NewsArticle, bool>> createdByFilter = n => n.CreatedById == request.CreatedBy.Value;
				filter = filter is null ? createdByFilter : ExpressionExtensions.AndAlso(filter, createdByFilter);
			}

			if (request.UpdatedBy.HasValue)
			{
				Expression<Func<NewsArticle, bool>> updatedByFilter = n => n.UpdatedById == request.UpdatedBy.Value;
				filter = filter is null ? updatedByFilter : ExpressionExtensions.AndAlso(filter, updatedByFilter);
			}

			// Other shared filters
			filter = AppendSharedFilters(filter, request);
			return filter;
		}

		private Expression<Func<NewsArticle, bool>>? BuildFilterFromOwnerRequest(GetMineRequest request)
		{
			Expression<Func<NewsArticle, bool>>? filter = null;
			// Owner requests exclude CreatedBy/UpdatedBy fields from user input; caller should pass CreatedBy via auth if needed.
			filter = AppendSharedFilters(filter, request);
			return filter;
		}

		private Expression<Func<NewsArticle, bool>>? AppendSharedFilters(Expression<Func<NewsArticle, bool>>? filter, GetMineRequest request)
		{
			if (!string.IsNullOrWhiteSpace(request.NewsTitle))
			{
				var keyword = request.NewsTitle.Trim();
				Expression<Func<NewsArticle, bool>> titleFilter = n =>
					n.NewsTitle != null && (
						EF.Functions.Collate(n.NewsTitle, "Vietnamese_CI_AI").Contains(keyword) ||
						EF.Functions.Collate(n.NewsTitle, "Latin1_General_CI_AI").Contains(keyword)
					);
				filter = filter is null ? titleFilter : ExpressionExtensions.AndAlso(filter, titleFilter);
			}

			if (!string.IsNullOrWhiteSpace(request.Headline))
			{
				var keyword = request.Headline.Trim();
				Expression<Func<NewsArticle, bool>> headlineFilter = n =>
					n.Headline != null && (
						EF.Functions.Collate(n.Headline, "Vietnamese_CI_AI").Contains(keyword) ||
						EF.Functions.Collate(n.Headline, "Latin1_General_CI_AI").Contains(keyword)
					);
				filter = filter is null ? headlineFilter : ExpressionExtensions.AndAlso(filter, headlineFilter);
			}

			if (!string.IsNullOrWhiteSpace(request.NewsSource))
			{
				var keyword = request.NewsSource.Trim();
				Expression<Func<NewsArticle, bool>> sourceFilter = n =>
					n.NewsSource != null && (
						EF.Functions.Collate(n.NewsSource, "Vietnamese_CI_AI").Contains(keyword) ||
						EF.Functions.Collate(n.NewsSource, "Latin1_General_CI_AI").Contains(keyword)
					);
				filter = filter is null ? sourceFilter : ExpressionExtensions.AndAlso(filter, sourceFilter);
			}

			if (request.CategoryId.HasValue)
			{
				Expression<Func<NewsArticle, bool>> categoryFilter = n => n.CategoryId == request.CategoryId.Value;
				filter = filter is null ? categoryFilter : ExpressionExtensions.AndAlso(filter, categoryFilter);
			}

			if (request.NewsStatus.HasValue)
			{
				var status = (byte)request.NewsStatus.Value;
				Expression<Func<NewsArticle, bool>> statusFilter = n => n.NewsStatus == status;
				filter = filter is null ? statusFilter : ExpressionExtensions.AndAlso(filter, statusFilter);
			}

			if (request.CreatedDateFrom.HasValue)
			{
				Expression<Func<NewsArticle, bool>> fromFilter = n => n.CreatedDate >= request.CreatedDateFrom.Value;
				filter = filter is null ? fromFilter : ExpressionExtensions.AndAlso(filter, fromFilter);
			}

			if (request.CreatedDateTo.HasValue)
			{
				Expression<Func<NewsArticle, bool>> toFilter = n => n.CreatedDate <= request.CreatedDateTo.Value;
				filter = filter is null ? toFilter : ExpressionExtensions.AndAlso(filter, toFilter);
			}

			if (request.TagIds != null)
			{
				foreach (var id in request.TagIds)
				{
					var localId = id; // avoid closure issues
					Expression<Func<NewsArticle, bool>> tagContains = n => n.Tags.Any(t => t.TagId == localId);
					filter = filter is null ? tagContains : ExpressionExtensions.AndAlso(filter, tagContains);
				}
			}

			return filter;
		}

		public async Task<BaseResponse<string>> UpdateAsync(int id, UpdateRequest request)
		{
			if (request is null)
				return new BaseResponse<string>("Request is null", StatusCodes.BadRequest, null);

			var existing = await _newsArticleRepository.GetByIdAsync(id);
			if (existing == null)
				return new BaseResponse<string>("News article not found", StatusCodes.NotFound, null);

			var validation = await _updateRequestValidator.ValidateAsync(request);
			if (!validation.IsValid)
			{
				var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
				return new BaseResponse<string>(errors, StatusCodes.BadRequest, null);
			}

			_mapper.Map(request, existing);

			// Update tags if provided
			if (request.TagIds != null)
			{
				var tags = (await _tagRepository.GetAllAsync(t => request.TagIds.Contains(t.TagId))).ToList();
				existing.Tags.Clear();
				foreach (var tag in tags)
					existing.Tags.Add(tag);
			}

			_newsArticleRepository.Update(existing);
			var saved = await _newsArticleRepository.SaveChangesAsync();
			if (!saved)
				return new BaseResponse<string>("Failed to update news article", StatusCodes.InternalServerError, null);

			return new BaseResponse<string>("News article updated", StatusCodes.Ok, existing.NewsArticleId.ToString());
		}
	}
}

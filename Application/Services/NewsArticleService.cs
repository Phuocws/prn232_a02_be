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
		private readonly IGenericRepository<Category> _categoryRepository;
		private readonly IMapper _mapper;
		private readonly IValidator<CreateRequest> _createRequestValidator;
		private readonly IValidator<UpdateRequest> _updateRequestValidator;

		public NewsArticleService(IGenericRepository<NewsArticle> newsArticleRepository, IGenericRepository<Tag> tagRepository, IGenericRepository<Category> categoryRepository, IMapper mapper, IValidator<CreateRequest> createRequestValidator, IValidator<UpdateRequest> updateRequestValidator)
		{
			_newsArticleRepository = newsArticleRepository;
			_tagRepository = tagRepository;
			_categoryRepository = categoryRepository;
			_mapper = mapper;
			_createRequestValidator = createRequestValidator;
			_updateRequestValidator = updateRequestValidator;
		}

		public async Task<BaseResponse<string>> CreateAsync(int ownerId, CreateRequest request)
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
			// set owner from controller
			entity.CreatedById = ownerId;

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

			// Soft-delete: mark as inactive instead of removing
			existing.NewsStatus = (byte)NewsStatuses.Inactive;

			_newsArticleRepository.Update(existing);
			var saved = await _newsArticleRepository.SaveChangesAsync();
			if (!saved)
				return new BaseResponse<string>("Failed to deactivate news article", StatusCodes.InternalServerError, null);

			return new BaseResponse<string>("News article deactivated", StatusCodes.Ok, null);
		}

		public async Task<BaseResponse<GetDetailResponse>> GetByIdAsync(int id)
		{
			var entity = await _newsArticleRepository.GetByConditionAsync(
										na => na.NewsArticleId == id,
										na => na.Include(na => na.Category)
												.ThenInclude(c => c.InverseParentCategory)
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

			// If caller does not want to include inactive categories, compute allowed category ids where the whole ancestor chain is active
			if (!request.IncludeInactiveCategories)
			{
				var categories = (await _categoryRepository.GetAllAsync(asNoTracking: true)).ToList();
				var dict = categories.ToDictionary(c => c.CategoryId);

				var memo = new Dictionary<int, bool>();
				bool IsChainActive(int catId)
				{
					if (memo.TryGetValue(catId, out var val)) return val;
					if (!dict.TryGetValue(catId, out var cat))
					{
						memo[catId] = false; return false;
					}
					if (!cat.IsActive)
					{
						memo[catId] = false; return false;
					}
					if (!cat.ParentCategoryId.HasValue)
					{
						memo[catId] = true; return true;
					}
					var parentActive = IsChainActive(cat.ParentCategoryId.Value);
					memo[catId] = parentActive;
					return parentActive;
				}

				var allowedIds = categories.Where(c => IsChainActive(c.CategoryId)).Select(c => c.CategoryId).ToList();

				if (allowedIds.Count == 0)
				{
					// No allowed categories -> return empty paged result
					var emptyPaged = new PagedResult<GetSummaryResponse>(Enumerable.Empty<GetSummaryResponse>(), request.PageNumber, request.PageSize, 0);
					return new BaseResponse<PagedResult<GetSummaryResponse>>("News articles retrieved", StatusCodes.Ok, emptyPaged);
				}

				Expression<Func<NewsArticle, bool>> categoryFilter = n => allowedIds.Contains(n.CategoryId);
				filter = filter is null ? categoryFilter : ExpressionExtensions.AndAlso(filter, categoryFilter);
			}

			var (items, totalCount) = await _newsArticleRepository.GetPagedAsync(
				filter: filter,
				include: q => q.Include(n => n.Category).ThenInclude(c => c.ParentCategory).Include(n => n.CreatedBy),
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

			// If caller does not want to include inactive categories, compute allowed category ids where the whole ancestor chain is active
			if (!request.IncludeInactiveCategories)
			{
				var categories = (await _categoryRepository.GetAllAsync(asNoTracking: true)).ToList();
				var dict = categories.ToDictionary(c => c.CategoryId);

				var memo = new Dictionary<int, bool>();
				bool IsChainActive(int catId)
				{
					if (memo.TryGetValue(catId, out var val)) return val;
					if (!dict.TryGetValue(catId, out var cat))
					{
						memo[catId] = false; return false;
					}
					if (!cat.IsActive)
					{
						memo[catId] = false; return false;
					}
					if (!cat.ParentCategoryId.HasValue)
					{
						memo[catId] = true; return true;
					}
					var parentActive = IsChainActive(cat.ParentCategoryId.Value);
					memo[catId] = parentActive;
					return parentActive;
				}

				var allowedIds = categories.Where(c => IsChainActive(c.CategoryId)).Select(c => c.CategoryId).ToList();

				if (!allowedIds.Any())
				{
					var emptyPaged = new PagedResult<GetSummaryResponse>(Enumerable.Empty<GetSummaryResponse>(), request.PageNumber, request.PageSize, 0);
					return new BaseResponse<PagedResult<GetSummaryResponse>>("News articles retrieved", StatusCodes.Ok, emptyPaged);
				}

				Expression<Func<NewsArticle, bool>> categoryFilter = n => allowedIds.Contains(n.CategoryId);
				filter = filter is null ? categoryFilter : ExpressionExtensions.AndAlso(filter, categoryFilter);
			}

			var (items, totalCount) = await _newsArticleRepository.GetPagedAsync(
				filter: filter,
				include: q => q.Include(n => n.Category).ThenInclude(c => c.ParentCategory).Include(n => n.CreatedBy),
				orderBy: q => q.ApplySorting(request.SortBy, request.IsDescending),
				pageNumber: request.PageNumber,
				pageSize: request.PageSize,
				asNoTracking: true
			);

			var mapped = _mapper.Map<IEnumerable<GetSummaryResponse>>(items);
			var paged = new PagedResult<GetSummaryResponse>(mapped, request.PageNumber, request.PageSize, totalCount);

			return new BaseResponse<PagedResult<GetSummaryResponse>>("News articles retrieved", StatusCodes.Ok, paged);
		}

		public async Task<BaseResponse<GetStatisticsReportResponse>> GetReportAsync(GetStatisticsReportRequest request)
		{
			if (request == null)
				return new BaseResponse<GetStatisticsReportResponse>("Request is null", StatusCodes.BadRequest, null);

			if (request.EndDate < request.StartDate)
				return new BaseResponse<GetStatisticsReportResponse>("EndDate must be greater than or equal StartDate", StatusCodes.BadRequest, null);

			var start = request.StartDate.Date;
			var end = request.EndDate.Date.AddDays(1).AddTicks(-1);

			// fetch articles in range with category and author
			var (items, total) = await _newsArticleRepository.GetPagedAsync(
				filter: n => n.CreatedDate >= start && n.CreatedDate <= end,
				include: q => q.Include(n => n.Category).Include(n => n.CreatedBy),
				orderBy: null,
				pageNumber: 1,
				pageSize: int.MaxValue,
				asNoTracking: true
			);

			var itemList = items.ToList();
			var totalArticles = itemList.Count;

			// Load all categories to compute effective (ancestor-aware) active state
			var allCategories = (await _categoryRepository.GetAllAsync(asNoTracking: true)).ToList();
			var catDict = allCategories.ToDictionary(c => c.CategoryId);
			var catMemo = new Dictionary<int, bool>();

			bool IsChainActive(int catId)
			{
				if (catMemo.TryGetValue(catId, out var cached)) return cached;
				if (!catDict.TryGetValue(catId, out var cat))
				{
					catMemo[catId] = false; return false;
				}
				if (!cat.IsActive) { catMemo[catId] = false; return false; }
				if (!cat.ParentCategoryId.HasValue) { catMemo[catId] = true; return true; }
				var parentActive = IsChainActive(cat.ParentCategoryId.Value);
				catMemo[catId] = parentActive;
				return parentActive;
			}

			// Count inactive articles in the set (by NewsStatus)
			var inactiveArticlesCount = itemList.Count(n => n.NewsStatus == (byte)NewsStatuses.Inactive);

			// Total categories in system
			var totalCategories = allCategories.Count;

			// Count inactive categories across all categories using effective active
			var inactiveCategoriesCount = allCategories.Count(c => !IsChainActive(c.CategoryId));

			// Daily breakdown grouped by date (descending) including active/inactive split (by article status)
			var daily = itemList
				.GroupBy(n => n.CreatedDate.Date)
				.Select(g => new DailyStatistic
				{
					Date = g.Key,
					TotalArticles = g.Count(),
					ActiveArticles = g.Count(x => x.NewsStatus != (byte)NewsStatuses.Inactive),
					InactiveArticles = g.Count(x => x.NewsStatus == (byte)NewsStatuses.Inactive)
				})
				.OrderByDescending(d => d.Date)
				.ToList();

			// Build counts dictionary for articles per category in the period
			var articleCountsByCategory = itemList
				.GroupBy(n => n.CategoryId)
				.ToDictionary(g => g.Key, g => g.Count());

			// Category breakdown (include all categories, even with zero articles)
			var categoryGroups = allCategories
				.Select(c =>
				{
					var catId = c.CategoryId;
					var cnt = articleCountsByCategory.TryGetValue(catId, out var v) ? v : 0;
					var effectiveActive = IsChainActive(catId);
					return new StatisticBreakdown
					{
						ItemId = catId,
						ItemName = c.CategoryName + (effectiveActive ? string.Empty : " (inactive)"),
						TotalArticles = cnt,
						Percentage = totalArticles == 0 ? 0 : Math.Round((double)cnt * 100.0 / totalArticles, 2)
					};
				})
				.OrderByDescending(s => s.TotalArticles)
				.ThenBy(s => s.ItemName)
				.ToList();

			// Author breakdown remains the same
			var authorGroups = itemList
				.GroupBy(n => new { Id = n.CreatedById, Name = n.CreatedBy?.AccountName ?? string.Empty })
				.Select(g => new StatisticBreakdown
				{
					ItemId = g.Key.Id,
					ItemName = g.Key.Name,
					TotalArticles = g.Count(),
					Percentage = totalArticles == 0 ? 0 : Math.Round((double)g.Count() * 100.0 / totalArticles, 2)
				})
				.OrderByDescending(s => s.TotalArticles)
				.ToList();

			var response = new GetStatisticsReportResponse
			{
				StartDate = start,
				EndDate = end,
				TotalArticlesCreated = totalArticles,
				TotalCategories = totalCategories,
				InactiveCategoriesCount = inactiveCategoriesCount,
				InactiveArticlesCount = inactiveArticlesCount,
				DailyBreakdown = daily,
				CategoryBreakdown = categoryGroups,
				AuthorBreakdown = authorGroups
			};

			return new BaseResponse<GetStatisticsReportResponse>("Report generated", StatusCodes.Ok, response);
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

		public async Task<BaseResponse<string>> UpdateAsync(int ownerId, int id, UpdateRequest request)
		{
			if (request is null)
				return new BaseResponse<string>("Request is null", StatusCodes.BadRequest, null);

			var existing = await _newsArticleRepository.GetByConditionAsync(na => na.NewsArticleId == id, na => na.Include(na => na.Tags));

			if (existing == null)
				return new BaseResponse<string>("News article not found", StatusCodes.NotFound, null);

			var validation = await _updateRequestValidator.ValidateAsync(request);
			if (!validation.IsValid)
			{
				var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
				return new BaseResponse<string>(errors, StatusCodes.BadRequest, null);
			}

			_mapper.Map(request, existing);

			// set UpdatedById
			existing.UpdatedById = ownerId;

			// Update tags if provided - do diff instead of remove-all to avoid duplicate join inserts
			if (request.TagIds != null)
			{
				var desiredIds = request.TagIds.Distinct().ToList();

				existing.Tags ??= new List<Tag>();
				var existingIds = existing.Tags.Select(t => t.TagId).ToList();

				// Remove tags that are not desired
				var toRemove = existing.Tags.Where(t => !desiredIds.Contains(t.TagId)).ToList();
				foreach (var tag in toRemove)
				{
					existing.Tags.Remove(tag);
				}

				// Add tags that are desired but missing
				var toAddIds = desiredIds.Except(existingIds).ToList();
				if (toAddIds.Any())
				{
					var tags = (await _tagRepository.GetAllAsync(t => toAddIds.Contains(t.TagId))).ToList();
					foreach (var tag in tags)
						existing.Tags.Add(tag);
				}
			}

			_newsArticleRepository.Update(existing);
			var saved = await _newsArticleRepository.SaveChangesAsync();
			if (!saved)
				return new BaseResponse<string>("Failed to update news article", StatusCodes.InternalServerError, null);

			return new BaseResponse<string>("News article updated", StatusCodes.Ok, existing.NewsArticleId.ToString());
		}
	}
}

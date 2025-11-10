using Application.DTOs.Requests.Categories;
using Application.DTOs.Responses.Bases;
using Application.DTOs.Responses.Categories;
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
	public class CategoryService : ICategoryService
	{
		private readonly IGenericRepository<Category> _categoryRepository;
		private readonly IMapper _mapper;
		private readonly IValidator<CreateRequest> _createValidator;
		private readonly IValidator<UpdateRequest> _updateValidator;

		public CategoryService(IGenericRepository<Category> categoryRepository, IMapper mapper, IValidator<CreateRequest> createValidator, IValidator<UpdateRequest> updateValidator)
		{
			_categoryRepository = categoryRepository;
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

			// Optional: check unique name
			if (!string.IsNullOrWhiteSpace(request.CategoryName) && await _categoryRepository.AnyAsync(c => c.CategoryName == request.CategoryName))
				return new BaseResponse<string>("Category with the same name already exists.", StatusCodes.BadRequest, null);

			var entity = _mapper.Map<Category>(request);
			await _categoryRepository.AddAsync(entity);
			var saved = await _categoryRepository.SaveChangesAsync();
			if (!saved)
				return new BaseResponse<string>("Failed to create category", StatusCodes.InternalServerError, null);

			return new BaseResponse<string>("Category created", StatusCodes.Created, entity.CategoryId.ToString());
		}

		public async Task<BaseResponse<string>> DeleteAsync(int id)
		{
			var existing = await _categoryRepository.FirstOrDefaultAsync(
				predicate: c => c.CategoryId == id,
				include: q => q.Include(c => c.NewsArticles),
				asNoTracking: false
			);

			if (existing == null)
				return new BaseResponse<string>("Category not found", StatusCodes.NotFound, null);

			if (existing.NewsArticles != null && existing.NewsArticles.Count != 0)
				return new BaseResponse<string>("Cannot delete category because it has news articles.", StatusCodes.BadRequest, null);

			_categoryRepository.Remove(existing);
			var saved = await _categoryRepository.SaveChangesAsync();
			if (!saved)
				return new BaseResponse<string>("Failed to delete category", StatusCodes.InternalServerError, null);

			return new BaseResponse<string>("Category deleted", StatusCodes.Ok, null);
		}

		public async Task<BaseResponse<GetResponse>> GetByIdAsync(int id)
		{
			var entity = await _categoryRepository.GetByConditionAsync(c => c.CategoryId == id, include: c => c.Include(c => c.ParentCategory));
			if (entity == null)
				return new BaseResponse<GetResponse>("Category not found", StatusCodes.NotFound, null);

			var dto = _mapper.Map<GetResponse>(entity);
			return new BaseResponse<GetResponse>("Category retrieved", StatusCodes.Ok, dto);
		}

		public async Task<BaseResponse<List<GetDropdownResponse>>> GetDropdownAsync(GetDropdownRequest request)
		{
			if (request is null)
				return new BaseResponse<List<GetDropdownResponse>>("Request is null", StatusCodes.BadRequest, null);

			// Fetch all categories as a flat list via repository
			var allCategories = (await _categoryRepository.GetAllAsync(asNoTracking: true)).ToList();

			// Apply requested filters
			var filteredCategories = allCategories.AsEnumerable();

			if (!request.IncludeInactive)
			{
				filteredCategories = filteredCategories.Where(c => c.IsActive);
			}

			if (request.IncludeParentCategoriesOnly)
			{
				filteredCategories = filteredCategories.Where(c => !c.ParentCategoryId.HasValue);
			}

			var filteredList = filteredCategories.ToList();

			// Build the tree structure from the filtered set
			var lookup = new Dictionary<int, GetDropdownResponse>();
			var rootNodes = new List<GetDropdownResponse>();

			// Create node objects
			foreach (var item in filteredList)
			{
				lookup[item.CategoryId] = new GetDropdownResponse { Id = item.CategoryId, Name = item.CategoryName };
			}

			// Link children to parents (only link when both parent and child are present in the filtered set)
			foreach (var item in filteredList)
			{
				if (item.ParentCategoryId.HasValue && lookup.TryGetValue(item.ParentCategoryId.Value, out GetDropdownResponse? parent))
				{
					parent.Children.Add(lookup[item.CategoryId]);
				}
				else
				{
					rootNodes.Add(lookup[item.CategoryId]);
				}
			}

			return new BaseResponse<List<GetDropdownResponse>>("Categories retrieved.", StatusCodes.Ok, rootNodes);
		}

		public async Task<BaseResponse<PagedResult<GetResponse>>> GetWithPagingSortFilterAsync(GetRequest request)
		{
			if (request is null)
				return new BaseResponse<PagedResult<GetResponse>>("Request is null", StatusCodes.BadRequest, null);

			Expression<Func<Category, bool>>? filter = c => true;
			if (!string.IsNullOrWhiteSpace(request.CategoryName))
			{
				var keyword = request.CategoryName.Trim();
				Expression<Func<Category, bool>> nameFilter = c =>
					c.CategoryName != null && (
						EF.Functions.Collate(c.CategoryName, "Vietnamese_CI_AI").Contains(keyword) ||
						EF.Functions.Collate(c.CategoryName, "Latin1_General_CI_AI").Contains(keyword)
					);
				if (filter == null) filter = nameFilter; else filter = ExpressionExtensions.AndAlso(filter, nameFilter);
			}

			if (request.ParentCategoryId.HasValue)
			{
				Expression<Func<Category, bool>> parentFilter = c => c.ParentCategoryId == request.ParentCategoryId.Value;
				if (filter == null) filter = parentFilter; else filter = ExpressionExtensions.AndAlso(filter, parentFilter);
			}

			if (request.IsActive.HasValue)
			{
				Expression<Func<Category, bool>> activeFilter = c => c.IsActive == request.IsActive.Value;
				if (filter == null) filter = activeFilter; else filter = ExpressionExtensions.AndAlso(filter, activeFilter);
			}

			var (Items, TotalCount) = await _categoryRepository.GetPagedAsync(
				filter: filter,
				include: c => c.Include(c => c.ParentCategory),
				orderBy: q => q.ApplySorting(request.SortBy, request.IsDescending),
				pageNumber: request.PageNumber,
				pageSize: request.PageSize,
				asNoTracking: true
			);

			var items = Items;
			var totalCount = TotalCount;

			var mapped = _mapper.Map<IEnumerable<GetResponse>>(items);
			var paged = new PagedResult<GetResponse>(mapped, request.PageNumber, request.PageSize, totalCount);

			return new BaseResponse<PagedResult<GetResponse>>("Categories retrieved", StatusCodes.Ok, paged);
		}

		public async Task<BaseResponse<string>> UpdateAsync(int id, UpdateRequest request)
		{
			if (request is null)
				return new BaseResponse<string>("Request is null", StatusCodes.BadRequest, null);

			var existing = await _categoryRepository.GetByIdAsync(id);
			if (existing == null)
				return new BaseResponse<string>("Category not found", StatusCodes.NotFound, null);

			var validation = await _updateValidator.ValidateAsync(request);
			if (!validation.IsValid)
			{
				var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
				return new BaseResponse<string>("Validation failed", StatusCodes.BadRequest, errors);
			}

			// Check name uniqueness when updating
			if (!string.IsNullOrWhiteSpace(request.CategoryName) && await _categoryRepository.AnyAsync(c => c.CategoryName == request.CategoryName && c.CategoryId != id))
				return new BaseResponse<string>("Another category with the same name exists.", StatusCodes.BadRequest, null);

			_mapper.Map(request, existing);
			_categoryRepository.Update(existing);
			var saved = await _categoryRepository.SaveChangesAsync();
			if (!saved)
				return new BaseResponse<string>("Failed to update category", StatusCodes.InternalServerError, null);

			return new BaseResponse<string>("Category updated", StatusCodes.Ok, existing.CategoryId.ToString());
		}
	}
}

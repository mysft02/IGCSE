using AutoMapper;
using BusinessObject.DTOs.Request.Categories;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Categories;
using BusinessObject.Model;
using Common.Constants;
using Repository.IRepositories;
using BusinessObject.DTOs.Response.Courses;

namespace Service
{
    public class CategoryService
    {
        private readonly IMapper _mapper;
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(IMapper mapper, ICategoryRepository categoryRepository)
        {
            _mapper = mapper;
            _categoryRepository = categoryRepository;
        }

        public async Task<BaseResponse<CategoryResponse>> CreateCategoryAsync(CategoryRequest request)
        {
            // Check if category name already exists
            var existingCategories = await _categoryRepository.GetAllAsync();
            var nameExists = existingCategories.Any(c => c.CategoryName.ToLower() == request.CategoryName.ToLower());
            if (nameExists)
            {
                throw new Exception("Category name already exists");
            }

            // Create new category
            var category = new Category
            {
                CategoryName = request.CategoryName,
                Description = request.Description,
                IsActive = request.IsActive
            };

                var createdCategory = await _categoryRepository.AddAsync(category);

            var categoryResponse = _mapper.Map<CategoryResponse>(createdCategory);

            return new BaseResponse<CategoryResponse>(
                "Category created successfully",
                StatusCodeEnum.Created_201,
                categoryResponse
            );
        }

        public async Task<BaseResponse<CategoryResponse>> UpdateCategoryAsync(int categoryId, CategoryRequest request)
        {
            // Get existing category
            var existingCategory = await _categoryRepository.GetByCategoryIdAsync(categoryId);
            if (existingCategory == null)
            {
                throw new Exception("Category not found");
            }

            // Check if new category name already exists (excluding current category)
            var existingCategories = await _categoryRepository.GetAllAsync();

            // Update category properties
            existingCategory.CategoryName = request.CategoryName;
            existingCategory.Description = request.Description;
            existingCategory.IsActive = request.IsActive;

            var updatedCategory = await _categoryRepository.UpdateAsync(existingCategory);

            var categoryResponse = _mapper.Map<CategoryResponse>(updatedCategory);

            return new BaseResponse<CategoryResponse>(
                "Category updated successfully",
                StatusCodeEnum.OK_200,
                categoryResponse
            );
        }

        public async Task<BaseResponse<CategoryResponse>> GetCategoryByIdAsync(int categoryId)
        {
            var category = await _categoryRepository.GetByCategoryIdAsync(categoryId);
            if (category == null)
            {
                throw new Exception("Category not found");
            }

            var categoryResponse = _mapper.Map<CategoryResponse>(category);

            return new BaseResponse<CategoryResponse>(
                "Category retrieved successfully",
                StatusCodeEnum.OK_200,
                categoryResponse
            );
        }

        public async Task<BaseResponse<IEnumerable<CategoryResponse>>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            var categoryResponses = new List<CategoryResponse>();
            foreach (var category in categories)
            {
                var categoryResponse = _mapper.Map<CategoryResponse>(category);
                categoryResponses.Add(categoryResponse);
            }

            return new BaseResponse<IEnumerable<CategoryResponse>>(
                "Categories retrieved successfully",
                StatusCodeEnum.OK_200,
                categoryResponses
            );
        }

        public async Task<BaseResponse<IEnumerable<CategoryResponse>>> GetActiveCategoriesAsync()
        {
            var categories = await _categoryRepository.GetActiveCategoriesAsync();

            var categoryResponses = new List<CategoryResponse>();
            foreach (var category in categories)
            {
                var categoryResponse = _mapper.Map<CategoryResponse>(category);
                categoryResponses.Add(categoryResponse);
            }

            return new BaseResponse<IEnumerable<CategoryResponse>>(
                "Active categories retrieved successfully",
                StatusCodeEnum.OK_200,
                categoryResponses
            );
        }

        public async Task<BaseResponse<PaginatedResponse<CategoryResponse>>> GetCategoriesPagedAsync(CategoryListQuery query)
        {
            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

            var (items, total) = await _categoryRepository.SearchAsync(page, pageSize, query.SearchByName, query.IsActive);
            var responses = items.Select(i => _mapper.Map<CategoryResponse>(i)).ToList();

            var paginated = new PaginatedResponse<CategoryResponse>
            {
                Items = responses,
                TotalCount = total,
                Page = page - 1, // PaginatedResponse uses 0-based page
                Size = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };

            return new BaseResponse<PaginatedResponse<CategoryResponse>>(
                "Categories retrieved successfully",
                StatusCodeEnum.OK_200,
                paginated
            );
        }
    }
}

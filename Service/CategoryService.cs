using AutoMapper;
using BusinessObject.Model;
using Common.Constants;
using DTOs.Request.Categories;
using DTOs.Response.Categories;
using Repository.IRepositories;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using DTOs.Response.Accounts;

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

        public async Task<DTOs.Response.Accounts.BaseResponse<CategoryResponse>> CreateCategoryAsync(CategoryRequest request)
        {
            try
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

                return new DTOs.Response.Accounts.BaseResponse<CategoryResponse>(
                    "Category created successfully",
                    StatusCodeEnum.Created_201,
                    categoryResponse
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create category: {ex.Message}");
            }
        }

        public async Task<DTOs.Response.Accounts.BaseResponse<CategoryResponse>> UpdateCategoryAsync(int categoryId, CategoryRequest request)
        {
            try
            {
                // Get existing category
                var existingCategory = await _categoryRepository.GetByCategoryIdAsync(categoryId);
                if (existingCategory == null)
                {
                    throw new Exception("Category not found");
                }

                // Check if new category name already exists (excluding current category)
                var existingCategories = await _categoryRepository.GetAllAsync();
                var nameExists = existingCategories.Any(c => c.CategoryName.ToLower() == request.CategoryName.ToLower()
                                                           && c.CategoryID != categoryId);
                if (nameExists)
                {
                    throw new Exception("Category name already exists");
                }

                // Update category properties
                existingCategory.CategoryName = request.CategoryName;
                existingCategory.Description = request.Description;
                existingCategory.IsActive = request.IsActive;

                var updatedCategory = await _categoryRepository.UpdateAsync(existingCategory);

                var categoryResponse = _mapper.Map<CategoryResponse>(updatedCategory);

                return new DTOs.Response.Accounts.BaseResponse<CategoryResponse>(
                    "Category updated successfully",
                    StatusCodeEnum.OK_200,
                    categoryResponse
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update category: {ex.Message}");
            }
        }

        public async Task<DTOs.Response.Accounts.BaseResponse<CategoryResponse>> GetCategoryByIdAsync(int categoryId)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Failed to get category: {ex.Message}");
            }
        }

        public async Task<DTOs.Response.Accounts.BaseResponse<IEnumerable<CategoryResponse>>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();

                var categoryResponses = new List<CategoryResponse>();
                foreach (var category in categories)
                {
                    var categoryResponse = _mapper.Map<CategoryResponse>(category);
                    categoryResponses.Add(categoryResponse);
                }

                return new DTOs.Response.Accounts.BaseResponse<IEnumerable<CategoryResponse>>(
                    "Categories retrieved successfully",
                    StatusCodeEnum.OK_200,
                    categoryResponses
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get categories: {ex.Message}");
            }
        }

        public async Task<DTOs.Response.Accounts.BaseResponse<IEnumerable<CategoryResponse>>> GetActiveCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetActiveCategoriesAsync();

                var categoryResponses = new List<CategoryResponse>();
                foreach (var category in categories)
                {
                    var categoryResponse = _mapper.Map<CategoryResponse>(category);
                    categoryResponses.Add(categoryResponse);
                }

                return new DTOs.Response.Accounts.BaseResponse<IEnumerable<CategoryResponse>>(
                    "Active categories retrieved successfully",
                    StatusCodeEnum.OK_200,
                    categoryResponses
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get active categories: {ex.Message}");
            }
        }
    }
}

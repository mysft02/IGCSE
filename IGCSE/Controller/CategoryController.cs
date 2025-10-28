using BusinessObject.DTOs.Request.Categories;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Categories;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;

namespace IGCSE.Controller
{
    [Route("api/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost("create")]
        [SwaggerOperation(Summary = "Tạo category")]
        public async Task<ActionResult<BaseResponse<CategoryResponse>>> CreateCategory([FromForm] CategoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new BaseResponse<string>(
                    "Dữ liệu không hợp lệ",
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    string.Join(", ", errors)
                ));
            }

            var result = await _categoryService.CreateCategoryAsync(request);
            return CreatedAtAction(nameof(GetCategory), new { id = result.Data.CategoryID }, result);
        }

        [HttpPut("update/{id}")]
        [SwaggerOperation(Summary = "Cập nhật category")]
        public async Task<ActionResult<BaseResponse<CategoryResponse>>> UpdateCategory(int id, [FromForm] CategoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new BaseResponse<string>(
                    "Dữ liệu không hợp lệ",
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    string.Join(", ", errors)
                ));
            }

            var result = await _categoryService.UpdateCategoryAsync(id, request);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Lấy category theo id")]
        public async Task<ActionResult<BaseResponse<CategoryResponse>>> GetCategory(int id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("all")]
        [SwaggerOperation(Summary = "Lấy tất cả category")]
        public async Task<ActionResult<BaseResponse<IEnumerable<CategoryResponse>>>> GetAllCategories()
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            return Ok(result);
        }

        [HttpGet("active")]
        [SwaggerOperation(Summary = "Lấy category status active")]
        public async Task<ActionResult<BaseResponse<IEnumerable<CategoryResponse>>>> GetActiveCategories()
        {
            var result = await _categoryService.GetActiveCategoriesAsync();
            return Ok(result);
        }
    }
}

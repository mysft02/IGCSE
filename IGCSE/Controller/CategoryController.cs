using DTOs.Request.Categories;
using DTOs.Response.Categories;
using Microsoft.AspNetCore.Mvc;
using Service;

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
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<CategoryResponse>>> CreateCategory([FromBody] CategoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    "Dữ liệu không hợp lệ",
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    string.Join(", ", errors)
                ));
            }

            try
            {
                var result = await _categoryService.CreateCategoryAsync(request);
                return CreatedAtAction(nameof(GetCategory), new { id = result.Data.CategoryID }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<CategoryResponse>>> UpdateCategory(int id, [FromBody] CategoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    "Dữ liệu không hợp lệ",
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    string.Join(", ", errors)
                ));
            }

            try
            {
                var result = await _categoryService.UpdateCategoryAsync(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<CategoryResponse>>> GetCategory(int id)
        {
            try
            {
                var result = await _categoryService.GetCategoryByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.NotFound_404,
                    null
                ));
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<IEnumerable<CategoryResponse>>>> GetAllCategories()
        {
            try
            {
                var result = await _categoryService.GetAllCategoriesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<DTOs.Response.Accounts.BaseResponse<IEnumerable<CategoryResponse>>>> GetActiveCategories()
        {
            try
            {
                var result = await _categoryService.GetActiveCategoriesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new DTOs.Response.Accounts.BaseResponse<string>(
                    ex.Message,
                    Common.Constants.StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }
    }
}

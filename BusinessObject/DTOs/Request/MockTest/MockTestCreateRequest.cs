using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.Request.MockTest
{
    public class MockTestCreateRequest
    {
        [Required]
        [SwaggerSchema("Title of the mock test")]
        public string MockTestTitle { get; set; } = null!;

        [Required]
        [SwaggerSchema("Description of the mock test")]
        public string? MockTestDescription { get; set; }

        [Required]
        [SwaggerSchema("Excel File of the mock test")]
        public IFormFile? ExcelFile { get; set; } = null!;
    }
}

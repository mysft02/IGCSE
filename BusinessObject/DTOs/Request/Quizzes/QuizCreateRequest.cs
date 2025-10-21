using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.Request.Quizzes;

public class QuizCreateRequest
{
    [Required]
    public int CourseId { get; set; }
    
    [Required]
    public string QuizTitle { get; set; } = null!;

    [Required]
    public string? QuizDescription { get; set; }
    
    public IFormFile? ExcelFile { get; set; } = null!;
}

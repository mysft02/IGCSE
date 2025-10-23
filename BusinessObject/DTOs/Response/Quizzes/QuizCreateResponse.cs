using BusinessObject.Model;

namespace BusinessObject.DTOs.Response.Quizzes;

public class QuizCreateResponse
{
    public int CourseId { get; set; }
    
    public string QuizTitle { get; set; }

    public string QuizDescription { get; set; }

    public List<Question> Questions { get; set; } = new List<Question>();
}

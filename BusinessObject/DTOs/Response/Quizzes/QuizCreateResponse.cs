using BusinessObject.Model;

namespace BusinessObject.DTOs.Response.Quizzes;

public class QuizCreateResponse
{
    public int QuizId { get; set; }

    public int CourseId { get; set; }

    public int LessonId { get; set; }
    
    public string QuizTitle { get; set; }

    public string QuizDescription { get; set; }

    public List<QuestionCreateResponse> Questions { get; set; } = new List<QuestionCreateResponse>();
}

public class QuestionCreateResponse
{
    public int QuestionId { get; set; }

    public string QuestionContent { get; set; }

    public string CorrectAnswer { get; set; }
}

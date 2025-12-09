using Microsoft.AspNetCore.Http;

namespace BusinessObject.DTOs.Request.AptitudeTestQuestions
{
    public class AptitudeTestQuestionCreateRequest
    {
        public IFormFile Questions { get; set; }
    }
}

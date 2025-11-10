namespace BusinessObject.DTOs.Response.FinalQuizzes
{
    public class FinalQuizMarkResponse
    {
        public string Question { get; set; }

        public string Answer { get; set; }

        public string RightAnswer { get; set; }

        public bool IsCorrect { get; set; }

        public string Comment { get; set; }
    }
}

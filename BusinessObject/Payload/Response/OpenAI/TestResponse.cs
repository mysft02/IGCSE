namespace BusinessObject.Payload.Response.OpenAI
{ 
    public class TestResponse
    {
        public List<GradedQuestion> GradedQuestions { get; set; }
    }

    public class GradedQuestion
    {
        public string Question { get; set; }

        public string Answer { get; set; }

        public string RightAnswer { get; set; }

        public string Comment { get; set; }
    }
}

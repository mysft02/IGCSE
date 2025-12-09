namespace BusinessObject.Payload.Request.OpenAI
{
    public class QuestionMarkRequest
    {
        public string QuestionText { get; set; }

        public string Answer { get; set; }

        public string RightAnswer { get; set; }
    }
}

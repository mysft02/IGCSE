namespace BusinessObject.DTOs.Response.Accounts
{
    public class VerifyEmailResponse
    {
        public string Email { get; set; }
        public string Message { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}

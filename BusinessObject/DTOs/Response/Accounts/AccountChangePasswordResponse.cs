namespace DTOs.Response.Accounts
{
    public class AccountChangePasswordResponse
    {
        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? ConfirmPassword { get; set; }
    }
}

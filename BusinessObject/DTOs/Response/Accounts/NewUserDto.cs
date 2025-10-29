namespace BusinessObject.DTOs.Response.Accounts
{
    public class NewUserDto
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public bool? isActive { get; set; }
        public IList<string> Roles { get; set; }
    }
}

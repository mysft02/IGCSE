using Microsoft.AspNetCore.Identity;

namespace BusinessObject.Model
{
    public class Account : IdentityUser
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public string Phone { get; set; }

        public bool Status { get; set; }

        public DateOnly DateOfBirth { get; set; }
        public UserProfile UserProfile { get; set; }
    }
}

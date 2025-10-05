using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Response.Accounts
{
    public class AccountChangePasswordResponse
    {
        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? ConfirmPassword { get; set; }
    }
}

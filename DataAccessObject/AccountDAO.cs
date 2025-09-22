using BusinessObject.Model;
using DataAccessObject.BaseDAO;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessObject
{
    public class AccountDAO : BaseDAO<Account>
    {
        private readonly IGCSEContext _context;
        private readonly UserManager<Account> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AccountDAO(IGCSEContext context, UserManager<Account> userManager,
            RoleManager<IdentityRole> roleManager) : base(context)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<(int totalAccount, int studentsAccount, int parentsAccount, int teachersAccount, int adminAccount)> GetTotalAccount()
        {
            var studentRole = await _roleManager.FindByNameAsync("Student");
            var studentsCount = await _userManager.GetUsersInRoleAsync(studentRole.Name);

            var parentRole = await _roleManager.FindByNameAsync("Parent");
            var parentsCount = await _userManager.GetUsersInRoleAsync(parentRole.Name);

            var teacherRole = await _roleManager.FindByNameAsync("Teacher");
            var teachersCount = await _userManager.GetUsersInRoleAsync(teacherRole.Name);

            var adminRole = await _roleManager.FindByNameAsync("Admin");
            var adminsCount = await _userManager.GetUsersInRoleAsync(adminRole.Name);



            int totalAccountsCount = studentsCount.Count + parentsCount.Count + teachersCount.Count + adminsCount.Count;
            int studentsAccount = studentsCount.Count;
            int parentsAccount = parentsCount.Count;
            int teachersAccount = teachersCount.Count;
            int adminsAccount = adminsCount.Count;

            return (totalAccountsCount, studentsAccount, parentsAccount, teachersAccount, adminsAccount);
        }
    }
}

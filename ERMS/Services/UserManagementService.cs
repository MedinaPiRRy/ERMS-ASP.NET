using ERMS.Data;
using ERMS.Models;
using Microsoft.AspNetCore.Identity;

namespace ERMS.Services
{
    // This would basically help me save the employee normally to the Employees table, however, it will also help me create the
    // user in the Identity system which will later be used to authenticate the user.
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserManagementService(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<(bool Success, string ErrorMessage)> CreateEmployeeUserAsync(Employee employee, string password)
        {
            var existingUser = await _userManager.FindByEmailAsync(employee.Email);
            if (existingUser != null)
                return (false, "A user with this email already exists.");

            var identityUser = new IdentityUser
            {
                UserName = employee.Email,
                Email = employee.Email
            };

            var result = await _userManager.CreateAsync(identityUser, password);
            if (!result.Succeeded)
                return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

            if (!await _roleManager.RoleExistsAsync(employee.Role))
                await _roleManager.CreateAsync(new IdentityRole(employee.Role));

            await _userManager.AddToRoleAsync(identityUser, employee.Role);

            employee.IdentityUserId = identityUser.Id;
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool, string, List<string>)> CreateIdentityUserAsync(Employee employee, string password)
        {
            var existing = await _userManager.FindByEmailAsync(employee.Email);
            if (existing != null)
                return (false, null, new List<string> { "User already exists" });

            var user = new IdentityUser { UserName = employee.Email, Email = employee.Email };
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                return (false, null, result.Errors.Select(e => e.Description).ToList());

            if (!await _roleManager.RoleExistsAsync(employee.Role))
                await _roleManager.CreateAsync(new IdentityRole(employee.Role));

            await _userManager.AddToRoleAsync(user, employee.Role);

            return (true, user.Id, null);
        }
    }

}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ERMS.Data;
using ERMS.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

// This controller manages the Login and Register page making sure that users are saved to the database on Employees and IdentityUser tables

namespace ERMS.Controllers
{
    public class AccountController : Controller
    {
        // Initializing the UserManager, RoleManager, ApplicationDbContext and SignInManager
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(); 
        }

        // This method handles the login process.
        // It checks if the user exists and if the password is correct.
        // If successful, it redirects to the home page.
        // If not, it adds an error message to the ModelState and returns the login view.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user.UserName, 
                    password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "You’ve logged in successfully!";

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(); 
        }

        // This method handles the registration process.
        // It checks if the model state is valid and if the role exists.
        // If the user is created successfully, it adds the user to the role and saves the employee to the database.
        // If not, it adds an error message to the ModelState and returns the registration view.
        // It also assigns a default manager if none is provided. (Right now they cannot be provided at creation)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Employee model, string password)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("Validation error: " + error.ErrorMessage);
                }
                return View(model);
            }

            model.Manager = model.Manager ?? "Unassigned";

            var identityUser = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(identityUser, password);

            foreach (var error in result.Errors)
            {
                Console.WriteLine("Identity error: " + error.Description);
                ModelState.AddModelError("", error.Description);
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                ModelState.AddModelError("", $"Role '{model.Role}' does not exist.");
                return View(model);
            }


            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(identityUser, model.Role); 

                model.IdentityUserId = identityUser.Id;
                _context.Employees.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Registration successful! Welcome aboard";

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // This method handles the logout process.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

    }
}

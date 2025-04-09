using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ERMS.Data;
using ERMS.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Runtime.CompilerServices;

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
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            _logger.LogInformation("User accessed Login page");
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
            Console.WriteLine($"Login POST triggered: {email}");

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning($"Login failed: User {email} not found.");
                ModelState.AddModelError("", "User not found.");
                TempData["Error"] = "User not found.";
                return View();
            }

            if (user != null)
            {
                _logger.LogInformation($"User {email} found. Attempting to sign in.");
                var result = await _signInManager.PasswordSignInAsync(user.UserName, 
                    password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {email} logged in successfully.");
                    TempData["SuccessMessage"] = "You’ve logged in successfully!";

                    return RedirectToAction("Index", "Home");
                }

                if (!result.Succeeded)
                {
                    _logger.LogWarning($"Login failed: Incorrect password for user {email}.");
                    TempData["Error"] = "Incorrect password.";
                    return View();
                }
            }

            _logger.LogWarning($"Login failed: Invalid login attempt for user {email}.");
            ModelState.AddModelError("", "Invalid login attempt.");
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            _logger.LogInformation("User accessed Register page");
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
                _logger.LogWarning("Registration failed: Model state is invalid.");
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
                _logger.LogWarning("Registration failed: " + error.Description);
                Console.WriteLine("Identity error: " + error.Description);
                ModelState.AddModelError("", error.Description);
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                _logger.LogWarning($"Registration failed: Role '{model.Role}' does not exist.");
                ModelState.AddModelError("", $"Role '{model.Role}' does not exist.");
                return View(model);
            }


            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(identityUser, model.Role); 

                model.IdentityUserId = identityUser.Id;
                _context.Employees.Add(model);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {model.Email} registered successfully and added to role {model.Role}.");
                TempData["SuccessMessage"] = "Registration successful! Welcome aboard";

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                _logger.LogWarning("Registration failed: " + error.Description);
                ModelState.AddModelError("", error.Description);
            }

            _logger.LogWarning("Registration failed: User creation failed.");
            return View(model);
        }

        // This method handles the logout process.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("User logged out.");
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

    }
}

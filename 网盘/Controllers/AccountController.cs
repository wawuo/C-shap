// Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using WebDisk.Models;
using WebDisk.ViewModels;
using System.Threading.Tasks;

namespace WebDisk.Controllers
{
    // A controller that handles user account related actions
    public class AccountController : Controller
    {
        // A service for user authentication and authorization
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        // A constructor that injects the service
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // A GET action that returns the login view
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // A POST action that handles the login request
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Try to sign in the user with the given email and password
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    // If successful, redirect to the home page
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // If failed, add an error message and return the same view
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            return View(model);
        }

        // A GET action that returns the register view
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // A POST action that handles the register request
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create a new user with the given email and password
                var user = new User { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // If successful, sign in the user and redirect to the home page
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // If failed, add the error messages and return the same view
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            return View(model);
        }

        // A POST action that handles the logout request
        [HttpPost]
        [Authorize] // Only authorized users can access this action
        public async Task<IActionResult> Logout()
        {
            // Sign out the current user and redirect to the home page
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // A GET action that returns the access denied view
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

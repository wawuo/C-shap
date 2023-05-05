// Controllers/FileController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebDisk.Models;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace WebDisk.Controllers
{
    // A controller that handles file related actions
    public class FileController : Controller
    {
        // A service for user authentication and authorization
        private readonly UserManager<User> _userManager;

        // A database context for accessing file data
        private readonly WebDiskContext _context;

        // A constructor that injects the services
        public FileController(UserManager<User> userManager, WebDiskContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // A GET action that returns the index view with a list of files in a folder
        [HttpGet]
        [Authorize] // Only authorized users can access this action
        public async Task<ActionResult<File>> Index(int? id)
        {
            // Get the current user from the database
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            // Get the folder from the database by its id, or null if id is null or not found
            var folder = await _context.Files.FindAsync(id);

            if (id != null && folder == null)
            {
                return NotFound();
            }

            // Get the files that belong to the current user and the current folder from the database
            var files = await _context.Files.Where(f => f.UserId == user.Id && f.ParentId == id).ToListAsync();

            // Pass the files to the view using ViewBag
            ViewBag.Files = files;

            return View(folder);
        }
    }
}

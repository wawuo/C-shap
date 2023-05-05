// ViewModels/LoginViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace WebDisk.ViewModels
{
    // A class that represents the data for the login view
    public class LoginViewModel
    {
        // The email of the user
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // The password of the user
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        // A flag that indicates whether to remember the user
        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}

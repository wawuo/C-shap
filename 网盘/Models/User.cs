// Models/User.cs
using Microsoft.AspNetCore.Identity;

namespace WebDisk.Models
{
    // A class that represents a user of the web disk
    public class User : IdentityUser
    {
        // A navigation property for the files that belong to this user
        public virtual ICollection<File> Files { get; set; }
    }
}

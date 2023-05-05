// Models/File.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebDisk.Models
{
    // A class that represents a file or a folder in the web disk
    public class File
    {
        // The primary key of the file
        public int Id { get; set; }

        // The name of the file
        [Required]
        public string Name { get; set; }

        // The size of the file in bytes, or null if it is a folder
        public long? Size { get; set; }

        // The type of the file, such as "image/jpeg" or "application/pdf", or null if it is a folder
        public string Type { get; set; }

        // The date and time when the file was created
        public DateTime CreatedAt { get; set; }

        // The date and time when the file was last modified
        public DateTime ModifiedAt { get; set; }

        // The foreign key of the user who owns this file
        [ForeignKey("User")]
        public string UserId { get; set; }

        // A navigation property for the user who owns this file
        public virtual User User { get; set; }

        // The foreign key of the parent folder of this file, or null if it is in the root folder
        [ForeignKey("Parent")]
        public int? ParentId { get; set; }

        // A navigation property for the parent folder of this file
        public virtual File Parent { get; set; }

        // A navigation property for the child files or folders of this file, if it is a folder
        public virtual ICollection<File> Children { get; set; }
    }
}

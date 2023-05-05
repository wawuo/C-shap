// ViewModels/UploadViewModel.cs
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace WebDisk.ViewModels
{
    // A class that represents the data for the upload view
    public class UploadViewModel
    {
        // The file to be uploaded
        [Required]
        [Display(Name = "Select a file")]
        public IFormFile File { get; set; }

        // The id of the parent folder to upload to, or null if in the root folder
        public int? ParentId { get; set; }
    }
}

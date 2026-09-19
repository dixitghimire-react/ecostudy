using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EcoStudy.Models.ViewModels
{
    public class NoteEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Note Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        [Display(Name = "Note Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        [Display(Name = "Description / Overview")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a Chapter.")]
        [Display(Name = "Chapter")]
        public int ChapterId { get; set; }

        public string ExistingFileName { get; set; } = string.Empty;
        public string ExistingFileType { get; set; } = string.Empty;
        public long ExistingFileSize { get; set; }

        [Display(Name = "Replace File (Optional)")]
        public IFormFile? NewFile { get; set; }
    }
}

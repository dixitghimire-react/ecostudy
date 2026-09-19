using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EcoStudy.Models.ViewModels
{
    public class NoteUploadViewModel
    {
        [Required(ErrorMessage = "Note Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        [Display(Name = "Note Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        [Display(Name = "Note Description / Overview")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a Chapter.")]
        [Display(Name = "Chapter")]
        public int ChapterId { get; set; }

        [Required(ErrorMessage = "Please select a file to upload (PDF, DOC, DOCX, PPT, PPTX).")]
        [Display(Name = "Document File")]
        public IFormFile? File { get; set; }
    }
}

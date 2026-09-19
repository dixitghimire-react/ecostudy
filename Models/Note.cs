using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoStudy.Models
{
    public class Note
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Note title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        [Display(Name = "Note Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        [Display(Name = "Description / Summary")]
        public string? Description { get; set; }

        // Backwards compatibility alias for existing seed/views
        [NotMapped]
        public string Summary
        {
            get => Description ?? string.Empty;
            set => Description = value;
        }

        [NotMapped]
        public string Content
        {
            get => Description ?? string.Empty;
            set => Description = value;
        }

        [Required(ErrorMessage = "Please select a Chapter.")]
        [Display(Name = "Chapter")]
        public int ChapterId { get; set; }

        [ForeignKey("ChapterId")]
        public virtual Chapter? Chapter { get; set; }

        // Stored original file name for downloads
        [Required]
        [StringLength(255)]
        [Display(Name = "File Name")]
        public string FileName { get; set; } = string.Empty;

        // Relative path under wwwroot (e.g., "uploads/notes/{guid}.pdf")
        [Required]
        [StringLength(500)]
        [Display(Name = "File Path")]
        public string FilePath { get; set; } = string.Empty;

        // Extension (e.g., ".pdf", ".docx", ".pptx")
        [Required]
        [StringLength(20)]
        [Display(Name = "File Type")]
        public string FileType { get; set; } = string.Empty;

        // File size in bytes
        [Display(Name = "File Size")]
        public long FileSize { get; set; }

        public bool IsPublished { get; set; } = true;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public virtual ApplicationUser? Teacher { get; set; }
    }
}

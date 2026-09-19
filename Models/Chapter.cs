using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoStudy.Models
{
    public class Chapter
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Chapter Number is required.")]
        [Range(1, 999, ErrorMessage = "Chapter Number must be a positive number between 1 and 999.")]
        [Display(Name = "Chapter Number")]
        public int ChapterNumber { get; set; }

        [Required(ErrorMessage = "Chapter Name is required.")]
        [StringLength(150, ErrorMessage = "Chapter Name cannot exceed 150 characters.")]
        [Display(Name = "Chapter Name")]
        public string Name { get; set; } = string.Empty;

        // Alias for backward-compatibility with existing views
        [NotMapped]
        public string Title
        {
            get => Name;
            set => Name = value;
        }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        // Optional subject area/category
        [StringLength(100)]
        [Display(Name = "Subject Area")]
        public string? SubjectArea { get; set; } = "Economics";

        // Navigation properties
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
        public virtual ICollection<ImportantQuestion> ImportantQuestions { get; set; } = new List<ImportantQuestion>();
    }
}

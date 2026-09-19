using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoStudy.Models
{
    public class ImportantQuestion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The question text is required.")]
        [Display(Name = "Question")]
        public string Question { get; set; } = string.Empty;

        // Alias for backwards compatibility
        [NotMapped]
        public string QuestionText
        {
            get => Question;
            set => Question = value;
        }

        [Required(ErrorMessage = "Please select a Chapter.")]
        [Display(Name = "Chapter")]
        public int ChapterId { get; set; }

        [ForeignKey("ChapterId")]
        public virtual Chapter? Chapter { get; set; }

        [Required(ErrorMessage = "Please select a Question Type.")]
        [StringLength(50)]
        [Display(Name = "Question Type")]
        public string QuestionType { get; set; } = "Short"; // "Short", "Long", "Numerical"

        [Range(1, 50, ErrorMessage = "Marks must be between 1 and 50.")]
        [Display(Name = "Marks / Weightage")]
        public int Marks { get; set; } = 2;

        [StringLength(50)]
        [Display(Name = "Difficulty Level")]
        public string? DifficultyLevel { get; set; } = "Medium"; // "Easy", "Medium", "Hard"

        [StringLength(150)]
        [Display(Name = "Exam Reference / Year")]
        public string? ExamReference { get; set; } // e.g. "Board Exam 2023", "Sample Paper 2024"

        [Display(Name = "Answer Hint / Solution Outline")]
        public string? AnswerHint { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}


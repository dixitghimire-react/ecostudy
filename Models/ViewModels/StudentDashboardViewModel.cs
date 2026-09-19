using System.Collections.Generic;

namespace EcoStudy.Models.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string? StudentGrade { get; set; }
        public int TotalAvailableChapters { get; set; }
        public int TotalStudyNotes { get; set; }
        public int TotalPracticeQuestions { get; set; }

        public List<Chapter> Chapters { get; set; } = new List<Chapter>();
        public List<Note> FeaturedNotes { get; set; } = new List<Note>();
        public List<ImportantQuestion> HighYieldQuestions { get; set; } = new List<ImportantQuestion>();
    }
}

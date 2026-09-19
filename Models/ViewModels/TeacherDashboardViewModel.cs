using System.Collections.Generic;

namespace EcoStudy.Models.ViewModels
{
    public class TeacherDashboardViewModel
    {
        public string TeacherName { get; set; } = string.Empty;
        public int TotalChapters { get; set; }
        public int TotalNotes { get; set; }
        public int TotalImportantQuestions { get; set; }
        public int TotalStudents { get; set; }

        public List<Chapter> RecentChapters { get; set; } = new List<Chapter>();
        public List<Note> RecentNotes { get; set; } = new List<Note>();
        public List<ImportantQuestion> RecentQuestions { get; set; } = new List<ImportantQuestion>();
        public List<ApplicationUser> EnrolledStudents { get; set; } = new List<ApplicationUser>();
    }
}

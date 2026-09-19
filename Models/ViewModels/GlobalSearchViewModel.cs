using System.Collections.Generic;
using EcoStudy.Models;

namespace EcoStudy.Models.ViewModels
{
    public class GlobalSearchViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public List<Chapter> MatchingChapters { get; set; } = new List<Chapter>();
        public List<Note> MatchingNotes { get; set; } = new List<Note>();
        public List<ImportantQuestion> MatchingQuestions { get; set; } = new List<ImportantQuestion>();

        public int TotalResults => MatchingChapters.Count + MatchingNotes.Count + MatchingQuestions.Count;
    }
}

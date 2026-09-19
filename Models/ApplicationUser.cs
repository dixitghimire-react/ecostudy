using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace EcoStudy.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string UserRole { get; set; } = "Student"; // "Teacher" or "Student"
        public string? GradeOrLevel { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Note> AuthoredNotes { get; set; } = new List<Note>();
    }
}

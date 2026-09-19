using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoStudy.Data;
using EcoStudy.Models;
using EcoStudy.Models.ViewModels;
using EcoStudy.Services;

namespace EcoStudy.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileService _fileService;
        private readonly IWebHostEnvironment _environment;

        public StudentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IFileService fileService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _environment = environment;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            var viewModel = new StudentDashboardViewModel
            {
                StudentName = user?.FullName ?? "Student",
                StudentGrade = user?.GradeOrLevel ?? "Economics Learner",
                TotalAvailableChapters = await _context.Chapters.CountAsync(),
                TotalStudyNotes = await _context.Notes.CountAsync(n => n.IsPublished),
                TotalPracticeQuestions = await _context.ImportantQuestions.CountAsync(),
                Chapters = await _context.Chapters
                    .Include(c => c.Notes)
                    .Include(c => c.ImportantQuestions)
                    .OrderBy(c => c.ChapterNumber)
                    .ToListAsync(),
                FeaturedNotes = await _context.Notes
                    .Include(n => n.Chapter)
                    .Where(n => n.IsPublished)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(4)
                    .ToListAsync(),
                HighYieldQuestions = await _context.ImportantQuestions
                    .Include(q => q.Chapter)
                    .OrderByDescending(q => q.Marks)
                    .Take(4)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Chapters()
        {
            var chapters = await _context.Chapters
                .Include(c => c.Notes)
                .Include(c => c.ImportantQuestions)
                .OrderBy(c => c.ChapterNumber)
                .ToListAsync();

            return View(chapters);
        }

        [HttpGet]
        public async Task<IActionResult> ChapterDetails(int id)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Notes)
                .Include(c => c.ImportantQuestions)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chapter == null)
            {
                TempData["ErrorMessage"] = "The requested chapter was not found.";
                return RedirectToAction(nameof(Chapters));
            }

            return View(chapter);
        }

        public async Task<IActionResult> Notes(int? chapterId)
        {
            var query = _context.Notes
                .Include(n => n.Chapter)
                .Include(n => n.Teacher)
                .Where(n => n.IsPublished);

            if (chapterId.HasValue && chapterId.Value > 0)
            {
                query = query.Where(n => n.ChapterId == chapterId.Value);
            }

            var notes = await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
            ViewBag.Chapters = await _context.Chapters.OrderBy(c => c.ChapterNumber).ToListAsync();
            ViewBag.SelectedChapterId = chapterId;

            return View(notes);
        }

        [HttpGet]
        public async Task<IActionResult> NoteDetails(int id)
        {
            var note = await _context.Notes
                .Include(n => n.Chapter)
                .Include(n => n.Teacher)
                .Where(n => n.IsPublished)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (note == null)
            {
                TempData["ErrorMessage"] = "The requested note was not found.";
                return RedirectToAction(nameof(Notes));
            }

            return View(note);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadNote(int id)
        {
            var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id && n.IsPublished);
            if (note == null)
            {
                TempData["ErrorMessage"] = "The requested note does not exist or has been unpublished.";
                return RedirectToAction(nameof(Notes));
            }

            var fullPath = Path.Combine(_environment.WebRootPath, note.FilePath.TrimStart('/', '\\'));
            if (!System.IO.File.Exists(fullPath))
            {
                TempData["ErrorMessage"] = "The document file is temporarily unavailable on the server.";
                return RedirectToAction(nameof(Notes));
            }

            var contentType = _fileService.GetContentType(note.FileType);
            return PhysicalFile(fullPath, contentType, note.FileName);
        }

        public async Task<IActionResult> ImportantQuestions(int? chapterId, string? questionType, string? search)
        {
            var query = _context.ImportantQuestions.Include(q => q.Chapter).AsQueryable();

            if (chapterId.HasValue && chapterId.Value > 0)
            {
                query = query.Where(q => q.ChapterId == chapterId.Value);
            }

            if (!string.IsNullOrWhiteSpace(questionType) && questionType != "All")
            {
                query = query.Where(q => q.QuestionType == questionType);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.Trim().ToLower();
                query = query.Where(q => q.Question.ToLower().Contains(searchLower)
                    || (q.AnswerHint != null && q.AnswerHint.ToLower().Contains(searchLower))
                    || (q.ExamReference != null && q.ExamReference.ToLower().Contains(searchLower)));
            }

            var questions = await query
                .OrderBy(q => q.Chapter != null ? q.Chapter.ChapterNumber : 0)
                .ThenBy(q => q.QuestionType == "Short" ? 1 : (q.QuestionType == "Long" ? 2 : 3))
                .ThenByDescending(q => q.Marks)
                .ToListAsync();

            ViewBag.Chapters = await _context.Chapters.OrderBy(c => c.ChapterNumber).ToListAsync();
            ViewBag.SelectedChapterId = chapterId;
            ViewBag.SelectedQuestionType = questionType;
            ViewBag.SearchTerm = search;

            // Counts for filter pills
            var baseQuery = _context.ImportantQuestions.AsQueryable();
            if (chapterId.HasValue && chapterId.Value > 0)
            {
                baseQuery = baseQuery.Where(q => q.ChapterId == chapterId.Value);
            }
            ViewBag.TotalCount = await baseQuery.CountAsync();
            ViewBag.ShortCount = await baseQuery.CountAsync(q => q.QuestionType == "Short");
            ViewBag.LongCount = await baseQuery.CountAsync(q => q.QuestionType == "Long");
            ViewBag.NumericalCount = await baseQuery.CountAsync(q => q.QuestionType == "Numerical");

            return View(questions);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(string fullName, string? gradeOrLevel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                ModelState.AddModelError("FullName", "Full Name is required.");
                return View(user);
            }

            user.FullName = fullName;
            user.GradeOrLevel = gradeOrLevel;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(user);
        }
    }
}

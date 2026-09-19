using System;
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
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileService _fileService;
        private readonly IWebHostEnvironment _environment;

        public TeacherController(
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
            var students = await _userManager.GetUsersInRoleAsync("Student");

            var viewModel = new TeacherDashboardViewModel
            {
                TeacherName = user?.FullName ?? "Teacher",
                TotalChapters = await _context.Chapters.CountAsync(),
                TotalNotes = await _context.Notes.CountAsync(),
                TotalImportantQuestions = await _context.ImportantQuestions.CountAsync(),
                TotalStudents = students.Count,
                RecentChapters = await _context.Chapters.Include(c => c.Notes).Include(c => c.ImportantQuestions).OrderByDescending(c => c.CreatedAt).Take(5).ToListAsync(),
                RecentNotes = await _context.Notes.Include(n => n.Chapter).OrderByDescending(n => n.CreatedAt).Take(5).ToListAsync(),
                RecentQuestions = await _context.ImportantQuestions.Include(q => q.Chapter).OrderByDescending(q => q.CreatedAt).Take(5).ToListAsync(),
                EnrolledStudents = students.OrderByDescending(s => s.CreatedAt).Take(5).ToList()
            };

            return View(viewModel);
        }

        // ==========================================
        // CHAPTER MANAGEMENT (CRUD)
        // ==========================================

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
                TempData["ErrorMessage"] = $"Chapter with ID #{id} was not found.";
                return RedirectToAction(nameof(Chapters));
            }

            return View(chapter);
        }

        [HttpGet]
        public IActionResult CreateChapter()
        {
            int nextNumber = 1;
            if (_context.Chapters.Any())
            {
                nextNumber = _context.Chapters.Max(c => c.ChapterNumber) + 1;
            }

            var chapter = new Chapter
            {
                ChapterNumber = nextNumber
            };

            return View(chapter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateChapter(Chapter chapter)
        {
            if (await _context.Chapters.AnyAsync(c => c.ChapterNumber == chapter.ChapterNumber))
            {
                ModelState.AddModelError("ChapterNumber", $"A chapter with Number {chapter.ChapterNumber} already exists.");
            }

            if (ModelState.IsValid)
            {
                chapter.CreatedAt = DateTime.UtcNow;
                chapter.UpdatedAt = null;

                _context.Chapters.Add(chapter);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Chapter {chapter.ChapterNumber}: '{chapter.Name}' was successfully created!";
                return RedirectToAction(nameof(Chapters));
            }

            return View(chapter);
        }

        [HttpGet]
        public async Task<IActionResult> EditChapter(int id)
        {
            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null)
            {
                TempData["ErrorMessage"] = $"Chapter with ID #{id} was not found.";
                return RedirectToAction(nameof(Chapters));
            }

            return View(chapter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditChapter(int id, Chapter model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Chapter ID mismatch.";
                return RedirectToAction(nameof(Chapters));
            }

            if (await _context.Chapters.AnyAsync(c => c.Id != id && c.ChapterNumber == model.ChapterNumber))
            {
                ModelState.AddModelError("ChapterNumber", $"Another chapter already uses Number {model.ChapterNumber}.");
            }

            if (ModelState.IsValid)
            {
                var existingChapter = await _context.Chapters.FindAsync(id);
                if (existingChapter == null)
                {
                    TempData["ErrorMessage"] = "Chapter not found in database.";
                    return RedirectToAction(nameof(Chapters));
                }

                existingChapter.ChapterNumber = model.ChapterNumber;
                existingChapter.Name = model.Name;
                existingChapter.Description = model.Description;
                existingChapter.SubjectArea = model.SubjectArea;
                existingChapter.UpdatedAt = DateTime.UtcNow;

                _context.Chapters.Update(existingChapter);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Chapter {existingChapter.ChapterNumber}: '{existingChapter.Name}' updated successfully!";
                return RedirectToAction(nameof(Chapters));
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteChapter(int id)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Notes)
                .Include(c => c.ImportantQuestions)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chapter == null)
            {
                TempData["ErrorMessage"] = $"Chapter with ID #{id} was not found.";
                return RedirectToAction(nameof(Chapters));
            }

            return View(chapter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteChapter")]
        public async Task<IActionResult> DeleteChapterConfirmed(int id)
        {
            var chapter = await _context.Chapters.Include(c => c.Notes).FirstOrDefaultAsync(c => c.Id == id);
            if (chapter == null)
            {
                TempData["ErrorMessage"] = $"Chapter with ID #{id} could not be found.";
                return RedirectToAction(nameof(Chapters));
            }

            // Clean up files for notes in this chapter
            foreach (var note in chapter.Notes)
            {
                _fileService.DeleteFile(note.FilePath);
            }

            string chapterLabel = $"Chapter {chapter.ChapterNumber}: '{chapter.Name}'";
            _context.Chapters.Remove(chapter);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{chapterLabel} and all associated notes were permanently deleted.";
            return RedirectToAction(nameof(Chapters));
        }

        // ==========================================
        // NOTES MANAGEMENT (CRUD & DOWNLOADS)
        // ==========================================

        // 1. List All Notes
        [HttpGet]
        public async Task<IActionResult> Notes(int? chapterId)
        {
            var query = _context.Notes
                .Include(n => n.Chapter)
                .Include(n => n.Teacher)
                .AsQueryable();

            if (chapterId.HasValue && chapterId.Value > 0)
            {
                query = query.Where(n => n.ChapterId == chapterId.Value);
            }

            var notes = await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
            ViewBag.Chapters = await _context.Chapters.OrderBy(c => c.ChapterNumber).ToListAsync();
            ViewBag.SelectedChapterId = chapterId;

            return View(notes);
        }

        // 2. Upload Note (GET)
        [HttpGet]
        public async Task<IActionResult> UploadNote(int? chapterId)
        {
            ViewBag.Chapters = await _context.Chapters.OrderBy(c => c.ChapterNumber).ToListAsync();

            var model = new NoteUploadViewModel();
            if (chapterId.HasValue && chapterId.Value > 0)
            {
                model.ChapterId = chapterId.Value;
            }

            return View(model);
        }

        // 2. Upload Note (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadNote(NoteUploadViewModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError("File", "Please select a file to upload.");
            }

            var chapterExists = await _context.Chapters.AnyAsync(c => c.Id == model.ChapterId);
            if (!chapterExists)
            {
                ModelState.AddModelError("ChapterId", "Selected chapter does not exist.");
            }

            if (ModelState.IsValid)
            {
                var uploadResult = await _fileService.UploadNoteFileAsync(model.File!);
                if (!uploadResult.IsSuccess)
                {
                    ModelState.AddModelError("File", uploadResult.ErrorMessage ?? "File upload failed.");
                }
                else
                {
                    var user = await _userManager.GetUserAsync(User);
                    var note = new Note
                    {
                        Title = model.Title,
                        Description = model.Description,
                        ChapterId = model.ChapterId,
                        FileName = uploadResult.OriginalFileName!,
                        FilePath = uploadResult.FilePath!,
                        FileType = uploadResult.FileType!,
                        FileSize = uploadResult.FileSize,
                        TeacherId = user?.Id,
                        IsPublished = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Notes.Add(note);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Note '{note.Title}' was uploaded successfully!";
                    return RedirectToAction(nameof(Notes), new { chapterId = note.ChapterId });
                }
            }

            ViewBag.Chapters = await _context.Chapters.OrderBy(c => c.ChapterNumber).ToListAsync();
            return View(model);
        }

        // 3. View Note Details
        [HttpGet]
        public async Task<IActionResult> NoteDetails(int id)
        {
            var note = await _context.Notes
                .Include(n => n.Chapter)
                .Include(n => n.Teacher)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (note == null)
            {
                TempData["ErrorMessage"] = $"Note with ID #{id} was not found.";
                return RedirectToAction(nameof(Notes));
            }

            return View(note);
        }

        // 4. Edit Note Information (GET)
        [HttpGet]
        public async Task<IActionResult> EditNote(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                TempData["ErrorMessage"] = $"Note with ID #{id} was not found.";
                return RedirectToAction(nameof(Notes));
            }

            var model = new NoteEditViewModel
            {
                Id = note.Id,
                Title = note.Title,
                Description = note.Description,
                ChapterId = note.ChapterId,
                ExistingFileName = note.FileName,
                ExistingFileType = note.FileType,
                ExistingFileSize = note.FileSize
            };

            ViewBag.Chapters = await _context.Chapters.OrderBy(c => c.ChapterNumber).ToListAsync();
            return View(model);
        }

        // 4. Edit Note Information (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNote(int id, NoteEditViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Note ID mismatch.";
                return RedirectToAction(nameof(Notes));
            }

            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                TempData["ErrorMessage"] = "Note not found.";
                return RedirectToAction(nameof(Notes));
            }

            var chapterExists = await _context.Chapters.AnyAsync(c => c.Id == model.ChapterId);
            if (!chapterExists)
            {
                ModelState.AddModelError("ChapterId", "Selected chapter does not exist.");
            }

            if (ModelState.IsValid)
            {
                // If a new replacement file is uploaded
                if (model.NewFile != null && model.NewFile.Length > 0)
                {
                    var uploadResult = await _fileService.UploadNoteFileAsync(model.NewFile);
                    if (!uploadResult.IsSuccess)
                    {
                        ModelState.AddModelError("NewFile", uploadResult.ErrorMessage ?? "File upload failed.");
                        ViewBag.Chapters = await _context.Chapters.OrderBy(c => c.ChapterNumber).ToListAsync();
                        return View(model);
                    }

                    // Delete old physical file
                    _fileService.DeleteFile(note.FilePath);

                    // Update file metadata
                    note.FileName = uploadResult.OriginalFileName!;
                    note.FilePath = uploadResult.FilePath!;
                    note.FileType = uploadResult.FileType!;
                    note.FileSize = uploadResult.FileSize;
                }

                note.Title = model.Title;
                note.Description = model.Description;
                note.ChapterId = model.ChapterId;

                _context.Notes.Update(note);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Note '{note.Title}' updated successfully!";
                return RedirectToAction(nameof(Notes), new { chapterId = note.ChapterId });
            }

            ViewBag.Chapters = await _context.Chapters.OrderBy(c => c.ChapterNumber).ToListAsync();
            return View(model);
        }

        // 5. Delete Note (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNote(int id, string? returnUrl = null)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                TempData["ErrorMessage"] = $"Note with ID #{id} was not found.";
                return RedirectToAction(nameof(Notes));
            }

            string noteTitle = note.Title;
            int chapterId = note.ChapterId;

            // Remove physical file
            _fileService.DeleteFile(note.FilePath);

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Note '{noteTitle}' was deleted successfully.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Notes), new { chapterId });
        }

        // 6. Download Note (GET)
        [HttpGet]
        public async Task<IActionResult> DownloadNote(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                TempData["ErrorMessage"] = "The requested note does not exist.";
                return RedirectToAction(nameof(Notes));
            }

            var fullPath = Path.Combine(_environment.WebRootPath, note.FilePath.TrimStart('/', '\\'));
            if (!System.IO.File.Exists(fullPath))
            {
                TempData["ErrorMessage"] = "The attached document file could not be found on the server.";
                return RedirectToAction(nameof(Notes));
            }

            var contentType = _fileService.GetContentType(note.FileType);
            return PhysicalFile(fullPath, contentType, note.FileName);
        }

        // ==========================================
        // IMPORTANT QUESTIONS & STUDENTS
        // ==========================================

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

            // Summary statistics for badges
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
        public async Task<IActionResult> AddQuestion(int? chapterId, string? defaultType)
        {
            var chapters = await _context.Chapters.OrderBy(c => c.ChapterNumber).ToListAsync();
            if (!chapters.Any())
            {
                TempData["ErrorMessage"] = "Please create at least one chapter before adding questions.";
                return RedirectToAction(nameof(Chapters));
            }

            ViewBag.Chapters = chapters;

            var model = new ImportantQuestion
            {
                ChapterId = chapterId ?? chapters.First().Id,
                QuestionType = !string.IsNullOrEmpty(defaultType) ? defaultType : "Short",
                Marks = defaultType == "Long" ? 6 : (defaultType == "Numerical" ? 4 : 2),
                DifficultyLevel = "Medium"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(ImportantQuestion model)
        {
            if (string.IsNullOrWhiteSpace(model.Question))
            {
                ModelState.AddModelError(nameof(model.Question), "Question text is required.");
            }

            var chapter = await _context.Chapters.FindAsync(model.ChapterId);
            if (chapter == null)
            {
                ModelState.AddModelError(nameof(model.ChapterId), "The selected chapter does not exist.");
            }

            if (model.Marks < 1 || model.Marks > 50)
            {
                ModelState.AddModelError(nameof(model.Marks), "Marks must be between 1 and 50.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Chapters = await _context.Chapters.OrderBy(c => c.ChapterNumber).ToListAsync();
                return View(model);
            }

            model.Question = model.Question.Trim();
            model.ExamReference = model.ExamReference?.Trim();
            model.AnswerHint = model.AnswerHint?.Trim();
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = null;

            _context.ImportantQuestions.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"New {model.QuestionType} Question successfully added to Chapter {chapter!.ChapterNumber}.";
            return RedirectToAction(nameof(ImportantQuestions), new { chapterId = model.ChapterId, questionType = model.QuestionType });
        }

        [HttpGet]
        public async Task<IActionResult> EditQuestion(int id)
        {
            var question = await _context.ImportantQuestions
                .Include(q => q.Chapter)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                TempData["ErrorMessage"] = $"Question #{id} was not found.";
                return RedirectToAction(nameof(ImportantQuestions));
            }

            ViewBag.Chapters = await _context.Chapters.OrderBy(c => c.ChapterNumber).ToListAsync();
            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(int id, ImportantQuestion model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Invalid Question ID.";
                return RedirectToAction(nameof(ImportantQuestions));
            }

            var existing = await _context.ImportantQuestions.FindAsync(id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "The question you are trying to edit could not be found.";
                return RedirectToAction(nameof(ImportantQuestions));
            }

            if (string.IsNullOrWhiteSpace(model.Question))
            {
                ModelState.AddModelError(nameof(model.Question), "Question text is required.");
            }

            var chapter = await _context.Chapters.FindAsync(model.ChapterId);
            if (chapter == null)
            {
                ModelState.AddModelError(nameof(model.ChapterId), "The selected chapter does not exist.");
            }

            if (model.Marks < 1 || model.Marks > 50)
            {
                ModelState.AddModelError(nameof(model.Marks), "Marks must be between 1 and 50.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Chapters = await _context.Chapters.OrderBy(c => c.ChapterNumber).ToListAsync();
                return View(model);
            }

            existing.Question = model.Question.Trim();
            existing.ChapterId = model.ChapterId;
            existing.QuestionType = model.QuestionType;
            existing.Marks = model.Marks;
            existing.DifficultyLevel = model.DifficultyLevel;
            existing.ExamReference = model.ExamReference?.Trim();
            existing.AnswerHint = model.AnswerHint?.Trim();
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Question #{id} was successfully updated.";
            return RedirectToAction(nameof(ImportantQuestions), new { chapterId = existing.ChapterId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int id, string? returnUrl)
        {
            var question = await _context.ImportantQuestions
                .Include(q => q.Chapter)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                TempData["ErrorMessage"] = $"Question #{id} could not be found.";
            }
            else
            {
                _context.ImportantQuestions.Remove(question);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Question #{id} was successfully removed.";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(ImportantQuestions));
        }

        public async Task<IActionResult> Students()
        {
            var students = await _userManager.GetUsersInRoleAsync("Student");
            var sortedStudents = students.OrderByDescending(s => s.CreatedAt).ToList();
            return View(sortedStudents);
        }
    }
}

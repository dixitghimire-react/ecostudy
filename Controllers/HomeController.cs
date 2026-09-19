using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoStudy.Data;
using EcoStudy.Models;
using EcoStudy.Models.ViewModels;

namespace EcoStudy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // ==========================================
        // UNIFIED GLOBAL SEARCH
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Search(string? q)
        {
            var viewModel = new GlobalSearchViewModel
            {
                SearchTerm = q?.Trim() ?? string.Empty
            };

            if (string.IsNullOrWhiteSpace(viewModel.SearchTerm))
            {
                return View(viewModel);
            }

            var term = viewModel.SearchTerm.ToLower();

            // 1. Search Chapters (Name or Description)
            viewModel.MatchingChapters = await _context.Chapters
                .Include(c => c.Notes)
                .Include(c => c.ImportantQuestions)
                .Where(c => c.Name.ToLower().Contains(term)
                    || (c.Description != null && c.Description.ToLower().Contains(term))
                    || (c.SubjectArea != null && c.SubjectArea.ToLower().Contains(term)))
                .OrderBy(c => c.ChapterNumber)
                .ToListAsync();

            // 2. Search Notes (Title, Description, FileName)
            viewModel.MatchingNotes = await _context.Notes
                .Include(n => n.Chapter)
                .Where(n => n.Title.ToLower().Contains(term)
                    || (n.Description != null && n.Description.ToLower().Contains(term))
                    || n.FileName.ToLower().Contains(term))
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            // 3. Search Important Questions (Question, AnswerHint, ExamReference)
            viewModel.MatchingQuestions = await _context.ImportantQuestions
                .Include(iq => iq.Chapter)
                .Where(iq => iq.Question.ToLower().Contains(term)
                    || (iq.AnswerHint != null && iq.AnswerHint.ToLower().Contains(term))
                    || (iq.ExamReference != null && iq.ExamReference.ToLower().Contains(term))
                    || iq.QuestionType.ToLower().Contains(term))
                .OrderBy(iq => iq.Chapter != null ? iq.Chapter.ChapterNumber : 0)
                .ThenBy(iq => iq.QuestionType == "Short" ? 1 : (iq.QuestionType == "Long" ? 2 : 3))
                .ToListAsync();

            return View(viewModel);
        }

        // ==========================================
        // FRIENDLY STATUS CODE ERROR PAGES
        // ==========================================

        [Route("/Home/StatusCodePage")]
        public IActionResult StatusCodePage(int? code)
        {
            int statusCode = code ?? 404;
            ViewBag.StatusCode = statusCode;

            return View("StatusCodePage");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

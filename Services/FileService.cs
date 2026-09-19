using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace EcoStudy.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        // Allowed extensions: PDF, DOC, DOCX, PPT, PPTX
        private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".ppt", ".pptx" };

        // Max file size: 25 Megabytes
        private const long MaxFileSizeBytes = 25 * 1024 * 1024;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<(bool IsSuccess, string? FilePath, string? OriginalFileName, string? FileType, long FileSize, string? ErrorMessage)>
            UploadNoteFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return (false, null, null, null, 0, "No file was selected or the uploaded file is empty.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return (false, null, null, null, 0, $"File exceeds the maximum allowed size of {FormatFileSize(MaxFileSizeBytes)}.");
            }

            var originalFileName = Path.GetFileName(file.FileName);
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
            {
                string allowedList = string.Join(", ", _allowedExtensions.Select(e => e.ToUpperInvariant().TrimStart('.')));
                return (false, null, null, null, 0, $"Invalid file format '{extension}'. Only {allowedList} files are accepted.");
            }

            // Target directory: wwwroot/uploads/notes
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "notes");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate collision-proof unique filename
            var uniqueFileName = $"{Guid.NewGuid():N}_{SanitizeFileName(originalFileName)}";
            var absolutePath = Path.Combine(uploadsFolder, uniqueFileName);

            try
            {
                using (var stream = new FileStream(absolutePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Stored relative path under wwwroot
                var relativePath = Path.Combine("uploads", "notes", uniqueFileName).Replace('\\', '/');

                return (true, relativePath, originalFileName, extension, file.Length, null);
            }
            catch (Exception ex)
            {
                return (false, null, null, null, 0, $"Error saving file: {ex.Message}");
            }
        }

        public bool DeleteFile(string? relativeFilePath)
        {
            if (string.IsNullOrWhiteSpace(relativeFilePath))
            {
                return false;
            }

            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, relativeFilePath.TrimStart('/', '\\'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
            }
            catch
            {
                // Silently return false on failure
            }

            return false;
        }

        public string GetContentType(string? fileExtension)
        {
            var ext = fileExtension?.ToLowerInvariant().Trim();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                _ => "application/octet-stream"
            };
        }

        public string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";
            if (bytes < 1024 * 1024)
                return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F2} MB";
        }

        public string GetFileIcon(string? fileType)
        {
            var ext = fileType?.ToLowerInvariant().Trim();
            return ext switch
            {
                ".pdf" => "bi-filetype-pdf text-danger",
                ".doc" or ".docx" => "bi-filetype-docx text-primary",
                ".ppt" or ".pptx" => "bi-filetype-pptx text-warning",
                _ => "bi-file-earmark-text text-secondary"
            };
        }

        public string GetFileBadgeClass(string? fileType)
        {
            var ext = fileType?.ToLowerInvariant().Trim();
            return ext switch
            {
                ".pdf" => "badge-soft-danger",
                ".doc" or ".docx" => "badge-soft-primary",
                ".ppt" or ".pptx" => "badge-soft-warning",
                _ => "badge-soft-secondary"
            };
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName.Where(ch => !invalidChars.Contains(ch)).ToArray());
            return sanitized.Replace(" ", "_");
        }
    }
}

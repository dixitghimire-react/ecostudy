using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EcoStudy.Services
{
    public interface IFileService
    {
        Task<(bool IsSuccess, string? FilePath, string? OriginalFileName, string? FileType, long FileSize, string? ErrorMessage)>
            UploadNoteFileAsync(IFormFile file);

        bool DeleteFile(string? relativeFilePath);

        string GetContentType(string? fileExtension);

        string FormatFileSize(long bytes);

        string GetFileIcon(string? fileType);

        string GetFileBadgeClass(string? fileType);
    }
}

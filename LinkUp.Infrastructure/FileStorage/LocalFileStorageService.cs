using LinkUp.Application.Abstractions.Services;
using Microsoft.AspNetCore.Hosting;

namespace LinkUp.Infrastructure.FileStorage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveFileAsync(byte[] fileData, string fileName, string folder)
    {
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(fileName);
        var uniqueName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(uploadsDir, uniqueName);

        await File.WriteAllBytesAsync(fullPath, fileData);

        return $"/uploads/{folder}/{uniqueName}";
    }
}

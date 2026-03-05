namespace LinkUp.Application.Abstractions.Services;

public interface IFileStorageService
{
    // Saves file bytes and returns the relative URL path
    Task<string> SaveFileAsync(byte[] fileData, string fileName, string folder);
}

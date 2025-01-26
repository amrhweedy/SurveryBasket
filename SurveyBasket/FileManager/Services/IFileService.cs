namespace FileManager.Services;

public interface IFileService
{
    Task<Guid> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default);
}

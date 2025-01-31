namespace FileManager.Services;

public interface IFileService
{
    Task<Guid> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> UploadManyFilesAsync(IFormFileCollection files, CancellationToken cancellationToken = default);
    Task UploadImageAsync(IFormFile image, CancellationToken cancellationToken = default);
    Task<(byte[] fileContent, string contentType, string fileName)> DownloadFileAsync(Guid id, CancellationToken cancellationToken = default);

    // make stream for a video , we save the video in the server and display this video for the user not make the user download this video
    // we make the user watch the video from the server without downloading it (live stream)
    Task<(FileStream? stream, string contentType, string fileName)> StreamAsync(Guid id, CancellationToken cancellationToken = default);
}

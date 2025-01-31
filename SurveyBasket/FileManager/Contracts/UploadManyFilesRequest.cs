namespace FileManager.Contracts;

public record UploadManyFilesRequest(
    IFormFileCollection Files
    );

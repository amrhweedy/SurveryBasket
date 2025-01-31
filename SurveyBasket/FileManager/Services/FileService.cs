
using FileManager.Entities;
using FileManager.Persistence;

namespace FileManager.Services;

public class FileService(IWebHostEnvironment webHostEnvironment, ApplicationDbContext context) : IFileService
{
    // determine the physical path in the server to save the files in this path
    private readonly string _FilesPath = $"{webHostEnvironment.WebRootPath}/Uploads";
    private readonly string _ImagesPath = $"{webHostEnvironment.WebRootPath}/Images";
    private readonly ApplicationDbContext _context = context;

    public async Task<Guid> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default)
    {

        var uploadedFile = await SaveFileAtServer(file, cancellationToken);

        // save file in the database
        await _context.Files.AddAsync(uploadedFile, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return uploadedFile.Id;
    }


    public async Task<IEnumerable<Guid>> UploadManyFilesAsync(IFormFileCollection files, CancellationToken cancellationToken = default)
    {

        List<UploadedFile> uploadedFiles = [];

        foreach (var file in files)
        {

            var uploadedFile = await SaveFileAtServer(file, cancellationToken);

            uploadedFiles.Add(uploadedFile);

        }

        await _context.Files.AddRangeAsync(uploadedFiles, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);


        return uploadedFiles.Select(file => file.Id).ToList();

    }

    public async Task UploadImageAsync(IFormFile image, CancellationToken cancellationToken = default)
    {
        // save the image in the server only
        var path = Path.Combine(_ImagesPath, image.FileName);
        using var stream = File.Create(path);
        await image.CopyToAsync(stream, cancellationToken);

    }

    public async Task<(byte[] fileContent, string contentType, string fileName)> DownloadFileAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // first=> get the file from the database
        var file = await _context.Files.FindAsync(id);
        if (file is null)
            return ([], string.Empty, string.Empty);

        // second => determine the path of the file in the server
        var path = Path.Combine(_FilesPath, file.StoredFileName);

        // third => read the file content 
        byte[] fileContent = File.ReadAllBytes(path);

        return (fileContent, file.ContentType, file.FileName);
    }

    public async Task<(FileStream? stream, string contentType, string fileName)> StreamAsync(Guid id, CancellationToken cancellationToken = default)
    {

        // make stream for a video , we save the video in the server and display this video for the user not make the user download this video
        // we make the user watch the video from the server without downloading it (live stream)

        var file = await _context.Files.FindAsync(id);
        if (file is null)
            return (null, string.Empty, string.Empty);

        var path = Path.Combine(_FilesPath, file.StoredFileName);

        FileStream stream = File.OpenRead(path);

        return (stream, file.ContentType, file.FileName);
    }
    private async Task<UploadedFile> SaveFileAtServer(IFormFile file, CancellationToken cancellationToken)
    {
        // save the file in the server (wwwroot/Uploads folder) and save the file in the database

        // save the file in the server with the fake name with fake extension, because if the hacker gets this file he can not know the original file name or original file extension
        var fakeFileName = Path.GetRandomFileName();

        var uploadedFile = new UploadedFile
        {
            FileName = file.FileName, // we save the original file name in the database because if the user needs to download the file, i will download the file with the original file name
            StoredFileName = fakeFileName,
            ContentType = file.ContentType,
            FileExtension = Path.GetExtension(file.FileName)
        };


        // save the file in the server

        var path = Path.Combine(_FilesPath, fakeFileName);
        using var stream = File.Create(path);  // create an empty file in the server (wwwroot/uploads)
        await file.CopyToAsync(stream, cancellationToken); //copy the content of the uploaded file to this stream( the empty file in the server)


        return uploadedFile;
    }


}


using FileManager.Entities;
using FileManager.Persistence;

namespace FileManager.Services;

public class FileService(IWebHostEnvironment webHostEnvironment, ApplicationDbContext context) : IFileService
{
    // determine the physical path in the server to save the files in this path
    private readonly string _FilesPath = $"{webHostEnvironment.WebRootPath}/Uploads";
    private readonly ApplicationDbContext _context = context;

    public async Task<Guid> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default)
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


        // save file in the database
        await _context.Files.AddAsync(uploadedFile, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);


        return uploadedFile.Id;


    }
}

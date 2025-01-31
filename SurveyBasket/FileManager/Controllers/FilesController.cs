using FileManager.Contracts;
using FileManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.Controllers;
[Route("api/[controller]")]
[ApiController]
public class FilesController(IFileService fileService) : ControllerBase
{
    private readonly IFileService _fileService = fileService;

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] UploadFileRequest request, CancellationToken cancellationToken)
    {
        var fileId = await _fileService.UploadFileAsync(request.File, cancellationToken);

        return CreatedAtAction(nameof(Download), new { id = fileId }, null);
    }


    [HttpPost("upload_many_files")]
    public async Task<IActionResult> UploadManyFiles([FromForm] UploadManyFilesRequest request, CancellationToken cancellationToken)
    {
        var filesIds = await _fileService.UploadManyFilesAsync(request.Files, cancellationToken);

        return Ok(filesIds);
    }

    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage([FromForm] UploadImageRequest request, CancellationToken cancellationToken)
    {
        await _fileService.UploadImageAsync(request.Image, cancellationToken);

        return Created();
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var (fileContent, contentType, fileName) = await _fileService.DownloadFileAsync(id, cancellationToken);

        return fileContent is [] ? NotFound() : File(fileContent, contentType, fileName);

    }

    [HttpGet("stream/{id}")]
    public async Task<IActionResult> Stream([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var (stream, contentType, fileName) = await _fileService.StreamAsync(id, cancellationToken);

        return stream is null ? NotFound() : File(stream, contentType, fileName, true);

        //Without Range Processing(false or omitted)
        //❌ The browser has to download the entire file before playing the video.
        //❌ No seeking support—users cannot jump to different positions in the video.
        //❌ Not bandwidth efficient, especially for large videos.
        //
        //With Range Processing(true)
        //✅ Streaming support – Only necessary chunks are downloaded.
        //✅ Seeking enabled – Users can jump to different timestamps.
        //✅ Optimized performance – No need to fully download the video before watching.


    }
}

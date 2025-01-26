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

        return Created();
    }
}

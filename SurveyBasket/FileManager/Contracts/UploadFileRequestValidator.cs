using FileManager.Contracts.Common;
using FluentValidation;

namespace FileManager.Contracts;

public class UploadFileRequestValidator : AbstractValidator<UploadFileRequest>
{
    public UploadFileRequestValidator()
    {
        // validate the file size

        //RuleFor(x => x.File)
        //    .SetValidator(new FileSizeValidator());


        // we validate the file signature not the extension because the user can change the file extension for example convert exe to pdf

        RuleFor(x => x.File)
            .SetValidator(new FileSignatureValidator());
    }
}

using FileManager.Contracts.Common;
using FluentValidation;

namespace FileManager.Contracts;

public class UploadManyFilesRequestValidator : AbstractValidator<UploadManyFilesRequest>
{
    public UploadManyFilesRequestValidator()
    {
        RuleForEach(x => x.Files)
            .SetValidator(new FileSizeValidator());

        RuleForEach(x => x.Files)
            .SetValidator(new FileSignatureValidator());
    }
}

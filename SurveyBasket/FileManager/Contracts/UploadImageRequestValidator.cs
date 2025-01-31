using FileManager.Contracts.Common;
using FileManager.Settings;
using FluentValidation;

namespace FileManager.Contracts;

public class UploadImageRequestValidator : AbstractValidator<UploadImageRequest>
{
    public UploadImageRequestValidator()
    {

        RuleFor(x => x.Image)
            .SetValidator(new FileSizeValidator())
            .SetValidator(new FileSignatureValidator());

        RuleFor(x => x.Image)
            .Must((request, context) =>
            {
                var imageExtension = Path.GetExtension(request.Image.FileName.ToLower());
                return FileSettings.AllowedImageExtensions.Contains(imageExtension);
            })
            .WithMessage("the image extension is  not allowed")
            .When(x => x.Image is not null);
    }
}

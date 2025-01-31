using FileManager.Settings;
using FluentValidation;

namespace FileManager.Contracts.Common;

public class FileSizeValidator : AbstractValidator<IFormFile>
{
    public FileSizeValidator()
    {

        RuleFor(x => x)
                   .Must((request, context) =>
                   {
                       return request.Length <= FileSettings.MaxFileSizeInBytes;
                   })
                   .WithMessage($"max file size is {FileSettings.MaxFileSizeInMB} MB.")
                   .When(request => request is not null);

    }
}

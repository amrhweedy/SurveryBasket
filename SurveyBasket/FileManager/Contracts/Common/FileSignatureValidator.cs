using FileManager.Settings;
using FluentValidation;

namespace FileManager.Contracts.Common;

public class FileSignatureValidator : AbstractValidator<IFormFile>
{
    // we validate the file signature not the extension because the user can change the file extension for example convert exe to pdf

    public FileSignatureValidator()
    {
        RuleFor(x => x)
           .Must((request, context) =>
           {
               BinaryReader binary = new BinaryReader(request.OpenReadStream());
               var bytes = binary.ReadBytes(2);   // the first 2 bytes of the file is the file signature
               var fileSequenceHex = BitConverter.ToString(bytes);

               foreach (var signature in FileSettings.BlockedSignatures)
               {
                   if (signature.Equals(fileSequenceHex, StringComparison.OrdinalIgnoreCase))
                       return false;
               }

               return true;

           })
           .WithMessage("Not allowed file content")
           .When(request => request is not null);
    }
}

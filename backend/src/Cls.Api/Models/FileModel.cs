namespace Cls.Api.Models
{
    public class UploadMultipleFileModel
    {
        public List<IFormFile>? Files { get; set; } = default!;
    }
}

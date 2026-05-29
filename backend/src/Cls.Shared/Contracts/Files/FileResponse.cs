namespace Cls.Shared.Contracts.Files;

public class FileResponse 
{
    public int Id { get; set; }
    public required string OriginalFilename { get; set; }
    public required string StoredFilename { get; set; }
    public required string FilePath { get; set; }
    public int FileSize { get; set; }
    public required string MimeType { get; set; }
    public DateTime UploadedAt { get; set; }
    public int UploadedByUserId { get; set; }
}

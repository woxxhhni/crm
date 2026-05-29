namespace Cls.Shared.Contracts.Clients.FileGroups;

public class ClientFileGroupItemResponse
{
    public int Id { get; set; }
    public int ClientFileGroupId { get; set; }
    public int FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

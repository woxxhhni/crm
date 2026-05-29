namespace Cls.Shared.Contracts.Orders;

public class OrderNoteResponse
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string UserFullName { get; set; }
    public int? UserProfileFileId { get; set; }
    public string UserProfileUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ClientName { get; set; }
    public string ProviderName { get; set; }
    public List<FileResponse> Files { get; set; } = new();

}
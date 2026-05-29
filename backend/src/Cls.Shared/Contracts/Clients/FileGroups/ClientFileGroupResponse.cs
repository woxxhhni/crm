namespace Cls.Shared.Contracts.Clients.FileGroups;
public class ClientFileGroupResponse
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string Label { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ClientFileGroupItemResponse> GroupItems { get; set; } = new();
}

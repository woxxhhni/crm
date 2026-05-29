namespace Cls.Shared.Contracts.Orders;

public class OrderPaymentResponse
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string PaymentType { get; set; }
    public string? Description { get; set; }
    public string UserFullName { get; set; }
    public int? UserProfileFileId { get; set; }
    public string UserProfileUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<FileResponse> Files { get; set; } = new();
}
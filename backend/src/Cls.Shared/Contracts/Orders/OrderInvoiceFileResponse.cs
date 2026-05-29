namespace Cls.Shared.Contracts.Orders;

public class OrderInvoiceFileResponse
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

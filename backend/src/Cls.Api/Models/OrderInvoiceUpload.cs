namespace Cls.Api.Models;

public sealed class OrderSellInvoiceUpload
{
    public List<IFormFile>? SFiles { get; set; }
}

public sealed class OrderBuyInvoiceUpload
{
    public List<IFormFile>? BFiles { get; set; }
}

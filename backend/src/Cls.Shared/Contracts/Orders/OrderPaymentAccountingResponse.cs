namespace Cls.Shared.Contracts.Orders;

public class OrderPaymentAccountingResponse
{
    public string Name { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal CurrentBalance => TotalBalance - PaidAmount;
    public string Currency { get; set; }

    public List<OrderPaymentTransactionResponse> Transactions { get; set; } = new();

    public class OrderPaymentTransactionResponse
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string PaymentType { get; set; }
        public string UserFullName { get; set; }
        public int? UserProfileFileId { get; set; }
        public string UserProfileUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
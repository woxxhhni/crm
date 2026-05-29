namespace Cls.Shared.Contracts.Orders;

public class OrderEmployeesRequest
{
    public IList<int> UserIds { get; set; } = new List<int>();
}
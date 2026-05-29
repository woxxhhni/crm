using System.Text.Json.Serialization;
using Cls.Domain.Enums;

namespace Cls.Domain.Entities;

public class ClientOrderPayment : OrderPayment
{
    [JsonIgnore]
    public ICollection<ClientOrderPaymentFile> Files { get; private set; } = new List<ClientOrderPaymentFile>();

    private ClientOrderPayment() { }
    public ClientOrderPayment(int orderId, decimal amount, OrderPaymentType paymentType, string? description, List<int> fileIds, int userId)
        : base(orderId, amount, paymentType, description, userId)
    {
        AddFiles(fileIds, userId);
    }

    public new void Update(decimal amount, OrderPaymentType paymentType, string? description, List<int> fileIds, List<int> removedFileIds, int userId)
    {
        base.Update(amount, paymentType, description, userId);
        RemoveFiles(removedFileIds, userId);
        AddFiles(fileIds, userId);
    }

    private void AddFiles(List<int> fileIds, int userId)
    {
        fileIds.ForEach(x => Files.Add(new ClientOrderPaymentFile(x, userId)));
    }

    private void RemoveFiles(List<int> fileIds, int userId)
    {
        foreach (var fileId in fileIds)
        {
            var file = Files.FirstOrDefault(x => x.FileId == fileId);
            if(file == null) continue;
            file.Remove(userId);
        }
    }
}
using System.Text.Json.Serialization;
using Cls.Domain.Enums;

namespace Cls.Domain.Entities;

public class ProviderOrderPayment : OrderPayment
{
    [JsonIgnore]
    public ICollection<ProviderOrderPaymentFile> Files { get; private set; } = new List<ProviderOrderPaymentFile>();

    private ProviderOrderPayment() { }
    public ProviderOrderPayment(int orderId, decimal amount, OrderPaymentType paymentType, string? description, List<int> fileIds, int userId)
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
        fileIds.ForEach(x => Files.Add(new ProviderOrderPaymentFile(x, userId)));
    }

    private void RemoveFiles(List<int> fileIds, int userId)
    {
        foreach (var id in fileIds)
        {
            var file = Files.FirstOrDefault(x => x.FileId == id);
            if (file == null) continue;
            file.Remove(userId);
        }
    }
}
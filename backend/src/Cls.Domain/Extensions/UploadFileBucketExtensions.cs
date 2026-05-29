using Cls.Domain.Enums;

namespace Cls.Domain.Extensions;

/// <summary>
/// Maps UploadFileBucket enum values to organized MinIO folder paths.
/// This creates a clean hierarchical structure inside the storage bucket.
/// </summary>
public static class UploadFileBucketExtensions
{
    /// <summary>
    /// Returns the MinIO folder prefix for the given bucket category.
    /// Example: UploadFileBucket.ClientProfile → "clients/profiles/"
    /// </summary>
    public static string ToFolderPrefix(this UploadFileBucket bucket) => bucket switch
    {
        UploadFileBucket.ClientProfile      => "clients/profiles/",
        UploadFileBucket.ClientFileGroup     => "clients/documents/",
        UploadFileBucket.ProviderProfile     => "providers/profiles/",
        UploadFileBucket.ProviderFileGroup   => "providers/documents/",
        UploadFileBucket.EmployeeProfile     => "employees/profiles/",
        UploadFileBucket.SellInvoice         => "orders/invoices/sell/",
        UploadFileBucket.BuyInvoice          => "orders/invoices/buy/",
        UploadFileBucket.ClientPayment       => "orders/payments/client/",
        UploadFileBucket.ProviderPayment     => "orders/payments/provider/",
        UploadFileBucket.OrderChangeState    => "orders/status-changes/",
        UploadFileBucket.OrderStepChange     => "orders/steps/changes/",
        UploadFileBucket.OrderStepComplete   => "orders/steps/completions/",
        UploadFileBucket.OrderNote           => "orders/notes/",
        UploadFileBucket.GeneralBucket       => "general/",
        UploadFileBucket.SystemBackup        => "system/backups/",
        _                                    => "general/",
    };
}

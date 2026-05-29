using System.ComponentModel;

namespace Cls.Domain.Enums;

public enum UploadFileBucket
{
    [Description("general-bucket")]
    GeneralBucket, 
    [Description("client-file-group")]
    ClientFileGroup,
    [Description("client-profile")]
    ClientProfile, 
    [Description("provider-file-group")]
    ProviderFileGroup,
    [Description("provider-profile")]
    ProviderProfile,
    [Description("employee-profile")]
    EmployeeProfile,
    [Description("sell-invoice")]
    SellInvoice,
    [Description("buy-invoice")]
    BuyInvoice,
    [Description("order-change-state")]
    OrderChangeState,
    [Description("order-step-change")]
    OrderStepChange,
    [Description("order-step-complete")]
    OrderStepComplete,
    [Description("client-payment")]
    ClientPayment,
    [Description("provider-payment")]
    ProviderPayment,
    [Description("order-note")]
    OrderNote,
    [Description("system-backup")]
    SystemBackup
}

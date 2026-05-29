using Cls.Shared.Contracts.Orders;
using static Cls.Shared.Contracts.Orders.OrderPaymentAccountingResponse;

namespace Cls.Api.Services
{
    public static class OrderExcelService
    {
        public static byte[] ToExcelFile(this List<OrderPaymentTransactionResponse> transactions)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Transactions");

            worksheet.Cell(1, 1).Value = "User Name";
            worksheet.Cell(1, 2).Value = "Payment Type";
            worksheet.Cell(1, 3).Value = "Amount";
            worksheet.Cell(1, 4).Value = "Remaining Amount";
            worksheet.Cell(1, 5).Value = "Created";

          
            for (int i = 0; i < transactions.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = transactions[i].UserFullName;
                worksheet.Cell(i + 2, 2).Value = transactions[i].PaymentType;
                worksheet.Cell(i + 2, 3).Value = transactions[i].Amount;
                worksheet.Cell(i + 2, 4).Value = transactions[i].RemainingAmount;
                worksheet.Cell(i + 2, 5).Value = transactions[i].CreatedAt.ToString("yyyy-MM-dd");
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

    }
}

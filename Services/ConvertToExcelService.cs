using OfficeOpenXml;
using TransactionAPI.Interfaces;
using TransactionAPI.Models;

namespace TransactionAPI.Services
{
    public class ConvertToExcelService : IConvertToExcelService
    {
        public byte[] ConvertTransactionToExcel(Transaction transaction)
        {
            var memoryStream = new MemoryStream();
            var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("Transaction");

            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "Amount";
            worksheet.Cells[1, 3].Value = "Transaction Date";

            worksheet.Cells[2, 1].Value = transaction.Name;
            worksheet.Cells[2, 2].Value = $"${transaction.Amount:F2}";
            worksheet.Cells[2, 3].Value = transaction.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss");

            package.SaveAs(memoryStream);
            memoryStream.Position = 0;

            return memoryStream.ToArray();
        }
    }
}

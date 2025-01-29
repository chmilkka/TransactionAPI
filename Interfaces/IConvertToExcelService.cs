using TransactionAPI.Models;

namespace TransactionAPI.Interfaces
{
    public interface IConvertToExcelService
    {
        byte[] ConvertTransactionToExcel(Transaction transaction);
    }
}

using TransactionAPI.Models;

namespace TransactionAPI.Interfaces
{
    public interface ITransactionService
    {
        Task ImportTransactionsAsync(IFormFile file);
        Task<byte[]> ExportTransactionToExcelAsync(string transactionId);
        Task<IEnumerable<Transaction>> GetJanuaryTransactionsAsync();
        Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}

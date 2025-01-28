using TransactionAPI.Models;

namespace TransactionAPI.Interfaces
{
    public interface ITransactionService
    {
        Task ImportTransactionsAsync(IFormFile file);
    }
}

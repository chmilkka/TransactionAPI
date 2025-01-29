using TransactionAPI.Models;

namespace TransactionAPI.Interfaces
{
    public interface IParseCsvService
    {
        Task<List<Transaction>> ParseCsvFileAsync(IFormFile file);
    }
}

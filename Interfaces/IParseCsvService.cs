using TransactionAPI.Models;

namespace TransactionAPI.Interfaces
{
    public interface IParseCsvService
    {
        Task<List<TransactionFromImport>> ParseCsvFileAsync(IFormFile file);
    }
}

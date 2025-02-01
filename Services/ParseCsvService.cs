using CsvHelper;
using System.Globalization;
using TransactionAPI.Interfaces;
using TransactionAPI.Models;

namespace TransactionAPI.Services
{
    public class ParseCsvService : IParseCsvService
    {
        public async Task<List<TransactionFromImport>> ParseCsvFileAsync(IFormFile file)
        {
            var transactions = new List<TransactionFromImport>();
            using (var stream = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
            {
                await csv.ReadAsync();
                csv.ReadHeader();

                while (await csv.ReadAsync())
                {
                    var importTransaction = new TransactionFromImport
                    {
                        Id = csv.GetField<string>("transaction_id"),
                        Name = csv.GetField<string>("name"),
                        Email = csv.GetField<string>("email"),
                        Amount = Convert.ToDecimal(csv.GetField<string>("amount").Replace("$", "").Trim(), CultureInfo.InvariantCulture),
                        TransactionDate = DateTime.Parse(csv.GetField<string>("transaction_date")),
                        ClientLocation = csv.GetField<string>("client_location")
                    };

                    transactions.Add(importTransaction);
                }
            }

            return transactions;
        }
    }
}

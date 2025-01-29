using CsvHelper;
using System.Globalization;
using TransactionAPI.Interfaces;
using TransactionAPI.Models;

namespace TransactionAPI.Services
{
    public class ParseCsvService : IParseCsvService
    {
        public async Task<List<Transaction>> ParseCsvFileAsync(IFormFile file)
        {
            var transactions = new List<Transaction>();
            using (var stream = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
            {
                await csv.ReadAsync();
                csv.ReadHeader();

                while (await csv.ReadAsync())
                {
                    var transaction = new Transaction
                    {
                        Id = csv.GetField<string>("transaction_id"),
                        Name = csv.GetField<string>("name"),
                        Email = csv.GetField<string>("email"),
                        Amount = Convert.ToDecimal(csv.GetField<string>("amount").Replace("$", "").Trim(), CultureInfo.InvariantCulture),
                        TransactionDate = csv.GetField<DateTime>("transaction_date"),
                        ClientLocation = csv.GetField<string>("client_location")
                    };

                    transactions.Add(transaction);
                }
            }

            return transactions;
        }
    }
}

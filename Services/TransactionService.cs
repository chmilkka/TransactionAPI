using CsvHelper;
using CsvHelper.Configuration;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using TransactionAPI.Interfaces;
using TransactionAPI.Models;

namespace TransactionAPI.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly string _connectionString;

        decimal a = 103243.13m;
        public TransactionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task ImportTransactionsAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentNullException("No file uploaded.");
            }

            var transactions = await ParseCsvFile(file);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                foreach (var transaction in transactions)
                {
                    var existingTransaction = await connection.QueryFirstOrDefaultAsync<Transaction>(
                    @"SELECT transaction_id AS Id, 
                             name AS Name, 
                             email AS Email, 
                             amount AS Amount, 
                             transaction_date AS TransactionDate, 
                             client_location AS ClientLocation
                      FROM Transactions 
                      WHERE transaction_id = @Id",
                    new { Id = transaction.Id });

                    if (existingTransaction == null)
                    {
                        var insertQuery = @"INSERT INTO Transactions (transaction_id, name, email, amount, transaction_date, client_location)
                                    VALUES (@Id, @Name, @Email, @Amount, @TransactionDate, @ClientLocation)";
                        await connection.ExecuteAsync(insertQuery, transaction);
                    }
                    else if (existingTransaction.Name != transaction.Name ||
                        existingTransaction.Email != transaction.Email ||
                        existingTransaction.Amount != transaction.Amount ||
                        existingTransaction.TransactionDate != transaction.TransactionDate ||
                        existingTransaction.ClientLocation != transaction.ClientLocation)
                    {                                              
                        var updateQuery = @"UPDATE Transactions 
                                    SET name = @Name,
                                    email = @Email, 
                                    amount = @Amount,  
                                    transaction_date = @TransactionDate,
                                    client_location = @ClientLocation
                                    WHERE transaction_id = @TransactionId";

                        await connection.ExecuteAsync(updateQuery, transaction);         
                    }
                }
            }
        }

        private async Task<List<Transaction>> ParseCsvFile(IFormFile file)
        {
            var transactions = new List<Transaction>();
            using (var stream = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
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

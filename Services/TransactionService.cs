using CsvHelper;
using CsvHelper.Configuration;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using System.Globalization;
using TransactionAPI.Exceptions;
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

            var transactions = await ParseCsvFileAsync(file);

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
                                    WHERE transaction_id = @Id";

                        await connection.ExecuteAsync(updateQuery, transaction);
                    }
                }
            }
        }

        public async Task<byte[]> ExportTransactionToExcelAsync(string transactionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                SELECT 
                    transaction_id AS Id, 
                    name AS Name, 
                    email AS Email, 
                    amount AS Amount, 
                    transaction_date AS TransactionDate, 
                    client_location AS ClientLocation
                FROM Transactions 
                WHERE transaction_id = @Id;";

                var existingTransaction = await connection.QueryFirstOrDefaultAsync<Transaction>(query, new { Id = transactionId });

                if (existingTransaction == null)
                {
                    throw new TransactionNotFoundException();
                }
            
                var excelDocumentBytes = ConvertTransactionToExcel(existingTransaction);

                return excelDocumentBytes;
            }
        }

        public async Task<IEnumerable<Transaction>>GetJanuaryTransactionsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            {
                await connection.OpenAsync();

                var query = @"
                SELECT 
                    transaction_id AS Id, 
                    name AS Name, 
                    email AS Email, 
                    amount AS Amount, 
                    transaction_date AS TransactionDate, 
                    client_location AS ClientLocation
                FROM Transactions
                WHERE transaction_date >= @StartDate 
                      AND transaction_date < @EndDate";

                var parameters = new
                {
                    StartDate = new DateTime(2024, 1, 1, 0, 0, 0),
                    EndDate = new DateTime(2024, 2, 1, 0, 0, 0)
                };

                return await connection.QueryAsync<Transaction>(query, parameters);
            }           
        }

        private byte[] ConvertTransactionToExcel(Transaction transaction)
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

        private async Task<List<Transaction>> ParseCsvFileAsync(IFormFile file)
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

using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TransactionAPI.Exceptions;
using TransactionAPI.Extensions;
using TransactionAPI.Interfaces;
using TransactionAPI.Models;

namespace TransactionAPI.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly string _connectionString;
        private readonly IParseCsvService _parseCsvService;
        private readonly IConvertToExcelService _convertToExcelService;
        private readonly ITimeZoneService _timeZoneService;

        public TransactionService(IConfiguration configuration, IParseCsvService parseCsvService, IConvertToExcelService convertToExcelService, ITimeZoneService timeZoneService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _parseCsvService = parseCsvService;
            _convertToExcelService = convertToExcelService;
            _timeZoneService = timeZoneService;
        }

        public async Task ImportTransactionsAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentNullException("No file uploaded.");
            }

            var transactionsFromImport = await _parseCsvService.ParseCsvFileAsync(file);

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                foreach (var importTransaction in transactionsFromImport)
                {
                    var clientTimezone = _timeZoneService.GetIanaTimeZoneFromLocation(importTransaction.ClientLocation);
                
                    var formattedTransactionDate = _timeZoneService.ConvertToUtc(importTransaction.TransactionDate, clientTimezone);

                    var findTransactionByIdQuery = @"
                    SELECT transaction_id AS Id, 
                            name AS Name, 
                            email AS Email, 
                            amount AS Amount, 
                            transaction_date AS TransactionDate,
                            client_timezone AS ClientTimezone,
                            client_location AS ClientLocation
                    FROM ""Transactions""
                    WHERE transaction_id = @Id";

                    var insertQuery = @"
                    INSERT INTO ""Transactions"" (transaction_id, name, email, amount, transaction_date, client_timezone, client_location)
                    VALUES (@Id, @Name, @Email, @Amount, CAST(@TransactionDate AS timestamptz),@ClientTimezone, @ClientLocation)";

                    var updateQuery = @"
                    UPDATE ""Transactions"" 
                    SET name = @Name,
                        email = @Email, 
                        amount = @Amount,  
                        transaction_date = CAST(@TransactionDate AS timestamptz),
                        client_timezone = @ClientTimezone,
                        client_location = @ClientLocation
                    WHERE transaction_id = @Id";

                    var existingTransaction = await connection.QueryFirstOrDefaultAsync<Transaction>(findTransactionByIdQuery, new { Id = importTransaction.Id });

                    if (existingTransaction == null)
                    {
                        await connection.ExecuteAsync(insertQuery, new
                        {
                            Id = importTransaction.Id,
                            Name = importTransaction.Name,
                            Email = importTransaction.Email,
                            Amount = importTransaction.Amount,
                            TransactionDate = formattedTransactionDate,
                            clientTimezone = clientTimezone,
                            ClientLocation = importTransaction.ClientLocation
                        });
                    }
                    else if (existingTransaction.Name != importTransaction.Name ||
                        existingTransaction.Email != importTransaction.Email ||
                        existingTransaction.Amount != importTransaction.Amount ||
                        existingTransaction.TransactionDate != formattedTransactionDate ||
                        existingTransaction.ClientLocation != importTransaction.ClientLocation)
                    {
                        await connection.ExecuteAsync(updateQuery, new
                        {
                            Id = importTransaction.Id,
                            Name = importTransaction.Name,
                            Email = importTransaction.Email,
                            Amount = importTransaction.Amount,
                            TransactionDate = formattedTransactionDate,
                            clientTimezone = clientTimezone,
                            ClientLocation = importTransaction.ClientLocation
                        });
                    }
                }
            }
        }

        public async Task<byte[]> ExportTransactionToExcelAsync(string transactionId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
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
                FROM ""Transactions"" 
                WHERE transaction_id = @Id;";

                var existingTransaction = await connection.QueryFirstOrDefaultAsync<Transaction>(query, new { Id = transactionId });

                if (existingTransaction == null)
                {
                    throw new TransactionNotFoundException();
                }           

                return _convertToExcelService.ConvertTransactionToExcel(existingTransaction);
            }
        }

        public async Task<IEnumerable<Transaction>>GetJanuaryTransactionsAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            {
                await connection.OpenAsync();

                var query = @"
                SELECT 
                    transaction_id AS Id, 
                    name AS Name, 
                    email AS Email, 
                    amount AS Amount,
                    (transaction_date AT TIME ZONE client_timezone AT TIME ZONE 'UTC') AS TransactionDate, 
                    client_timezone AS ClientTimezone,
                    client_location AS ClientLocation
                FROM 
                    ""Transactions""
                WHERE                    
                    transaction_date AT TIME ZONE client_timezone AT TIME ZONE 'UTC'
                    BETWEEN @StartDate AND @EndDate;";



                var parameters = new
                {
                    StartDate = new DateTime(2024, 1, 1, 0, 0, 0),
                    EndDate = new DateTime(2024, 1, 31, 23, 59, 59)
                };

                return await connection.QueryAsync<Transaction>(query, parameters);
            }           
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime requestStartDate, DateTime requestEndDate)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            {
                await connection.OpenAsync();

                var query = @"
                SELECT 
                    transaction_id AS Id, 
                    name AS Name, 
                    email AS Email, 
                    amount AS Amount,
                    (transaction_date AT TIME ZONE client_timezone AT TIME ZONE 'UTC') AS TransactionDate, 
                    client_timezone AS ClientTimezone,
                    client_location AS ClientLocation
                FROM 
                    ""Transactions""
                WHERE                    
                    transaction_date AT TIME ZONE client_timezone AT TIME ZONE 'UTC'
                    BETWEEN @StartDate AND @EndDate;";

                return await connection.QueryAsync<Transaction>(query, new { StartDate = requestStartDate, EndDate = requestEndDate });
            }              
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeWithClientTimezoneAsync(DateTime startDate, DateTime endDate, string clientLocation)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            {
                var clientIanaTimezone = _timeZoneService.GetIanaTimeZoneFromLocation(clientLocation);

                var query = @"
                SELECT 
                    transaction_id AS Id, 
                    name AS Name, 
                    email AS Email, 
                    amount AS Amount,
                    (transaction_date AT TIME ZONE @UserTimeZone AT TIME ZONE 'UTC') AS TransactionDate, 
                    client_timezone AS ClientTimezone,
                    client_location AS ClientLocation
                FROM 
                    ""Transactions""
                WHERE                    
                    transaction_date AT TIME ZONE @UserTimeZone AT TIME ZONE 'UTC'
                    BETWEEN @StartDate AND @EndDate;";

                var transactions = await connection.QueryAsync<Transaction>(
                query,
                new
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    UserTimeZone = clientIanaTimezone
                });
                
                return transactions;
            }           
        }
    }
}

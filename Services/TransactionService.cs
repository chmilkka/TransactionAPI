using Dapper;
using GeoTimeZone;
using Microsoft.Data.SqlClient;
using System;
using System.Globalization;
using TimeZoneConverter;
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

            var transactions = await _parseCsvService.ParseCsvFileAsync(file);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                foreach (var transaction in transactions)
                {
                    var findTransactionByIdQuery = @"
                    SELECT transaction_id AS Id, 
                            name AS Name, 
                            email AS Email, 
                            amount AS Amount, 
                            transaction_date AS TransactionDate, 
                            client_location AS ClientLocation
                    FROM Transactions 
                    WHERE transaction_id = @Id";

                    var insertQuery = @"
                    INSERT INTO Transactions (transaction_id, name, email, amount, transaction_date, client_location)
                    VALUES (@Id, @Name, @Email, @Amount, @TransactionDate, @ClientLocation)";

                    var updateQuery = @"
                    UPDATE Transactions 
                    SET name = @Name,
                        email = @Email, 
                        amount = @Amount,  
                        transaction_date = @TransactionDate,
                        client_location = @ClientLocation
                    WHERE transaction_id = @Id";

                    var existingTransaction = await connection.QueryFirstOrDefaultAsync<Transaction>(findTransactionByIdQuery, new { Id = transaction.Id });

                    if (existingTransaction == null)
                    {
                        await connection.ExecuteAsync(insertQuery, transaction);
                    }
                    else if (existingTransaction.Name != transaction.Name ||
                        existingTransaction.Email != transaction.Email ||
                        existingTransaction.Amount != transaction.Amount ||
                        existingTransaction.TransactionDate != transaction.TransactionDate ||
                        existingTransaction.ClientLocation != transaction.ClientLocation)
                    {                      
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

                return _convertToExcelService.ConvertTransactionToExcel(existingTransaction);
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
                    AND transaction_date <= @EndDate";

                var parameters = new
                {
                    StartDate = new DateTime(2024, 1, 1, 0, 0, 0),
                    EndDate = new DateTime(2024, 2, 1, 0, 0, 0)
                };

                return await connection.QueryAsync<Transaction>(query, parameters);
            }           
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime requestStartDate, DateTime requestEndDate)
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
                WHERE 
                    FORMAT(transaction_date, 'yyyy-MM-dd HH:mm:ss') >= @StartDate 
                    AND FORMAT(transaction_date, 'yyyy-MM-dd HH:mm:ss') <= @EndDate;";

                return await connection.QueryAsync<Transaction>(query, new { StartDate = requestStartDate.ToDatabaseDateTimeFormat(), EndDate = requestEndDate.ToDatabaseDateTimeFormat() });
            }              
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeWithClientTimezoneAsync(DateTime startDate, DateTime endDate, string clientLocation)
        {
            var clientLocationParts = clientLocation.Split(',');

            double latitude = double.Parse(clientLocationParts[0], CultureInfo.InvariantCulture);
            double longitude = double.Parse(clientLocationParts[1], CultureInfo.InvariantCulture);

            var clientIanaTimezone = _timeZoneService.GetIanaTimeZone(latitude, longitude);

            DateTime expandedStartDate = startDate.AddHours(-24);
            DateTime expandedEndDate = endDate.AddHours(24);

            var expandedTransactions = await GetTransactionsByDateRangeAsync(expandedStartDate, expandedEndDate);

            foreach (var transaction in expandedTransactions)
            {
                var transactionClientLocationParts = transaction.ClientLocation.Split(',');

                double transactionLatitude = double.Parse(transactionClientLocationParts[0], CultureInfo.InvariantCulture);
                double transactionLongitude = double.Parse(transactionClientLocationParts[1], CultureInfo.InvariantCulture);
                string transactionIanaTimeZone = _timeZoneService.GetIanaTimeZone(transactionLatitude, transactionLongitude);

                if (transactionIanaTimeZone != clientIanaTimezone)
                {
                    TimeZoneInfo sourceZone = TZConvert.GetTimeZoneInfo(transactionIanaTimeZone);
                    TimeZoneInfo targetZone = TZConvert.GetTimeZoneInfo(clientIanaTimezone);

                    transaction.TransactionDate = TimeZoneInfo.ConvertTime(transaction.TransactionDate, sourceZone, targetZone);
                }
            }

            return expandedTransactions.Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate);
        }
    }
}

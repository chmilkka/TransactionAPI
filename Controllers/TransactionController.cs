using Azure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TransactionAPI.Interfaces;
using TransactionAPI.Models;

namespace TransactionAPI.Controllers
{
    /// <summary>
    /// Controller for managing transactions.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController(ITransactionService transactionService) : ControllerBase
    {
        /// <summary>
        /// Imports transactions from a CSV file.
        /// </summary>
        /// <param name="file">CSV file containing transactions.</param>
        ///<response code = "200" > OK: Transactions imported successfully.</response>
        ///<response code = "400" > Bad Request: If the provided file is null or invalid.</response>
        ///<response code = "500" > Internal Server Error: If an unexpected error occurs.</response>
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> ImportTransactionsFromCsv(IFormFile file)
        {
            await transactionService.ImportTransactionsAsync(file);

            return Ok("Transactions imported successfully.");
        }

        /// <summary>
        /// Exports a transaction to an Excel file.
        /// </summary>
        /// <param name="transactionId">The unique identifier of the transaction.</param>
        ///<response code = "200" > OK: If the transaction was successfully exported.</response>
        ///<response code = "404" > Not Found: If the transaction with the given ID does not exist.</response>
        ///<response code = "500" > Internal Server Error: If an unexpected error occurs.</response>
        [HttpGet("export-transaction/{transactionId}")]
        [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> ExportTransactionToExcel(string transactionId)
        {
            var excelFile = await transactionService.ExportTransactionToExcelAsync(transactionId);            

            return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "transaction.xlsx");
        }

        /// <summary>
        /// Retrieves all transactions that occurred in January 2024.
        /// </summary>
        ///<response code = "200" > OK: Returns the list of transactions.</response>
        ///<response code = "500" > Internal Server Error: If an unexpected error occurs.</response>
        [HttpGet("transactions/january-2024")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Transaction>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> GetJanuaryTransactions()
        {
            var transactions = await transactionService.GetJanuaryTransactionsAsync();
            return Ok(transactions);
        }

        /// <summary>
        /// Retrieves transactions that occurred within the specified date range.
        /// </summary>
        /// <param name="startDate">Start date of the range.</param>
        /// <param name="endDate">End date of the range.</param>       
        ///<response code = "200" > OK: Returns the list of transactions within the given date range.</response>
        ///<response code = "400" > Bad Request: If the provided date range is invalid (e.g., null or incorrect).</response>
        ///<response code = "500" > Internal Server Error: If an unexpected error occurs.</response>
        [HttpGet("range")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Transaction>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> GetTransactionsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {         
            var transactions = await transactionService.GetTransactionsByDateRangeAsync(startDate, endDate);
            return Ok(transactions);
        }

        /// <summary>
        /// Retrieves transactions within a date range, adjusting for the client's timezone.
        /// </summary>
        /// <param name="startDate">Start date of the range.</param>
        /// <param name="endDate">End date of the range.</param>
        /// <param name="clientLocation">Client's geographical coordinates (latitude, longitude).</param>
        /// <returns>
        /// - 200 OK: Returns the list of transactions within the given date range adjusted for the client's timezone.
        /// - 500 Internal Server Error: If an unexpected error occurs.
        /// </returns>
        ///<response code = "200" > OK: Returns the list of transactions within the given date range adjusted for the client's timezone.</response>
        ///<response code = "500" > Internal Server Error: If an unexpected error occurs.</response>
        [HttpGet("range-with-client-timezone")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Transaction>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> GetTransactionsByDateRangeWithClientTimezone([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string clientLocation)
        {
            var transactions = await transactionService.GetTransactionsByDateRangeWithClientTimezoneAsync(startDate, endDate, clientLocation);
            return Ok(transactions);
        }
    }
}

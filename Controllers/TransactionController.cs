using Microsoft.AspNetCore.Mvc;
using TransactionAPI.Interfaces;

namespace TransactionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController(ITransactionService transactionService) : ControllerBase
    {       
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportTransactionsFromCsv(IFormFile file)
        {
            await transactionService.ImportTransactionsAsync(file);

            return Ok("Transactions imported successfully.");
        }

        [HttpGet("export-transaction/{transactionId}")]
        [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        public async Task<IActionResult> ExportTransactionToExcel(string transactionId)
        {
            var excelFile = await transactionService.ExportTransactionToExcelAsync(transactionId);            

            return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "transaction.xlsx");
        }

        [HttpGet("transactions/january-2024")]
        public async Task<IActionResult> GetJanuaryTransactions()
        {
            var transactions = await transactionService.GetJanuaryTransactionsAsync();
            return Ok(transactions);
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetTransactionsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {         
            var transactions = await transactionService.GetTransactionsByDateRangeAsync(startDate, endDate);
            return Ok(transactions);
        }
    }
}

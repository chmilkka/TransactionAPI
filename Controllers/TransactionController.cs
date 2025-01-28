using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Formats.Asn1;
using System.Globalization;
using TransactionAPI.Interfaces;
using TransactionAPI.Models;

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
            try
            {
                await transactionService.ImportTransactionsAsync(file);

                return Ok("Transactions imported successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing file: {ex.Message}");
            }
        }
    }
}

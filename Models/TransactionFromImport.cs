namespace TransactionAPI.Models
{
    public class TransactionFromImport
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ClientLocation { get; set; }
    }
}

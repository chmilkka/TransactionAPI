namespace TransactionAPI.Models
{
    /// <summary>
    /// Represents a financial transaction.
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Unique identifier for the transaction.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name associated with the transaction.
        /// </summary>

        public string Name { get; set; }

        /// <summary>
        /// Email of the transaction owner.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Transaction amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The date and time when the transaction occurred(in client local time).
        /// </summary>
        public DateTimeOffset TransactionDate { get; set; }

        public string ClientTimezone { get; set; }

        /// <summary>
        /// Geographical location of the client involved in the transaction.
        /// </summary>
        public string ClientLocation { get; set; }
    }
}

namespace TransactionAPI.Exceptions
{
    public class TransactionNotFoundException : Exception
    { 
        public TransactionNotFoundException() : base($"Transaction was not found.") { }
    }
}

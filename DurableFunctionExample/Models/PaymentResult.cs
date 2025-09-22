namespace DurableFunctionExample.Models
{
    public class PaymentResult
    {
        public string FailureReason { get; set; }
        public bool Success { get; set; }
        public string TransactionId { get; set; }
    }
}
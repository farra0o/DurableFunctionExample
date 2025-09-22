namespace DurableFunctionExample.Models
{
    internal class Order
    {
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public string customerID { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public string productID { get; set; }
        public decimal Total { get; set; }
    }
}
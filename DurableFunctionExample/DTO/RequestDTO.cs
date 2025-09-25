namespace DurableFunctionExample.DTO
{
    public class RequestDTO
    {
        public string UserEmail { get; set; }
        public string userName { get; set; }
        public List<CreateItemOrderDTO> Items { get; set; }
    }
}

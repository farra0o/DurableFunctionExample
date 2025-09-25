namespace DurableFunctionExample.DTO
{
    public class RequestDTO
    {
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public List<CreateItemOrderDTO> Items { get; set; }
    }
}

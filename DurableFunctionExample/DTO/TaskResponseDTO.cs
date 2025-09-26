using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunctionExample.DTO
{
    public class TaskResponseDTO
    {
        public int OrderId {get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
      

    }
}

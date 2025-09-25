using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunctionExample.DTO
{
    public class UserWithRequestDTO
    { 
        public int UserId { get; set; }
        public RequestDTO Request { get; set; }
    }
}

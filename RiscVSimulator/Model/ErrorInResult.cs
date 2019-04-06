using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiscVSimulator.Model
{
    public class ErrorInResult : Exception
    {
        public int Line { get; set; }
        public string Message { get; set; }
    }
}

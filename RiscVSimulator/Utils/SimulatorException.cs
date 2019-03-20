using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiscVSimulator.Utils
{
    public class SimulatorException : Exception
    {
            public string ErrorMessage { get; set; }
        
    }
}

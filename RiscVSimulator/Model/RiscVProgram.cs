using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiscVSimulator.Model
{
    public class RiscVProgram
    {
        public string Program { get; set; }
        public List<Register> Registers { get; set; }
        public bool DebugMode { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiscVSimulator.Model
{
    public class RiscVProgram
    {
        public Register[] Registers { get; set; }
        public string Program { get; set; }
        public bool DebuggingMode { get; set; }
        public int StackTextFreePosition { get; set; }
        public int StackStaticDataFreePosition { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiscVSimulator.Model
{
    public class RiscVProgramResult
    {
        public RiscVProgramResult()
        {
            Register = new Register[32];
            Memory = new Dictionary<int, int>();
        }
        public Register[] Register { get; set; }
        public int StackTextFreePosition { get; set; }
        public int StackStaticDataFreePosition { get; set; }
        public Dictionary<int, int> Memory { get; set; }
    }
}

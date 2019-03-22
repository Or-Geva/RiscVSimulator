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
            Register = new Register[33];
            Memory = new Dictionary<int, byte>();
            Register[0] = new Register {Name = "zero/x0", Value = 0};
            Register[1] = new Register {Name = "ra/x1", Value = 0};
            Register[2] = new Register {Name = "sp/x2", Value = 0};
            Register[3] = new Register {Name = "gp/x3", Value = 0};
            Register[4] = new Register {Name = "tp/x4", Value = 0};
            Register[5] = new Register {Name = "t0/x5", Value = 0};
            Register[6] = new Register {Name = "t1/x6", Value = 0};
            Register[7] = new Register {Name = "t2/x7", Value = 0};
            Register[8] = new Register {Name = "s0/x8", Value = 0};
            Register[9] = new Register {Name = "s1/x9", Value = 0};
            Register[10] = new Register {Name = "a0/x10", Value = 0};
            Register[11] = new Register {Name = "a1/x11", Value = 0};
            Register[12] = new Register {Name = "a2/x12", Value = 0};
            Register[13] = new Register {Name = "a3/x13", Value = 0};
            Register[14] = new Register {Name = "a4/x14", Value = 0};
            Register[15] = new Register {Name = "a5/x15", Value = 0};
            Register[16] = new Register {Name = "a6/x16", Value = 0};
            Register[17] = new Register {Name = "a7/x17", Value = 0};
            Register[18] = new Register {Name = "s2/x18", Value = 0};
            Register[19] = new Register {Name = "s3/x19", Value = 0};
            Register[20] = new Register {Name = "s4/x20", Value = 0};
            Register[21] = new Register {Name = "s5/x21", Value = 0};
            Register[22] = new Register {Name = "s6/x22", Value = 0};
            Register[23] = new Register {Name = "s7/x23", Value = 0};
            Register[24] = new Register {Name = "s8/x24", Value = 0};
            Register[25] = new Register {Name = "s9/x25", Value = 0};
            Register[26] = new Register {Name = "s10/x26", Value = 0};
            Register[27] = new Register {Name = "s11/x27", Value = 0};
            Register[28] = new Register {Name = "t3/x28", Value = 0};
            Register[29] = new Register {Name = "t4/x29", Value = 0};
            Register[30] = new Register {Name = "t5/x30", Value = 0};
            Register[31] = new Register {Name = "t6/x31", Value = 0};
            Register[32] = new Register {Name = "pc", Value = 0x10000 };
        }
        public Register[] Register { get; set; }
        public int StackTextFreePosition { get; set; }
        public int StackStaticDataFreePosition { get; set; }
        public Dictionary<int, byte> Memory { get; set; }
    }
}

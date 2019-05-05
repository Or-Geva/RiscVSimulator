using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiscVSimulator.Model
{
    public class RiscVProgramResult
    {
        public Register[] Register { get; set; }
        public uint StackTextFreePosition { get; set; }
        public uint StackStaticDataFreePosition { get; set; }
        public uint StackDynamicDataFreePosition { get; set; }
        public Dictionary<uint, byte> Memory { get; set; }
        public int DebugLine { get; set; }
        public List<GraphicBorder> GraphicBorder { get; set; }
        public AlphanumericData alphanumericData { get; set; }

        public RiscVProgramResult(RiscVProgram req)
        {
            alphanumericData = new AlphanumericData();
            if (req.Registers == null)
                req.Registers = new List<Register>();
            Register = new Register[33];
            Memory = new Dictionary<uint, byte>();
            GraphicBorder = new List<GraphicBorder>();
            Register[0] = new Register {Name = "zero/x0", Value = 0};
            Register[1] = new Register {Name = "ra/x1", Value = req.Registers[1].Value};
            Register[2] = new Register {Name = "sp/x2", Value = req.Registers[2].Value};
            Register[3] = new Register {Name = "gp/x3", Value = req.Registers[3].Value};
            Register[4] = new Register {Name = "tp/x4", Value = req.Registers[4].Value};
            Register[5] = new Register {Name = "t0/x5", Value = req.Registers[5].Value};
            Register[6] = new Register {Name = "t1/x6", Value = req.Registers[6].Value};
            Register[7] = new Register {Name = "t2/x7", Value = req.Registers[7].Value};
            Register[8] = new Register {Name = "s0/x8", Value = req.Registers[8].Value};
            Register[9] = new Register {Name = "s1/x9", Value = req.Registers[9].Value};
            Register[10] = new Register {Name = "a0/x10", Value = req.Registers[10].Value};
            Register[11] = new Register {Name = "a1/x11", Value = req.Registers[11].Value};
            Register[12] = new Register {Name = "a2/x12", Value = req.Registers[12].Value};
            Register[13] = new Register {Name = "a3/x13", Value = req.Registers[13].Value};
            Register[14] = new Register {Name = "a4/x14", Value = req.Registers[14].Value};
            Register[15] = new Register {Name = "a5/x15", Value = req.Registers[15].Value};
            Register[16] = new Register {Name = "a6/x16", Value = req.Registers[16].Value};
            Register[17] = new Register {Name = "a7/x17", Value = req.Registers[17].Value};
            Register[18] = new Register {Name = "s2/x18", Value = req.Registers[18].Value};
            Register[19] = new Register {Name = "s3/x19", Value = req.Registers[19].Value};
            Register[20] = new Register {Name = "s4/x20", Value = req.Registers[20].Value};
            Register[21] = new Register {Name = "s5/x21", Value = req.Registers[21].Value};
            Register[22] = new Register {Name = "s6/x22", Value = req.Registers[22].Value};
            Register[23] = new Register {Name = "s7/x23", Value = req.Registers[23].Value};
            Register[24] = new Register {Name = "s8/x24", Value = req.Registers[24].Value};
            Register[25] = new Register {Name = "s9/x25", Value = req.Registers[25].Value};
            Register[26] = new Register {Name = "s10/x26", Value = req.Registers[26].Value};
            Register[27] = new Register {Name = "s11/x27", Value = req.Registers[27].Value};
            Register[28] = new Register {Name = "t3/x28", Value = req.Registers[28].Value};
            Register[29] = new Register {Name = "t4/x29", Value = req.Registers[29].Value};
            Register[30] = new Register {Name = "t5/x30", Value = req.Registers[30].Value};
            Register[31] = new Register {Name = "t6/x31", Value = req.Registers[31].Value};
            Register[32] = new Register {Name = "pc", Value = 0x10000};
        }

        public RiscVProgramResult(RiscVProgramResult result, int debugline = 0)
        {
            Register = new Register[result.Register.Length];
            Memory = new Dictionary<uint, byte>(result.Memory);
            for (int i = 0; i < result.Register.Length; i++)
                Register[i] = new Register
                {
                    Name = result.Register[i].Name,
                    Value = result.Register[i].Value
                };
            this.alphanumericData = alphanumericData;
            DebugLine = debugline;
            GraphicBorder = result.GraphicBorder;
            alphanumericData = result.alphanumericData;

        }
   
        public RiscVProgramResult()
        {
            Register = new Register[33];
            Memory = new Dictionary<uint, byte>();
            GraphicBorder = new List<GraphicBorder>();
            alphanumericData = new AlphanumericData();
        }
    }


}

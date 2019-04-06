using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RiscVSimulator.Model;

namespace RiscVSimulator.Utils
{
    public static class Instructions
    {
        private static Dictionary<string, int> registersOpcode;
        private static Dictionary<string, int> RInstructionOpcode;

         static Instructions()
        {
            registersOpcode = new Dictionary<string, int>
            {
                {"x0",0},{"zero",0},{"x1",1},{"ra",1},{"sp",2},{"x2",2},{"gp",3},{"x3",3},{"tp",4},{"x4",4},{"t0",5},{"x5",5},{"t1",6},{"x6",6},{"t2",7},{"x7",7},{"s0",8},{"x8",8},{"s1",9},{"x9",9},{"a0",10},{"x10",10},{"a1",11},{"x11",11},{"a2",12},{"x12",12},{"a3",13},{"x13",13},{"a4",14},{"x14",14},{"a5",15},{"x15",15},{"a6",16},{"x16",16},{"a7",17},{"x17",17},{"s2",18},{"x18",18},{"s3",19},{"x19",19},{"s4",20},{"x20",20},{"s5",21},{"x21",21},{"s6",22},{"x22",22},{"s7",23},{"x23",23},{"s8",24},{"x24",24},{"s9",25},{"x25",25},{"s10",26},{"x26",26},{"s11",27},{"x27",27},{"t3",28},{"x28",28},{"t4",29},{"x29",29},{"t5",30},{"x30",30},{"t6",31},{"x31",31},{"pc",32}
            };
            RInstructionOpcode = new Dictionary<string, int>
            {
                {"add",51},{"sub",1073741875},{"sll",4147},{"slt",8243},{"sltu",12339},{"xor",16435},{"srl",20531},{"sra",1073762355},{"or",24627},{"and",28723}
            };
        }

        public static int RInstruction(string reqProgram, ref int cursor, Register[] resRegister,string commandName)
        {
            int instructionLayout = RInstructionOpcode[commandName];
            var args = Healper.GetThreeArgs(reqProgram, ref cursor);

            instructionLayout = instructionLayout | registersOpcode[args[0]] << 7 |
                                registersOpcode[args[1]] << 15 | registersOpcode[args[2]] << 20;
            resRegister[registersOpcode[args[0]]].Value = Healper.CalculatRComand(resRegister[registersOpcode[args[1]]].Value,resRegister[registersOpcode[args[2]]].Value,commandName);

            return instructionLayout;
        }

        public static int AddiInstruction(string reqProgram, ref int cursor, out string label, Register[] resRegister)
        {
            int instructionLayout = 19;
            var args = Healper.GetThreeArgs(reqProgram, ref cursor);
            instructionLayout = instructionLayout | registersOpcode[args[0]] << 7 |
                                registersOpcode[args[1]] << 15;
            if (int.TryParse(args[2], out int number))
            {
                instructionLayout = instructionLayout | number << 20;
                resRegister[registersOpcode[args[0]]].Value = resRegister[registersOpcode[args[1]]].Value + number;
                label = null;
            }
            else
                label = args[2];
            return instructionLayout;
        }

    }
}

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

         static Instructions()
        {
            registersOpcode = new Dictionary<string, int>
            {
                {"x0",0},{"zero",0},{"x1",1},{"ra",1},
            };
        }
        public static int AddInstruction(string reqProgram, ref int cursor, Register[] resRegister)
        {
            int instructionLayout = 51;
            var args = Healper.GetThreeArgs(reqProgram, ref cursor);

            instructionLayout = instructionLayout | registersOpcode[args[0]] << 7 |
                                registersOpcode[args[1]] << 15 | registersOpcode[args[2]] << 20;
            resRegister[registersOpcode[args[0]]].Value = resRegister[registersOpcode[args[1]]].Value + resRegister[registersOpcode[args[2]]].Value;
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

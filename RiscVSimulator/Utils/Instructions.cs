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
        private static Dictionary<string, int> IInstructionOpcode;
        private static Dictionary<string, int> SInstructionOpcode;
        private static Dictionary<string, int> BInstructionOpcode;
        private static Dictionary<string, int> UInstructionOpcode;


        static Instructions()
        {
            registersOpcode = new Dictionary<string, int>
            {
                {"x0", 0}, {"zero", 0}, {"x1", 1}, {"ra", 1}, {"sp", 2}, {"x2", 2}, {"gp", 3}, {"x3", 3}, {"tp", 4},
                {"x4", 4}, {"t0", 5}, {"x5", 5}, {"t1", 6}, {"x6", 6}, {"t2", 7}, {"x7", 7}, {"s0", 8}, {"x8", 8},
                {"s1", 9}, {"x9", 9}, {"a0", 10}, {"x10", 10}, {"a1", 11}, {"x11", 11}, {"a2", 12}, {"x12", 12},
                {"a3", 13}, {"x13", 13}, {"a4", 14}, {"x14", 14}, {"a5", 15}, {"x15", 15}, {"a6", 16}, {"x16", 16},
                {"a7", 17}, {"x17", 17}, {"s2", 18}, {"x18", 18}, {"s3", 19}, {"x19", 19}, {"s4", 20}, {"x20", 20},
                {"s5", 21}, {"x21", 21}, {"s6", 22}, {"x22", 22}, {"s7", 23}, {"x23", 23}, {"s8", 24}, {"x24", 24},
                {"s9", 25}, {"x25", 25}, {"s10", 26}, {"x26", 26}, {"s11", 27}, {"x27", 27}, {"t3", 28}, {"x28", 28},
                {"t4", 29}, {"x29", 29}, {"t5", 30}, {"x30", 30}, {"t6", 31}, {"x31", 31}, {"pc", 32}
            };
            RInstructionOpcode = new Dictionary<string, int>
            {
                {"add", 51}, {"sub", 1073741875}, {"sll", 4147}, {"slt", 8243}, {"sltu", 12339}, {"xor", 16435},
                {"srl", 20531}, {"sra", 1073762355}, {"or", 24627}, {"and", 28723}
            };
            IInstructionOpcode = new Dictionary<string, int>
            {
                {"addi", 19}, {"slti", 8211}, {"sltiu", 30023}, {"xori", 40023}, {"ori", 60023}, {"andi", 70023},
                {"slli", 4115}, {"srli", 20499}, {"srai", 1073762323},
                {"lb", 3}, {"lh", 4099}, {"lw", 8195}, {"lbu", 16387}, {"lhu", 20483}, {"jalr", 103}
            };
            SInstructionOpcode = new Dictionary<string, int>
            {
                {"sb", 3}, {"sh", 8259}, {"sw", 16451}
            };
            BInstructionOpcode = new Dictionary<string, int>
            {
                {"beq", Convert.ToInt32("1100011", 2)}, {"bne", Convert.ToInt32("1000001100011", 2)},
                {"blt", Convert.ToInt32("100000001100011", 2)},
                {"bge", Convert.ToInt32("101000001100011", 2)}, {"bltu", Convert.ToInt32("110000001100011", 2)},
                {"bgeu", Convert.ToInt32("111000001100011", 2)},
            };
            SInstructionOpcode = new Dictionary<string, int>
            {
                {"lui", 55}
            };
        }

        public static int RInstruction(string reqProgram, ref int cursor, Register[] resRegister, string commandName)
        {
            int instructionLayout = RInstructionOpcode[commandName];
            var args = Healper.GetArgs(reqProgram, ref cursor);

            instructionLayout = instructionLayout | registersOpcode[args[0]] << 7 |
                                registersOpcode[args[1]] << 15 | registersOpcode[args[2]] << 20;
            resRegister[registersOpcode[args[0]]].Value = Healper.CalculatRComand(
                resRegister[registersOpcode[args[1]]].Value, resRegister[registersOpcode[args[2]]].Value, commandName);

            return instructionLayout;
        }

        public static int BInstruction(string reqProgram, ref int cursor, Register[] resRegister,
            string commandName, Dictionary<int, byte> resMemory)
        {
            int instructionLayout = BInstructionOpcode[commandName];
            var args = Healper.GetArgs(reqProgram, ref cursor);
            instructionLayout = instructionLayout | registersOpcode[args[0]] << 15 | registersOpcode[args[1]] << 20;
            if (int.TryParse(args[2], out int number))
            {
                //TODO complite parse the command
                switch (commandName)
                {
                    case "beq":
                        if (resRegister[registersOpcode[args[0]]].Value == resRegister[registersOpcode[args[1]]].Value)
                            resRegister[registersOpcode["pc"]].Value += number;
                        break;
                    case "bne":
                        if (resRegister[registersOpcode[args[0]]].Value != resRegister[registersOpcode[args[1]]].Value)
                            resRegister[registersOpcode["pc"]].Value += number;
                        break;
                    case "blt":
                        if (resRegister[registersOpcode[args[0]]].Value <= resRegister[registersOpcode[args[1]]].Value)
                            resRegister[registersOpcode["pc"]].Value += number;
                        break;
                    case "bge":
                        if (resRegister[registersOpcode[args[0]]].Value > resRegister[registersOpcode[args[1]]].Value)
                            resRegister[registersOpcode["pc"]].Value += number;
                        break;
                    case "bltu":
                        if (Convert.ToUInt32(resRegister[registersOpcode[args[0]]].Value) <
                            Convert.ToUInt32(resRegister[registersOpcode[args[1]]].Value))
                            resRegister[registersOpcode["pc"]].Value += number;
                        break;
                    case "bgeu":
                        if (Convert.ToUInt32(resRegister[registersOpcode[args[0]]].Value) >=
                            Convert.ToUInt32(resRegister[registersOpcode[args[1]]].Value))
                            resRegister[registersOpcode["pc"]].Value += number;
                        break;
                }
            }
            else
            {
                throw new SimulatorException {ErrorMessage = $"'unknown offset in B"};
            }

            return instructionLayout;
        }

        public static int IInstruction(string reqProgram, ref int cursor, out string label, Register[] resRegister,
            string commandName, Dictionary<int, byte> resMemory)
        {
            List<string> args = new List<string>();
            int instructionLayout = IInstructionOpcode[commandName];
            if (commandName == "jalr")
            {
                label = null;
                args = Healper.ParseJalr(reqProgram, ref cursor);
                if (!Healper.IsComandWithOffset(args[1], out string register, out int offset))
                    throw new SimulatorException {ErrorMessage = $"'unknown load format command"};
                if (!registersOpcode.ContainsKey(register))
                    throw new SimulatorException {ErrorMessage = $"'unknown '{register}' register for load command"};
                instructionLayout |= registersOpcode[args[0]] << 7 | registersOpcode[register] << 15 | offset << 20;
                resRegister[registersOpcode[args[0]]].Value = resRegister[registersOpcode["pc"]].Value;
                resRegister[registersOpcode["pc"]].Value =
                    resRegister[registersOpcode[args[1]]].Value + Convert.ToInt32(args[1]);
                return instructionLayout;
            }
            else
            {
                args = Healper.GetArgs(reqProgram, ref cursor);
                instructionLayout = instructionLayout | registersOpcode[args[0]] << 7 |
                                    registersOpcode[args[1]] << 15;
            }

            if (int.TryParse(args[2], out int number))
            {
                if (commandName.ToLower() == "srai" || commandName.ToLower() == "slli" ||
                    commandName.ToLower() == "srli")
                {
                    instructionLayout = instructionLayout | ((number << 25) >> 5);
                    resRegister[registersOpcode[args[0]]].Value =
                        Healper.CalculatIComand(resRegister[registersOpcode[args[1]]].Value, number & 31, commandName);
                }
                else
                {
                    instructionLayout = instructionLayout | number << 20;
                    resRegister[registersOpcode[args[0]]].Value =
                        Healper.CalculatIComand(resRegister[registersOpcode[args[1]]].Value, number & 4095,
                            commandName);
                }

                label = null;
            }
            else
            {
                label = args[2];
            }

            return instructionLayout;
        }

        public static int LoadIInstruction(string reqProgram, ref int cursor, Register[] resRegister,
            string commandName, Dictionary<int, byte> resMemory)
        {
            int instructionLayout = IInstructionOpcode[commandName];
            var args = Healper.GetArgs(reqProgram, ref cursor, 2);

            if (!Healper.IsComandWithOffset(args[1], out string register, out int offset))
                throw new SimulatorException {ErrorMessage = $"'unknown load format command"};
            if (!registersOpcode.ContainsKey(register))
                throw new SimulatorException {ErrorMessage = $"'unknown '{register}' register for load command"};

            instructionLayout |= registersOpcode[args[0]] << 7 | registersOpcode[register] << 15;
            instructionLayout |= offset << 20;
            resRegister[registersOpcode[args[0]]].Value =
                Healper.CalculatLoadComand(resRegister[registersOpcode[register]].Value, offset, commandName,
                    resMemory);
            return instructionLayout;
        }

        public static int StoreInstruction(string reqProgram, ref int cursor, Register[] resRegister,
            string commandName, Dictionary<int, byte> resMemory)
        {
            int instructionLayout = SInstructionOpcode[commandName];

            var args = Healper.GetArgs(reqProgram, ref cursor, 2);

            if (!Healper.IsComandWithOffset(args[1], out string register, out int offset))
                throw new SimulatorException {ErrorMessage = $"'unknown store format command"};
            if (!registersOpcode.ContainsKey(register))
                throw new SimulatorException {ErrorMessage = $"'unknown '{register}' register for load command"};

            instructionLayout |= registersOpcode[args[0]] << 20 | registersOpcode[register] << 15;
            instructionLayout |= (offset & Convert.ToInt32("111111100000", 2) << 20) |
                                 (offset & Convert.ToInt32("11111", 2) << 7);
            var registerValue = resRegister[registersOpcode[args[0]]].Value;
            switch (commandName)
            {
                case "sb":
                    resMemory[resRegister[registersOpcode[register]].Value + offset] =
                        (byte) (registerValue & Convert.ToInt32("11111111", 2));
                    break;
                case "sh":
                    resMemory[resRegister[registersOpcode[register]].Value + offset] =
                        (byte) (registerValue & Convert.ToInt32("11111111", 2));
                    resMemory[resRegister[registersOpcode[register]].Value + offset + 1] =
                        (byte) ((registerValue >> 8) & Convert.ToInt32("11111111", 2));
                    break;
                default:
                    resMemory[resRegister[registersOpcode[register]].Value + offset] =
                        (byte) (registerValue & Convert.ToInt32("11111111", 2));
                    resMemory[resRegister[registersOpcode[register]].Value + offset + 1] =
                        (byte) ((registerValue >> 8) & Convert.ToInt32("11111111", 2));
                    resMemory[resRegister[registersOpcode[register]].Value + offset + 2] =
                        (byte) ((registerValue >> 16) & Convert.ToInt32("11111111", 2));
                    resMemory[resRegister[registersOpcode[register]].Value + offset + 3] = (byte) (registerValue >> 24);
                    break;
            }

            return instructionLayout;
        }

        public static int UInstruction(string reqProgram, ref int cursor, out string label, Register[] resRegister,
            string commandName, Dictionary<int, byte> resMemory)
        {
            int instructionLayout = UInstructionOpcode[commandName];
            label = null;
            var args = Healper.GetArgs(reqProgram, ref cursor, 2);
            if (!registersOpcode.ContainsKey(args[0]))
                throw new SimulatorException {ErrorMessage = $"'unknown '{args[0]}' register for lui command"};
            if (int.TryParse(args[1], out int number))
            {
                instructionLayout |= (number << 12) | registersOpcode[args[0]] << 7;
                resRegister[registersOpcode[args[0]]].Value = (number << 12);
            }
            else
            {
                label = args[1];
            }

            return instructionLayout;
        }
    }
}

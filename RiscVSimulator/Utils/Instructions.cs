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
            UInstructionOpcode = new Dictionary<string, int>
            {
                {"lui", 55},{"auipc", 23}
            };
        }

        public static int RInstruction(string[][] programTextArray, ref int i, ref int j,
            Dictionary<int, ExeCommand> commandsToExe)
        {
            var commandName = programTextArray[i][j];
            if (programTextArray[i].Length > ++j == false)
            {
                if (programTextArray.Length > ++i == false)
                    throw new SimulatorException { ErrorMessage = $"'incomplete command" };
                j = 0;
            }

            int instructionLayout = RInstructionOpcode[commandName];
            var args = Healper.GetArgs(programTextArray, ref i,ref j);
            commandsToExe[commandsToExe.Last().Key] = new ExeCommand{Args = args,Instraction = commandName};
            if (!registersOpcode.ContainsKey(args[0]) || !registersOpcode.ContainsKey(args[1]) || !registersOpcode.ContainsKey(args[2]))
                throw new SimulatorException { ErrorMessage = $"'unknown register" };

            instructionLayout = instructionLayout | registersOpcode[args[0]] << 7 |
                                registersOpcode[args[1]] << 15 | registersOpcode[args[2]] << 20;

            return instructionLayout;
        }

        public static void ExeRInstruction(RiscVProgramResult res, string exeCommand, List<string> args)
        {
            res.Register[registersOpcode[args[0]]].Value = Healper.CalculatRComand(
                res.Register[registersOpcode[args[1]]].Value, res.Register[registersOpcode[args[2]]].Value, exeCommand);
        }

        public static int BInstruction(string[][] programTextArray, ref int i, ref int j,
            Dictionary<int, ExeCommand> commandsToExe)
        {
            var commandName = programTextArray[i][j];
            if (programTextArray[i].Length > ++j == false)
            {
                if (programTextArray.Length > ++i == false)
                    throw new SimulatorException { ErrorMessage = $"'incomplete command" };
                j = 0;
            }

            int instructionLayout = BInstructionOpcode[commandName];
            var args = Healper.GetArgs(programTextArray, ref i, ref j);
            commandsToExe[commandsToExe.Last().Key] = new ExeCommand { Args = args, Instraction = commandName };

            if (!registersOpcode.ContainsKey(args[0]) || !registersOpcode.ContainsKey(args[1]))
                throw new SimulatorException { ErrorMessage = $"'unknown register" };
            instructionLayout |= registersOpcode[args[0]] << 15 | registersOpcode[args[1]] << 20;
            if (int.TryParse(args[2], out int number))
                instructionLayout = instructionLayout | ((number << 25) >> 5);
            else
                throw new SimulatorException { ErrorMessage = $"'unknown shamit" };

            var bits1to4 = number & Convert.ToInt32("1111", 2);
            var bits5to11 = (number & Convert.ToInt32("1111110000", 2) >> 4);
            var bit11 = (number & Convert.ToInt32("10000000000", 2) >>10);
            var bit12 = (number & Convert.ToInt32("100000000000", 2) >>11);
            instructionLayout |= (bits1to4 << 8) | (bit11 << 7) | (bits5to11 << 25) | (bit12 << 30);
            return instructionLayout;
        }

        public static void ExeBInstruction(RiscVProgramResult res, string exeCommand, List<string> args)
        {
            if (int.TryParse(args[2], out int number))
            {
                switch (exeCommand)
                {
                    case "beq":
                        if (res.Register[registersOpcode[args[0]]].Value ==
                            res.Register[registersOpcode[args[1]]].Value)
                            res.Register[registersOpcode["pc"]].Value += number;
                        break;
                    case "bne":
                        if (res.Register[registersOpcode[args[0]]].Value !=
                            res.Register[registersOpcode[args[1]]].Value)
                            res.Register[registersOpcode["pc"]].Value += number;
                        break;
                    case "blt":
                        if (res.Register[registersOpcode[args[0]]].Value <=
                            res.Register[registersOpcode[args[1]]].Value)
                            res.Register[registersOpcode["pc"]].Value += number;
                        break;
                    case "bge":
                        if (res.Register[registersOpcode[args[0]]].Value > res.Register[registersOpcode[args[1]]].Value)
                            res.Register[registersOpcode["pc"]].Value += number;
                        break;
                    case "bltu":
                        if (Convert.ToUInt32(res.Register[registersOpcode[args[0]]].Value) <
                            Convert.ToUInt32(res.Register[registersOpcode[args[1]]].Value))
                            res.Register[registersOpcode["pc"]].Value += number;
                        break;
                    case "bgeu":
                        if (Convert.ToUInt32(res.Register[registersOpcode[args[0]]].Value) >=
                            Convert.ToUInt32(res.Register[registersOpcode[args[1]]].Value))
                            res.Register[registersOpcode["pc"]].Value += number;
                        break;
                }
            }
            else
            {
                throw new SimulatorException {ErrorMessage = $"'unknown offset in B"};
            }
        }

        public static int ShamtIInstruction(string[][] programTextArray, ref int i, ref int j,
            Dictionary<int, ExeCommand> commandsToExe)
        {
            var commandName = programTextArray[i][j];
            int instructionLayout = IInstructionOpcode[commandName];

            if (programTextArray[i].Length > ++j == false)
            {
                if (programTextArray.Length > ++i == false)
                    throw new SimulatorException { ErrorMessage = $"'incomplete command" };
                j = 0;
            }

            var args = Healper.GetArgs(programTextArray, ref i, ref j);

            if (!registersOpcode.ContainsKey(args[0]) || !registersOpcode.ContainsKey(args[1]))
                throw new SimulatorException { ErrorMessage = $"'unknown register" };
            instructionLayout |= registersOpcode[args[0]] << 7 | registersOpcode[args[1]] << 15;
            commandsToExe[commandsToExe.Last().Key] = new ExeCommand { Args = args, Instraction = commandName };

            if (int.TryParse(args[2], out int number))
                instructionLayout = instructionLayout | ((number << 25) >> 5);
            else
                throw new SimulatorException {ErrorMessage = $"'unknown shamit"};
            

            return instructionLayout;
        }

        public static void ExeShamtIInstruction(RiscVProgramResult res, string exeCommand, List<string> args)
        {
        res.Register[registersOpcode[args[0]]].Value =
         Healper.CalculatIComand(res.Register[registersOpcode[args[1]]].Value, int.Parse(args[2]) & 31,
             exeCommand);
        }

        public static int IInstruction(string[][] programTextArray, ref int i, ref int j, out string label,
            Dictionary<int, ExeCommand> commandsToExe)
        {
            var commandName = programTextArray[i][j];
            if (programTextArray[i].Length > ++j == false)
            {
                if (programTextArray.Length > ++i == false)
                    throw new SimulatorException { ErrorMessage = $"'incomplete command" };
                j = 0;
            }

            int instructionLayout = IInstructionOpcode[commandName];

            if (commandName == "jalr")
            {
                label = null;
                var args = Healper.ParseJalr(programTextArray, ref i, ref j);
                commandsToExe[commandsToExe.Last().Key] = new ExeCommand { Args = args, Instraction = commandName };

                if (!Healper.IsComandWithOffset(args[1], out string register, out int offset))
                    throw new SimulatorException {ErrorMessage = $"'unknown load format command"};
                if (!registersOpcode.ContainsKey(register))
                    throw new SimulatorException {ErrorMessage = $"'unknown '{register}' register for load command"};
                instructionLayout |= registersOpcode[args[0]] << 7 | registersOpcode[register] << 15 | offset << 20;
 
                return instructionLayout;
            }
            else
            {
                var args = Healper.GetArgs(programTextArray, ref i, ref j);
                if (!registersOpcode.ContainsKey(args[0]) || !registersOpcode.ContainsKey(args[1]))
                    throw new SimulatorException {ErrorMessage = $"'unknown register"};
                instructionLayout |= registersOpcode[args[0]] << 7 | registersOpcode[args[1]] << 15;
                commandsToExe[commandsToExe.Last().Key] = new ExeCommand { Args = args, Instraction = commandName };

                if (int.TryParse(args[2], out int number))
                {
                        instructionLayout = instructionLayout | number << 20;
                    label = null;
                }
                else
                {
                    label = args[2];
                }

                return instructionLayout;
            }
        }

        public static void ExeIInstruction(RiscVProgramResult res, string exeCommand, List<string> args)
        {
            if (exeCommand == "jalr")
            {
               res.Register[registersOpcode[args[0]]].Value = res.Register[registersOpcode["pc"]].Value;
                res.Register[registersOpcode["pc"]].Value = res.Register[registersOpcode[args[1]]].Value + Convert.ToInt32(args[1]); 
            }
            else
            {
                res.Register[registersOpcode[args[0]]].Value =
                    Healper.CalculatIComand(res.Register[registersOpcode[args[1]]].Value, int.Parse(args[2]) & 4095,
                        exeCommand);
            }
        }

        public static int LoadIInstruction(string[][] programTextArray, ref int i, ref int j,
            Dictionary<int, ExeCommand> commandsToExe)
        {
            var commandName = programTextArray[i][j];
            if (programTextArray[i].Length > ++j == false)
            {
                if (programTextArray.Length > ++i == false)
                    throw new SimulatorException { ErrorMessage = $"'incomplete command" };
                j = 0;
            }
            int instructionLayout = IInstructionOpcode[commandName];
            var args = Healper.GetArgs(programTextArray, ref i, ref j, 2);
           

            if (!Healper.IsComandWithOffset(args[1], out string register, out int offset))
                throw new SimulatorException {ErrorMessage = $"'unknown load format command"};
            if (!registersOpcode.ContainsKey(register))
                throw new SimulatorException {ErrorMessage = $"'unknown '{register}' register for load command"};

            args.AddRange(new List<string> { register, offset.ToString() });
            commandsToExe[commandsToExe.Last().Key] = new ExeCommand { Args = args, Instraction = commandName };
            instructionLayout |= registersOpcode[args[0]] << 7 | registersOpcode[register] << 15 | offset << 20;
            return instructionLayout;
        }

        public static void ExeLoadInstruction(RiscVProgramResult res, string exeCommand, List<string> args)
        {
            res.Register[registersOpcode[args[0]]].Value =
                Healper.CalculatLoadComand(res.Register[registersOpcode[args[1]]].Value, Int32.Parse(args[2]),
                    exeCommand,
                    res.Memory);
        }

        public static int StoreInstruction(string[][] programTextArray, ref int i, ref int j,
            Dictionary<int, ExeCommand> commandsToExe)
        {
            var commandName = programTextArray[i][j];
            if (programTextArray[i].Length > ++j == false)
            {
                if (programTextArray.Length > ++i == false)
                    throw new SimulatorException { ErrorMessage = $"'incomplete command" };
                j = 0;
            }
            int instructionLayout = SInstructionOpcode[commandName];
            var args = Healper.GetArgs(programTextArray, ref i, ref j, 2);
            commandsToExe[commandsToExe.Last().Key] = new ExeCommand { Args = args, Instraction = commandName };

            if (!Healper.IsComandWithOffset(args[1], out string register, out int offset))
                throw new SimulatorException {ErrorMessage = $"'unknown store format command"};
            if (!registersOpcode.ContainsKey(register))
                throw new SimulatorException {ErrorMessage = $"'unknown '{register}' register for load command"};

            instructionLayout |= registersOpcode[args[0]] << 20 | registersOpcode[register] << 15;
            instructionLayout |= (offset & Convert.ToInt32("111111100000", 2) << 20) |
                                 (offset & Convert.ToInt32("11111", 2) << 7);
  

            return instructionLayout;
        }

        public static void ExeStoreInstruction(RiscVProgramResult res, string exeCommand, List<string> args)
        {
            res.Register[registersOpcode[args[0]]].Value =
                Healper.CalculatLoadComand(res.Register[registersOpcode[args[1]]].Value, Int32.Parse(args[2]),
                    exeCommand,
                    res.Memory);
             var registerValue = res.Register[registersOpcode[args[0]]].Value;
             switch (exeCommand)
             {
                 case "sb":
                     res.Memory[res.Register[registersOpcode[args[1]]].Value + Int32.Parse(args[2])] =
                         (byte) (registerValue & Convert.ToInt32("11111111", 2));
                     break;
                 case "sh":
                     res.Memory[res.Register[registersOpcode[args[1]]].Value + Int32.Parse(args[2])] =
                         (byte) (registerValue & Convert.ToInt32("11111111", 2));
                     res.Memory[res.Register[registersOpcode[args[1]]].Value + Int32.Parse(args[2]) + 1] =
                         (byte) ((registerValue >> 8) & Convert.ToInt32("11111111", 2));
                     break;
                 default:
                     res.Memory[res.Register[registersOpcode[args[1]]].Value + Int32.Parse(args[2])] =
                         (byte) (registerValue & Convert.ToInt32("11111111", 2));
                     res.Memory[res.Register[registersOpcode[args[1]]].Value + Int32.Parse(args[2]) + 1] =
                         (byte) ((registerValue >> 8) & Convert.ToInt32("11111111", 2));
                     res.Memory[res.Register[registersOpcode[args[1]]].Value + Int32.Parse(args[2]) + 2] =
                         (byte) ((registerValue >> 16) & Convert.ToInt32("11111111", 2));
                     res.Memory[res.Register[registersOpcode[args[1]]].Value + Int32.Parse(args[2]) + 3] =
                         (byte) (registerValue >> 24);
                     break;
             }
        }

        public static int JInstruction(string[][] programTextArray, ref int i, ref int j,
            Dictionary<int, ExeCommand> commandsToExe)
        {
            var commandName = programTextArray[i][j];
            if (programTextArray[i].Length > ++j == false)
            {
                if (programTextArray.Length > ++i == false)
                    throw new SimulatorException { ErrorMessage = $"'incomplete command" };
                j = 0;
            }
            int instructionLayout = UInstructionOpcode[commandName];
            var args = Healper.GetArgs(programTextArray, ref i, ref j, commandName == "jal" ? 2 : 1);
            commandsToExe[commandsToExe.Last().Key] = new ExeCommand { Args = args, Instraction = commandName };
            if (int.TryParse(args[commandName == "jal" ? 1 : 0], out int number))
            {
                instructionLayout |=  (commandName == "jal" ? registersOpcode[args[0]] << 7 : 0);
                commandsToExe[commandsToExe.Last().Key].Args[commandName == "jal" ? 1 : 0] = number.ToString();
            }
            else
            {
                throw new SimulatorException { ErrorMessage = $"'bad offset for j command" };
            }
            var bits1to10 = number & Convert.ToInt32("1111111111", 2);
            var bits12to19 = (number & Convert.ToInt32("1111111100000000000", 2) >> 11);
            var bit11 = (number & Convert.ToInt32("10000000000", 2) >> 10);
            var bit20 = (number & Convert.ToInt32("00000000000000000001", 2) >> 18);
            instructionLayout |= (bits12to19 << 12) | (bit11 << 20) | (bits1to10 << 21) | (bit20 << 31);
            return instructionLayout;
        }

        public static void ExeJInstruction(RiscVProgramResult res, string exeCommand, List<string> args)
        {
            switch (exeCommand)
            {
                case "jal":
                    res.Register[registersOpcode[args[0]]].Value = res.Register[registersOpcode["pc"]].Value + 4;
                    res.Register[registersOpcode["pc"]].Value += Int32.Parse(args[1]) & Convert.ToInt32("11111111111111111111", 2);
                    break;
                case "j":
                    res.Register[registersOpcode["pc"]].Value += Int32.Parse(args[1]) & Convert.ToInt32("11111111111111111111", 2);
                    break;
            }
        }

        public static int UInstruction(string[][] programTextArray, ref int i, ref int j, out string label,
            Dictionary<int, ExeCommand> commandsToExe)
        {
            var commandName = programTextArray[i][j];
            if (programTextArray[i].Length > ++j == false)
            {
                if (programTextArray.Length > ++i == false)
                    throw new SimulatorException { ErrorMessage = $"'incomplete command" };
                j = 0;
            }
            int instructionLayout = UInstructionOpcode[commandName];
            var args = Healper.GetArgs(programTextArray, ref i, ref j, 2);
            commandsToExe[commandsToExe.Last().Key] = new ExeCommand { Args = args, Instraction = commandName };

            label = null;
            if (!registersOpcode.ContainsKey(args[0]))
                throw new SimulatorException {ErrorMessage = $"'unknown '{args[0]}' register for U type command"};
            if (int.TryParse(args[1], out int number))
            {
                instructionLayout |= number << 12 | registersOpcode[args[0]] << 7;
                commandsToExe[commandsToExe.Last().Key].Args[1] = number.ToString();
            }
            else
            {
                label = args[1];
            }

            return instructionLayout;
        }

        public static void ExeUInstruction(RiscVProgramResult res, string exeCommand, List<string> args)
        {
              switch (exeCommand)
             {
                 case "auipc":
                     res.Register[registersOpcode[args[0]]].Value = res.Register[registersOpcode["pc"]].Value + (Int32.Parse(args[1]) << 12);
                     break;
                 case "lui":
                     res.Register[registersOpcode[args[0]]].Value = Int32.Parse(args[1]) << 12;
                     break;
             }
        }
    }
}

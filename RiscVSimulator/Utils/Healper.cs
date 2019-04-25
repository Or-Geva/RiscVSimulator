using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RiscVSimulator.Controllers;
using RiscVSimulator.Model;

namespace RiscVSimulator.Utils
{
    public static class Healper
    {
        public static List<string> GetArgs(string[][] programTextArray, ref int i, ref int j, int numberToGet = 3)
        {
            var result = new List<string>();
            if(programTextArray[i][j][0] == ',')
                throw new SimulatorException { ErrorMessage = "Missing first argument" };
            bool continiueLoop = true;
            while (result.Count != numberToGet && continiueLoop)
            {
                if (programTextArray[i][j].Contains(','))
                {
                    result.AddRange(programTextArray[i][j].Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).ToList());
                }
                else
                {
                    result.Add(programTextArray[i][j]);
                }
                if (programTextArray[i].Length > ++j == false)
                {
                    if (programTextArray.Length > ++i == false)
                        continiueLoop = false;
                    else
                        j = 0;
                }
            }

            if(result.Count != numberToGet)
                throw new SimulatorException { ErrorMessage = "Missing argument" };
            return result;
        }

        public static string GetDirective(string[][] programTextArray, ref int i, ref int j)
        {
            var result = programTextArray[i][j].Substring(1, programTextArray[i][j].Length-1);
            return result;
        }

        public static List<long> GetListOfNumber(string[][] programTextArray, ref int i, ref int j)
        {
            return programTextArray[i][j].Split(',').Select(x => long.Parse(x)).ToList();
        }

        public static bool IsLabel(string[][] programTextArray, ref int i, ref int j, out string label)
        {
            var temp = programTextArray[i][j].Length - 1;
            if (programTextArray[i][j][temp] == ':')
            {
                label = programTextArray[i][j].Substring(0, programTextArray[i][j].Length - 1);
                if (string.IsNullOrWhiteSpace(label))
                    throw new SimulatorException {ErrorMessage = $"Label cannot be empty"};
                if (programTextArray[i].Length > ++j == false)
                {
                    i++;
                    j = 0;
                }
                return true;
            }

            label = null;
            return false;

        }

        public static bool IsLowLabelCommand(string value, out string label)
        {
            label = "";
            try
            {
                if (value.Substring(0, 4) == "%lo(" && value[value.Length - 1] == ')')
                {
                    label = value.Substring(4, value.Length - 5);
                    return true;
                }

                if (value.Substring(0, 4) == "%hi(" && value[value.Length - 1] == ')')
                {
                    label = value.Substring(4, value.Length - 5);
                    return false;
                }

                throw new Exception();
            }
            catch (Exception e)
            {
                throw new SimulatorException {ErrorMessage = $"'{value}' is not recognize argument"};
            }
        }

        public static int GetAddress(RiscVProgramResult res, MemorySection memorySection)
        {
            switch (memorySection)
            {
                case MemorySection.Text:
                    return (int) memorySection + res.StackTextFreePosition;
                case MemorySection.Static:
                    return (int) memorySection + res.StackStaticDataFreePosition;
                default:
                    throw new SimulatorException
                        {ErrorMessage = $"'{memorySection}' cannot find fit section to return from f. GetAddress"};
            }
        }

        public static int CalculatRComand(int num1, int num2, string commandName)
        {
            switch (commandName)
            {
                case "add":
                    return num1 + num2;
                case "sub":
                    return num1 - num2;
                case "sll":
                    return num1 << num2;
                case "slt":
                    return num1 < num2 ? 1 : 0;
                case "sltu":
                    return Convert.ToUInt32(num1) < Convert.ToUInt32(num2) ? 1 : 0;
                case "xor":
                    return num1 ^ num2;
                case "srl":
                    return (int) (Convert.ToUInt32(num1) >> num2);
                case "sra":
                    return num1 >> num2; //??
                case "or":
                    return num1 | num2;
                case "and":
                    return num1 & num2;
                default:
                    throw new SimulatorException {ErrorMessage = $"'unknown '{commandName}' command"};
            }
        }

        public static int CalculatIComand(int num1, int num2, string commandName)
        {
            switch (commandName)
            {
                case "addi":
                    return num1 + num2;
                case "slli":
                    return num1 << num2;
                case "slti":
                    return num1 < num2 ? 1 : 0;
                case "sltiu":
                    return Convert.ToUInt32(num1) < Convert.ToUInt32(num2) ? 1 : 0;
                case "xori":
                    return num1 ^ num2;
                case "srli":
                    return (int) (Convert.ToUInt32(num1) >> num2);
                case "srai":
                    return num1 >> num2; //??
                case "ori":
                    return num1 | num2;
                case "andi":
                    return num1 & num2;
                default:
                    throw new SimulatorException {ErrorMessage = $"'unknown '{commandName}' command"};
            }
        }

        public static bool IsComandWithOffset(string s, out string register, out int offset)
        {
            var index1 = s.IndexOf('(');
            var index2 = s.IndexOf(')');
            offset = 0;
            register = string.Empty;
            if (index2 != -1 && index1 != -1)
            {
                if (!int.TryParse(s.Substring(0, index1), out int result))
                    return false;
                offset = result;
                register = s.Substring(index1 + 1, index2 - index1-1);
                return true;
            }

            return false;
        }

        public static int CalculatLoadComand(int value, int offset, string commandName, Dictionary<int, byte> resMemory)
        {
            switch (commandName)
            {
                case "lb":
                    return resMemory[value + offset] > 127
                        ? -256 | resMemory[value + offset]
                        : resMemory[value + offset];
                case "lbu":
                    return resMemory[value + offset];
                case "lh":
                    return (resMemory[value + offset] + (resMemory[value + offset + 1] << 8)) > 65536
                        ? -65536 | (resMemory[value + offset] + (resMemory[value + offset + 1] << 8))
                        : (resMemory[value + offset] + (resMemory[value + offset + 1] << 8));
                case "lhu":
                    return (resMemory[value + offset] + (resMemory[value + offset + 1] << 8));
                case "lw":
                    return (resMemory[value + offset] + (resMemory[value + offset + 1] << 8) +
                            (resMemory[value + offset + 2] << 16) + (resMemory[value + offset + 3] << 24));
                default:
                    throw new SimulatorException {ErrorMessage = $"'unknown '{commandName}' command"};
            }
        }

        public static List<string> ParseJalr(string[][] programTextArray, ref int i, ref int j)
        {
            var result = new List<string>();
            if (programTextArray[i][j][0] == ',')
                throw new SimulatorException { ErrorMessage = "Missing first argument" };
            if (programTextArray[i][j].Contains(','))
                {
                    result.AddRange(programTextArray[i][j++].Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).ToList());
                }
                else
                {
                    if (programTextArray[i][j].Contains('('))
                    {
                        result.Add(programTextArray[i][j++]);
                        result.Add("x1");
                    }
                    else
                    {
                    result.Add(programTextArray[i][j]);
                    i++;
                    j = 0;
                    if(programTextArray[i][j].Length > 1)
                        result.AddRange(programTextArray[i][j++].Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).ToList());
                    else
                        result.AddRange(programTextArray[++i][j++].Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).ToList());
                }
            }
            return result;
        }

        public static string PrepareString(string[][] programTextArray, ref int i, ref int j)
        {
            if(programTextArray[i][j][0] != '\"' )
                throw new SimulatorException { ErrorMessage = $"the string: '{programTextArray[i][j]}' missing a quote" };
            var result = string.Empty;
            result += programTextArray[i][j].Substring(1, programTextArray[i][j].Length - 1);
            if (programTextArray[i][j].Last() == '\"')
                return result.Substring(0, result.Length - 1) + '\0';
            if (programTextArray[i].Length > ++j == false)
            {
                if (programTextArray.Length > ++i == false)
                    throw new SimulatorException { ErrorMessage = $"the string: '{programTextArray[i][j]}' missing ends of string" };
                j = 0;
            }

            while (programTextArray[i][j][programTextArray[i][j].Length - 1] != '\"')
            {
                result += " " + programTextArray[i][j];
                if (programTextArray[i].Length > ++j == false)
                {
                    if (programTextArray.Length > ++i == false)
                        throw new SimulatorException { ErrorMessage = $"the string: '{programTextArray[i][j]}' missing ends of string" };
                    j = 0;
                }
            }
            result += " " + programTextArray[i][j].Substring(0, programTextArray[i][j].Length - 1) + '\0';
            return result;
        }
    }
}

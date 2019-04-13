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
        public static int FindNextLine(string content, ref int cursor)
        {
            return content.IndexOf("\n", cursor);
        }

        public static int FindNextEndingWord(string content, ref int cursor)
        {
            var a = content.IndexOf(" ", cursor, StringComparison.InvariantCultureIgnoreCase);
            var b = content.IndexOf("\n", cursor, StringComparison.InvariantCultureIgnoreCase);
            var c = content.IndexOf("\t", cursor, StringComparison.InvariantCultureIgnoreCase);
            if (a == -1)
            {
                if (b == -1)
                {
                    if (c == -1)
                        return content.Length;
                    return c;
                }
                else
                {
                    if (c == -1)
                        return b;
                    return Math.Min(c, b);
                }
            }
            else
            {
                if (b == -1)
                {
                    if (c == -1)
                        return a;
                    return Math.Min(a, c);

                }
                else
                {
                    if (c == -1)
                        return Math.Min(a, b);
                    return Math.Min(Math.Min(a, b), c);
                }
            }
        }

        public static int FindEndOfInstruction(string content, ref int cursor)
        {
            var end = content.IndexOf("\r", cursor, StringComparison.InvariantCultureIgnoreCase);
            if (end != -1)
                return end;
            end = content.IndexOf("#", cursor, StringComparison.InvariantCultureIgnoreCase);
            if (end != -1)
                return end;
            end = content.IndexOf("\n", cursor, StringComparison.InvariantCultureIgnoreCase);
            if (end != -1)
                return end;
            return content.Length;
        }

        public static int FindNextArg(string content, ref int cursor)
        {
            return content.IndexOf(",", cursor);
        }

        public static void SkipBlank(string content, ref int cursor, ref int lineNumber)
        {
            while (content[cursor] == ' ' || content[cursor] == '\r' || content[cursor] == '\t' ||
                   content[cursor] == '\n')
            {
                if (content[cursor] == '\n')
                    lineNumber++;
                cursor++;

            }

        }

        public static List<string> GetArgs(string reqProgram, ref int cursor, int numberToGet = 3)
        {
            List<string> result = new List<string>();
            int index;
            for (int i = 1; i < numberToGet; i++)
            {
                index = FindNextArg(reqProgram, ref cursor);
                if (index == -1)
                    throw new SimulatorException {ErrorMessage = $"got only {i} args out of {numberToGet}"};
                result.Add(reqProgram.Substring(cursor, index - cursor));
                cursor = index + 1;
            }

            index = FindEndOfInstruction(reqProgram, ref cursor);
            result.Add(reqProgram.Substring(cursor, index - cursor));
            cursor = index;
            return result;
        }

        public static string GetDirective(string reqProgram, ref int cursor)
        {
            cursor++;
            var index = FindNextEndingWord(reqProgram, ref cursor);
            var res = reqProgram.Substring(cursor, index - cursor);
            cursor = index;
            return res;
        }

        public static List<long> GetListOfNumber(string reqProgram, ref int cursor)
        {
            var index = FindEndOfInstruction(reqProgram, ref cursor);

            var text = reqProgram.Substring(cursor, index - cursor);
            text = text.Replace(" ", "");
            text = text.Replace("\t", "");
            cursor = index; //dont add +1 we dont know if its the end
            return text.Split(',').Select(x => long.Parse(x)).ToList();

        }

        public static bool IsLabel(string reqProgram, ref int cursor, out string label)
        {
            var index = FindNextEndingWord(reqProgram, ref cursor);
            label = reqProgram.Substring(cursor, index - cursor);
            if (label[label.Length - 1] == ':')
            {
                cursor = index + 1;
                label = label.Substring(0, label.Length - 1);
                if (string.IsNullOrWhiteSpace(label))
                    throw new SimulatorException {ErrorMessage = $"Label cannot be empty"};
                return true;
            }

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
                register = s.Substring(index1 + 1, index2 - index1);
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

        public static List<string> ParseJalr(string reqProgram, ref int cursor)
        {
            List<string> result = new List<string>();
            int index;

            index = FindNextArg(reqProgram, ref cursor);
            if (index == -1)
                throw new SimulatorException {ErrorMessage = $"missing args for jalr command"};
            if (reqProgram.Substring(cursor, index - cursor)
                .StartsWith("offset", StringComparison.InvariantCultureIgnoreCase))
            {
                result.Add("x1");
            }
            else
            {
                result.Add(reqProgram.Substring(cursor, index - cursor));
                cursor = index + 1;
            }

            if (IsComandWithOffset(reqProgram.Substring(cursor, index - cursor), out string register,
                out int offset))
            {
                result.Add(register);
                result.Add(offset.ToString());
            }
            else
            {
                throw new SimulatorException {ErrorMessage = $"missing args for jalr command"};
            }

            index = FindEndOfInstruction(reqProgram, ref cursor);
            cursor = index;
            return result;
        }
    }
}

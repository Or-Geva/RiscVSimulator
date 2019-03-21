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
                    if(c == -1)
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
             var end = content.IndexOf("\n", cursor, StringComparison.InvariantCultureIgnoreCase);
             if (end != -1)
                 return end;
            end = content.IndexOf("#", cursor, StringComparison.InvariantCultureIgnoreCase);
             if (end != -1)
                 return end;
             end = content.IndexOf(" ", cursor, StringComparison.InvariantCultureIgnoreCase);
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
            while (content[cursor] == ' ' || content[cursor] == '\t' || content[cursor] == '\n')
            {
                if (content[cursor] == '\n')
                    lineNumber++;
                cursor++;

            }

        }

        public static List<string> GetThreeArgs(string reqProgram, ref int cursor)
         {
             List <string> result = new List<string>();
             int index;
             for (int i = 0; i < 2; i++)
             {
                 index = FindNextArg(reqProgram, ref cursor);
                 if(index == -1)
                     throw new SimulatorException { ErrorMessage = $"got only {i+1} args out of 3" };
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
            cursor = index;//dont add +1 we dont know if its the end
            return text.Split(',').Select(x => long.Parse(x)).ToList();

        }

        public static bool IsLabel(string reqProgram, ref int cursor, out string label)
        {
           var index =  FindNextEndingWord(reqProgram, ref cursor);
            label = reqProgram.Substring(cursor, index - cursor);
            if (label[label.Length - 1] == ':')
            {
                cursor = index + 1;
                label = label.Substring(0, label.Length-1);
                return true;
            }
            return false;

        }

        public static bool IsLowLabelCommand(string value, out string label)
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
            throw new SimulatorException { ErrorMessage = $"'{value}' is not recognize command" };
        }

        public static int GetAddress(RiscVProgramResult res, MemorySection memorySection)
        {
            switch (memorySection)
            {
                case MemorySection.Text:
                    return (int) memorySection + res.StackTextFreePosition;
                case MemorySection.Static:
                    return (int)memorySection + res.StackStaticDataFreePosition;
                default:
                    throw new SimulatorException { ErrorMessage = $"'{memorySection}' cannot find fit section to return from f. GetAddress" };
            }
        }
    }
}

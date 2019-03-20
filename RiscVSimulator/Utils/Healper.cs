using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiscVSimulator.Utils
{
    public static class Healper
    {
        public static int FindNextLine(string content, ref int cursor)
        {
            return content.IndexOf("\n", cursor);
        }

         public static int FindNextSpace(string content, ref int cursor)
        {
            return content.IndexOf(" ", cursor);
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
            var index = FindNextSpace(reqProgram, ref cursor);
            var res = reqProgram.Substring(cursor, index - cursor);
            cursor = index + 1;
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
           var index =  FindNextSpace(reqProgram, ref cursor);
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
    }
}

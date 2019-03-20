using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RiscVSimulator.Model;
using RiscVSimulator.Utils;
using Microsoft.AspNetCore.Mvc;

namespace RiscVSimulator.Controllers

{

    [Route("api/[controller]")]
    [ApiController]
    public class SimulatorController : Controller
    {
        private static Dictionary<string, int> DirectiveNumber  = new Dictionary<string, int>
        {
            {"byte",1},{"word",4},{"string",1}
        };

        [HttpPost("ProgramToRun")]
        public async Task<ActionResult> ProgramToRun([FromBody] RiscVProgram req)
        {
            var cursor = 0;
            var commandInBinarryFormat = 0;
            var memorySection = MemorySection.Text;
            Dictionary<string, Lable> labelTable = new Dictionary<string, Lable>();
            Dictionary<int, (string,string)> uncompleteParse = new Dictionary<int, (string, string)>();
            RiscVProgramResult res = new RiscVProgramResult();
            bool newLabel = false;
            for (int i = 0; i < req.Program.Split('\n').Length; i++)
            {
                try
                {
                    Healper.SkipBlank(req.Program, ref cursor,ref i);
                    if(Healper.IsLabel(req.Program, ref cursor,out var label))
                    {
                        if(!labelTable.TryAdd(label,null))
                            return BadRequest(new ErrorInResult { Line = i, Message = $"The label {label} already exist." });
                        Healper.SkipBlank(req.Program, ref cursor, ref i);
                        newLabel = true;
                    }
                    if (req.Program[cursor] == '.')
                    {
                        var directive = Healper.GetDirective(req.Program, ref cursor);
                        if (DirectiveNumber.ContainsKey(directive))
                        {
                            if(labelTable[labelTable.Keys.Last()] == null)
                                labelTable[labelTable.Keys.Last()].Address = res.StackStaticDataFreePosition;
                            var numbers = Healper.GetListOfNumber(req.Program, ref cursor);
                            foreach (var number in numbers)
                            {
                                var longBytes = BitConverter.GetBytes(number);
                                for (int j = 0; j < DirectiveNumber[directive]; j++)
                                {
                                    if (longBytes.Length > j)
                                        res.Memory.Add((int)memorySection + res.StackStaticDataFreePosition, longBytes[j]);
                                    else
                                        res.Memory.Add((int)memorySection + res.StackStaticDataFreePosition, 0);

                                    res.StackStaticDataFreePosition++;
                                }
                            }
                        }
                        else
                        {
                            switch (directive)
                            {
                                case "data":
                                    memorySection = MemorySection.Static;
                                    break;
                                case "text":
                                    memorySection = MemorySection.Text;
                                    break;
                                default:
                                    throw new SimulatorException { ErrorMessage = $"'{directive}' is unknown  directive" };
                            }
                        }
                    }
                    else
                    {
                        commandInBinarryFormat = ParseCommandWithNoImm(req.Program, ref cursor);
                        if (commandInBinarryFormat == 0)
                        {
                            commandInBinarryFormat = ParseCommandWithImm(req.Program, ref cursor, out string optionalLabel,out string command);
                            if(optionalLabel != null)
                                uncompleteParse.Add((int)memorySection + res.StackTextFreePosition * 4, (optionalLabel, command));
                        }

                        res.Memory.Add((int)memorySection + res.StackTextFreePosition * 4, commandInBinarryFormat);
                        
                        if (newLabel)
                        {
                            labelTable[label] = new Lable{Address = (int)memorySection + res.StackTextFreePosition * 4};
                            newLabel = false;
                        }
                        res.StackTextFreePosition++;
                    }
                    GoToNextCommand(req.Program, ref cursor);
                }
                catch (SimulatorException e)
                {
                    return BadRequest(new ErrorInResult {Line = i, Message = e.ErrorMessage});
                }
                catch (Exception e)
                {
                    return BadRequest(new ErrorInResult {Line = i, Message = "Internal Error"});
                }
            }

            DoSecondParse(res, uncompleteParse, labelTable);

            return Ok(res);
        }

        private void DoSecondParse(RiscVProgramResult res, Dictionary<int, (string, string)> uncompleteParse,
            Dictionary<string, Lable> labelTable)
        {
            foreach (var commandToUpdate in uncompleteParse)
            {
                switch (commandToUpdate.Value.Item2)
                {
                    case "addi":
                        if (Healper.IsLowLabelCommand(commandToUpdate.Value.Item1, out string label))
                            res.Memory[commandToUpdate.Key] = res.Memory[commandToUpdate.Key] | labelTable[label].Address << 20;
                        else
                        {
                            labelTable[label].Address  = labelTable[label].Address >> 20;
                            res.Memory[commandToUpdate.Key] = res.Memory[commandToUpdate.Key] | labelTable[label].Address << 20;
                        }
                        break;
                }
            }
        }


        private void GoToNextCommand(string reqProgram, ref int cursor)
        {
            var index = Healper.FindNextLine(reqProgram, ref cursor);
            var text = "";
            if (index != -1)//if not end of program
            {
                text = reqProgram.Substring(cursor, index - cursor);
                cursor = index + 1;
            }
            else
            {
                text = reqProgram.Substring(cursor, reqProgram.Length - cursor);

            }
            if (text != "")
                foreach (var letter in text)
                {
                    if (letter == ' ' || letter == '\t')
                        continue;
                    if(letter == '#')
                        break;
                    throw new SimulatorException{ErrorMessage = $"'{letter}' is invalid argument"};
                }
        }

        private int ParseCommandWithNoImm(string reqProgram, ref int cursor)
        {
            var index = Healper.FindNextSpace(reqProgram, ref cursor);
            string ins = reqProgram.Substring(cursor, index - cursor);
            int result;

            switch (ins)
            {
                case "add":
                    result = Instructions.AddInstruction(reqProgram, ref cursor);
                    break;
                default:
                    return 0;
            }
            cursor = index + 1;
            return result;
        }

        private int ParseCommandWithImm(string reqProgram, ref int cursor, out string label, out string command)
        {
            var index = Healper.FindNextSpace(reqProgram, ref cursor);
            string ins = reqProgram.Substring(cursor, index - cursor);
            cursor = index + 1;
            label = null;
            command = null;
            switch (ins)
            {
                case "addi":
                    command = ins;
                    return Instructions.AddiInstruction(reqProgram, ref cursor,out label);
                default:
                    return 0;
            }
        }
    }

    enum MemorySection
    {
        Text = 0x10000,
        Static = 0x10000000,
        Dynamic,
        Stack
    }
}

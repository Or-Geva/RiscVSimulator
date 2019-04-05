using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using RiscVSimulator.Model;
using RiscVSimulator.Utils;

namespace RiscVSimulator.Controllers

{
    [EnableCors("SiteCorsPolicy")]
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
            RiscVProgramResult res = new RiscVProgramResult(req);
            List<RiscVProgramResult> debugRes =new List<RiscVProgramResult>();

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
                            if (newLabel)
                            {
                                labelTable[labelTable.Keys.Last()] = new Lable { Address = Healper.GetAddress(res, memorySection) };
                                newLabel = false;
                            }
                            var numbers = Healper.GetListOfNumber(req.Program, ref cursor);
                            foreach (var number in numbers)
                            {
                                var longBytes = BitConverter.GetBytes(number);
                                for (int j = 0; j < DirectiveNumber[directive]; j++)
                                {
                                    PutIntoAddress(res, memorySection, longBytes.Length > j ? longBytes[j] : (byte) 0);//we fill the memory with space if the number is not big enough  to fit the DirectiveNumber
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
                        commandInBinarryFormat = ParseCommandWithNoImm(req.Program, ref cursor,res.Register);
                        if (commandInBinarryFormat == 0)
                        {
                            commandInBinarryFormat = ParseCommandWithImm(req.Program, ref cursor, out string optionalLabel,out string command,res.Register);
                            if(optionalLabel != null)
                                uncompleteParse.Add(Healper.GetAddress(res, memorySection), (optionalLabel, command));
                        }
                        if (newLabel)
                        {
                            labelTable[label] = new Lable {Address = Healper.GetAddress(res, memorySection)};
                            newLabel = false;
                        }
                        var longBytes = BitConverter.GetBytes(commandInBinarryFormat);//Enter Command to Stack
                        for (int j = 0; j < 4; j++)
                        {
                            PutIntoAddress(res, memorySection, longBytes[j]);
                        }
                    }
                    GoToNextCommand(req.Program, ref cursor);
                }
                catch (SimulatorException e)
                {
                    return BadRequest(new ErrorInResult {Line = i+1, Message = e.ErrorMessage});
                }
                catch (Exception e)
                {
                    return BadRequest(new ErrorInResult {Line = i+1, Message = "Internal Error"});
                }
                if(req.DebugMode)
                    debugRes.Add(new RiscVProgramResult(res));
            }

            DoSecondParse(res, uncompleteParse, labelTable);

            if(req.DebugMode)
                return Ok(debugRes);
            return Ok(res);
        }

        private void PutIntoAddress(RiscVProgramResult res, MemorySection memorySection, byte longByte)
        {
            switch (memorySection)
            {
                case MemorySection.Text:
                    res.Memory.Add((int)memorySection + res.StackTextFreePosition, longByte);
                    res.StackTextFreePosition++;
                    break;
                case MemorySection.Static:
                    res.Memory.Add((int)memorySection + res.StackStaticDataFreePosition, longByte);
                    res.StackStaticDataFreePosition++;
                    break;
                default:
                    throw new SimulatorException { ErrorMessage = $"'{memorySection}' cannot find fit section to return from f. GetAddress" };
            }
        }

        private void DoSecondParse(RiscVProgramResult res, Dictionary<int, (string, string)> uncompleteParse,
            Dictionary<string, Lable> labelTable)
        {
            int commandToFix = 0;
          
            foreach (var commandToUpdate in uncompleteParse)
            {
                for (int j = 0; j < 4; j++)
                {
                    commandToFix |= res.Memory[commandToUpdate.Key + j] << 8 * j;
                }
                switch (commandToUpdate.Value.Item2)
                {
                    case "addi":
                        if (Healper.IsLowLabelCommand(commandToUpdate.Value.Item1, out string label))
                            commandToFix |= labelTable[label].Address << 20;
                        else
                        {
                            labelTable[label].Address  = labelTable[label].Address & 16773120;// we cant fit 20 high bit to imm 12 bit so we cut the bits from 13-20 (the high bits) and move the 12 to the left to fit the imm 12 bits
                            commandToFix |= labelTable[label].Address << 8;
                        }
                        break;
                }
                var longBytes = BitConverter.GetBytes(commandToFix);//Enter Command to Stack
                for (int j = 0; j < 4 ; j++)
                {
                    res.Memory[commandToUpdate.Key + j] = longBytes[j];
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

        private int ParseCommandWithNoImm(string reqProgram, ref int cursor, Register[] resRegister)
        {
            var index = Healper.FindNextEndingWord(reqProgram, ref cursor);
            string ins = reqProgram.Substring(cursor, index - cursor);
            int result;

            switch (ins)
            {
                case "add":
                    result = Instructions.AddInstruction(reqProgram, ref cursor, resRegister);
                    break;
                default:
                    return 0;
            }
            cursor = index + 1;
            return result;
        }

        private int ParseCommandWithImm(string reqProgram, ref int cursor, out string label, out string command,
            Register[] resRegister)
        {
            var index = Healper.FindNextEndingWord(reqProgram, ref cursor);
            string ins = reqProgram.Substring(cursor, index - cursor);
            cursor = index + 1;
            label = null;
            command = null;
            switch (ins)
            {
                case "addi":
                    command = ins;
                    return Instructions.AddiInstruction(reqProgram, ref cursor,out label, resRegister);
                default:
                    return 0;
            }
        }
    }


    public enum MemorySection
    {
        Text = 0x10000,
        Static = 0x10000000,
        Dynamic,
        Stack
    }
}

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
using Microsoft.EntityFrameworkCore.Internal;
using RiscVSimulator.Model;
using RiscVSimulator.Utils;

namespace RiscVSimulator.Controllers

{
    [EnableCors("SiteCorsPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class SimulatorController : Controller
    {
        private static Dictionary<string, int> DirectiveNumber = new Dictionary<string, int>
        {
            {"byte", 1}, {"word", 4}, {"string", 1}
        };

        [HttpPost("ProgramToRun")]
        public async Task<ActionResult> ProgramToRun([FromBody] RiscVProgram req)
        {
            var cursor = 0;
            var commandInBinarryFormat = 0;
            var memorySection = MemorySection.Text;
            Dictionary<string, Lable> labelTable = new Dictionary<string, Lable>();
            Dictionary<uint, UncompleteParseData> uncompleteParse = new Dictionary<uint, UncompleteParseData>();
            Dictionary<uint, ExeCommand> commandsToExe = new Dictionary<uint, ExeCommand>();
            RiscVProgramResult res = new RiscVProgramResult(req);
            List<RiscVProgramResult> debugRes = new List<RiscVProgramResult>();
            var TextArray = req.Program.Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            var programTextArray = new string[TextArray.Length][];
            for (int i = 0; i < TextArray.Length; i++)
            {
                programTextArray[i] = TextArray[i].Split(' ', '\t', '\r').Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToArray();
                ;
                if (programTextArray[i].FirstOrDefault(x => x.StartsWith('#')) != null)
                {
                    var index = programTextArray[i].IndexOf(programTextArray[i].FirstOrDefault(x => x.StartsWith('#')));
                    programTextArray[i] = programTextArray[i].Take(index).ToArray();
                }
            }

            bool newLabel = false;
            for (int i = 0; i < programTextArray.Length; i++)
            for (int j = 0; j < programTextArray[i].Length; j++)
            {
                try
                {
                    if (Healper.IsLabel(programTextArray, ref i, ref j, out var label))
                    {
                        if (!labelTable.TryAdd(label, null))
                            return BadRequest(new ErrorInResult
                                {Line = i, Message = $"The label {label} already exist."});
                        newLabel = true;
                    }

                    if (programTextArray[i][j][0] == '.')
                    {
                        var directive = Healper.GetDirective(programTextArray, ref i, ref j);
                        if (DirectiveNumber.ContainsKey(directive))
                        {
                            if (newLabel)
                            {
                                labelTable[labelTable.Keys.Last()] = new Lable
                                    {Address = Healper.GetAddress(res, memorySection), i = i, j = j};
                                newLabel = false;
                            }

                            if (programTextArray[i].Length > ++j == false)
                            {
                                if (programTextArray.Length > ++i == false)
                                    throw new SimulatorException {ErrorMessage = $"'incomplete command"};
                                j = 0;
                            }

                            if (directive == "string")
                            {
                                var items = Healper.PrepareString(programTextArray, ref i, ref j);
                                foreach (var item in items)
                                {
                                    var longBytes = BitConverter.GetBytes(item);
                                    for (int z = 0; z < DirectiveNumber[directive]; z++)
                                    {
                                        PutIntoAddress(res, memorySection,
                                            longBytes.Length > z
                                                ? longBytes[z]
                                                : (byte) 0); //we fill the memory with space if the number is not big enough  to fit the DirectiveNumber
                                    }
                                }
                            }
                            else
                            {
                                var items = Healper.GetListOfNumber(programTextArray, ref i, ref j);
                                foreach (var item in items)
                                {
                                    var longBytes = BitConverter.GetBytes(item);
                                    for (int z = 0; z < DirectiveNumber[directive]; z++)
                                    {
                                        PutIntoAddress(res, memorySection,
                                            longBytes.Length > z
                                                ? longBytes[z]
                                                : (byte) 0); //we fill the memory with space if the number is not big enough  to fit the DirectiveNumber
                                    }
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
                                    throw new SimulatorException
                                        {ErrorMessage = $"'{directive}' is unknown  directive"};
                            }
                        }
                    }
                    else
                    {
                        commandsToExe.Add(GetIntoAddress(res, memorySection), null);
                        commandInBinarryFormat = ParseCommandWithNoImm(programTextArray, ref i, ref j, commandsToExe);
                        if (commandInBinarryFormat == 0)
                        {
                            commandInBinarryFormat = ParseCommandWithImm(out string optionalLabel, out string command,
                                programTextArray, ref i, ref j, commandsToExe);
                            if (optionalLabel != null)
                                uncompleteParse.Add(Healper.GetAddress(res, memorySection),
                                    new UncompleteParseData(optionalLabel, command, i));
                        }

                        if (newLabel)
                        {
                            labelTable[label] = new Lable {Address = Healper.GetAddress(res, memorySection)};
                            newLabel = false;
                        }

                        var longBytes = BitConverter.GetBytes(commandInBinarryFormat); //Enter Command to Stack
                        for (int z = 0; z < 4; z++)
                        {
                            PutIntoAddress(res, memorySection, longBytes[z]);
                        }
                    }
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

            try
            {
                DoSecondParse(res, debugRes, req.DebugMode, uncompleteParse, labelTable, commandsToExe);
                ExeProgram(res, debugRes, req.DebugMode, commandsToExe, req.DebugMode);
            }
            catch (ErrorInResult e)
            {
                return BadRequest(new ErrorInResult {Line = e.Line, Message = e.Message});
            }
            catch (Exception e)
            {
                return BadRequest(new ErrorInResult {Message = "Internal Error"});
            }


            if (req.DebugMode)
                return Ok(debugRes);
            return Ok(res);
        }

        private void ExeProgram(RiscVProgramResult res, List<RiscVProgramResult> debugRes, bool reqDebugMode,
            Dictionary<uint, ExeCommand> commandsToExe, bool DebugMode)
        {
            var line = 0;
            string command = string.Empty;
            List<string> args = new List<string>();
            if (commandsToExe.Count == 0)
                return;

            bool finishExe = false;

            while (finishExe == false)
            {
                command = commandsToExe[(uint) res.Register[32].Value].Instraction;
                line = commandsToExe[(uint)res.Register[32].Value].Line;
                args = commandsToExe[(uint)res.Register[32].Value].Args;
                switch (command)
                {
                    case "add":
                    case "sub":
                    case "sll":
                    case "slt":
                    case "sltu":
                    case "xor":
                    case "srl":
                    case "sra":
                    case "or":
                    case "and":
                        Instructions.ExeRInstruction(res, command,args);
                        break;
                    case "srai":
                    case "slli":
                    case "srli":
                        Instructions.ExeShamtIInstruction(res, command, args);
                        break;
                    case "addi":
                    case "slti":
                    case "sltiu":
                    case "xori":
                    case "ori":
                    case "andi":
                    case "jalr":
                        Instructions.ExeIInstruction(res, command,args);
                        break;
                    case "lb":
                    case "lh":
                    case "lw":
                    case "lbu":
                    case "lhu":
                        Instructions.ExeLoadInstruction(res, command, args);
                        break;
                    case "sb":
                    case "sh":
                    case "sw":
                        Instructions.ExeStoreInstruction(res, command, args);
                        break;
                    case "beq":
                    case "bne":
                    case "blt":
                    case "bge":
                    case "bltu":
                    case "bgeu":
                        Instructions.ExeBInstruction(res, command, args);
                        break;
                    case "lui":
                    case "auipc":
                        Instructions.ExeUInstruction(res, command, args);
                        break;
                    case "jal":
                    case "j":
                        Instructions.ExeJInstruction(res, command, args);
                        break;
                    case "ecall":
                    case "ebreak":
                        Instructions.ExeEInstruction(res, command, args,ref finishExe);
                        break;
                    default:
                        throw new SimulatorException
                            {ErrorMessage = $"'unknown '{command}' command"};
                }

                res.Register[32].Value += 4;

                if (commandsToExe.ContainsKey((uint)res.Register[32].Value) == false)
                {
                    debugRes.Add(new RiscVProgramResult(res, line));
                    finishExe = true;
                }
                else
                {
                    if (DebugMode)
                        debugRes.Add(new RiscVProgramResult(res, line));
                }
            }
        }

        private void PutIntoAddress(RiscVProgramResult res, MemorySection memorySection, byte longByte)
        {
            switch (memorySection)
            {
                case MemorySection.Text:
                    res.Memory.Add(0x10000 + res.StackTextFreePosition, longByte);
                    res.StackTextFreePosition++;
                    break;
                case MemorySection.Static:
                    res.Memory.Add((uint)0x10000000 + res.StackStaticDataFreePosition, longByte);
                    res.StackStaticDataFreePosition++;
                    break;
                default:
                    throw new SimulatorException
                        {ErrorMessage = $"'{memorySection}' cannot find fit section to return from f. GetAddress"};
            }
        }

        public uint GetIntoAddress(RiscVProgramResult res, MemorySection memorySection)
        {
            switch (memorySection)
            {
                case MemorySection.Text:
                    return 0x10000 + res.StackTextFreePosition;
                case MemorySection.Static:
                    return (uint)0x10000000 + res.StackStaticDataFreePosition;
                case MemorySection.Dynamic:
                    return (uint)0x10000000 + res.StackStaticDataFreePosition + res.StackStaticDataFreePosition;
                default:
                    throw new SimulatorException
                        {ErrorMessage = $"'{memorySection}' cannot find fit section to return from f. GetAddress"};
            }
        }

        private void DoSecondParse(RiscVProgramResult res,
            List<RiscVProgramResult> debugRes,
            bool DebugMode, Dictionary<uint, UncompleteParseData> uncompleteParse,
            Dictionary<string, Lable> labelTable, Dictionary<uint, ExeCommand> commandsToExe)
        {
            int commandToFix = 0;
            bool lowLabelCommand = false;
            foreach (var commandToUpdate in uncompleteParse)
            {
                try
                {
                    for (int j = 0; j < 4; j++) //read command from memory, each command (32b) needs 4 bytes
                    {
                        commandToFix |= res.Memory[(uint) (commandToUpdate.Key + j)] << 8 * j;
                    }

                    lowLabelCommand = Healper.IsLowLabelCommand(commandToUpdate.Value.optionalLabel, out var label);
                    if (!labelTable.ContainsKey(label))
                        throw new SimulatorException {ErrorMessage = $"cannot find label {label}"};

                    switch (commandToUpdate.Value.command)
                    {
                        case "addi":
                        case "slti":
                        case "sltiu":
                        case "xori":
                        case "ori":
                        case "andi":
                        case "jalr":
                            if (lowLabelCommand)
                            {

                                commandsToExe[commandToUpdate.Key].Args[2] =
                                    (labelTable[label].Address & Convert.ToInt32("111111111111", 2)).ToString();
                                commandToFix |= ((int)labelTable[label].Address & Convert.ToInt32("111111111111", 2)) << 20;
                            }
                            else
                            {
                                // we cant fit 20 high bit to imm 12 bit so we cut the bits from 12-19 (the high bits) and move them to the left(20-31) to fit the imm 12 bits
                                labelTable[label].Address &= 16773120; //CUT
                                commandToFix |= (int)labelTable[label].Address << 8; //MOVE + MERGE
                                commandsToExe[commandToUpdate.Key].Args[2] =
                                    ((labelTable[label].Address &
                                      Convert.ToInt32("00000000111111111111000000000000", 2)) >> 12).ToString();
                            }

                            break;

                        case "lui":
                            if (lowLabelCommand)
                            {

                                commandsToExe[commandToUpdate.Key].Args[1] =
                                    (labelTable[label].Address & Convert.ToInt32("111111111111", 2)).ToString();
                                commandToFix |= ((int)labelTable[label].Address & Convert.ToInt32("111111111111", 2)) << 12;
                            }
                            else
                            {
                                commandsToExe[commandToUpdate.Key].Args[1] =
                                    (UInt32.Parse(labelTable[label].Address.ToString()) >> 12).ToString();
                                commandToFix |= (int)labelTable[label].Address &
                                                Convert.ToInt32("11111111111111111111000000000000", 2);
                            }

                            break;

                        case "auipc":
                            if (lowLabelCommand)
                            {

                                commandsToExe[commandToUpdate.Key].Args[1] =
                                   ((int)labelTable[label].Address & Convert.ToInt32("111111111111", 2)).ToString();
                                commandToFix |= ((int)labelTable[label].Address & Convert.ToInt32("111111111111", 2)) << 12;
                            }
                            else
                            {
                                //commandsToExe[commandToUpdate.Key].Args[1] = (labelTable[label].Address & Convert.ToInt32("11111111111111111111000000000000", 2)).ToString(); 
                                commandsToExe[commandToUpdate.Key].Args[1] =
                                    (UInt32.Parse(labelTable[label].Address.ToString()) >> 12).ToString();
                                commandToFix |= (int)labelTable[label].Address &
                                                Convert.ToInt32("11111111111111111111000000000000", 2);
                            }

                            break;
                    }

                    var longBytes = BitConverter.GetBytes(commandToFix); //Enter Command to Stack
                    if (DebugMode)
                    {
                        foreach (var snapshotMemory in debugRes)
                        {
                            if (debugRes.IndexOf((snapshotMemory)) >= commandToUpdate.Value.lineNumber)
                            {
                                for (int j = 0; j < 4; j++)
                                {
                                    snapshotMemory.Memory[(uint) (commandToUpdate.Key + j)] = longBytes[j];
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            res.Memory[(uint) (commandToUpdate.Key + j)] = longBytes[j];
                        }
                    }
                }

                catch (SimulatorException e)
                {
                    throw new ErrorInResult {Line = commandToUpdate.Value.lineNumber, Message = e.ErrorMessage};
                }
                catch (Exception e)
                {
                    throw new ErrorInResult {Line = commandToUpdate.Value.lineNumber, Message = "Internal Error"};
                }
            }

        }

        private int ParseCommandWithNoImm(string[][] programTextArray, ref int i, ref int j,
            Dictionary<uint, ExeCommand> commandsToExe)
        {
            int result;
            switch (programTextArray[i][j])
            {
                case "add":
                case "sub":
                case "sll":
                case "slt":
                case "sltu":
                case "xor":
                case "srl":
                case "sra":
                case "or":
                case "and":
                    result = Instructions.RInstruction(programTextArray, ref i, ref j, commandsToExe);
                    break;
                case "lb":
                case "lh":
                case "lw":
                case "lbu":
                case "lhu":
                    result = Instructions.LoadIInstruction(programTextArray, ref i, ref j, commandsToExe);
                    break;
                case "sb":
                case "sh":
                case "sw":
                    result = Instructions.StoreInstruction(programTextArray, ref i, ref j, commandsToExe);
                    break;
                case "beq":
                case "bne":
                case "blt":
                case "bge":
                case "bltu":
                case "bgeu":
                    result = Instructions.BInstruction(programTextArray, ref i, ref j, commandsToExe);
                    break;
                case "jal":
                case "j":
                    result = Instructions.JInstruction(programTextArray, ref i, ref j, commandsToExe);
                    break;
                case "ecall":
                case "ebreak":
                    result = Instructions.EInstruction(programTextArray, ref i, ref j, commandsToExe);
                    break;
                default:
                    return 0;
            }

            return result;
        }

        private int ParseCommandWithImm(out string label, out string command, string[][] programTextArray, ref int i,
            ref int j, Dictionary<uint, ExeCommand> commandsToExe)
        {
            label = null;
            command = null;
            switch (programTextArray[i][j])
            {
                case "srai":
                case "slli":
                case "srli":
                    command = programTextArray[i][j];
                    return Instructions.ShamtIInstruction(programTextArray, ref i, ref j, commandsToExe);
                case "addi":
                case "slti":
                case "sltiu":
                case "xori":
                case "ori":
                case "andi":
                case "jalr":
                    command = programTextArray[i][j];
                    return Instructions.IInstruction(programTextArray, ref i, ref j, out label, commandsToExe);
                case "lui":
                case "auipc":
                    command = programTextArray[i][j];
                    return Instructions.UInstruction(programTextArray, ref i, ref j, out label, commandsToExe);
                default:
                    throw new SimulatorException {ErrorMessage = $"'{programTextArray[i][j]}' is invalid instruction"};
            }
        }
    }


    public enum MemorySection
    {
        Text ,
        Static ,
        Dynamic
    }
}

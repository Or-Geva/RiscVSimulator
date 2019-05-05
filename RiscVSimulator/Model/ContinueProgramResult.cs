using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiscVSimulator.Model
{
    public class ContinueProgramResult
    {
        public RiscVProgramResult res { get; set; }
        public List<RiscVProgramResult> debugRes { get; set; }
        public Dictionary<uint, ExeCommand> commandsToExe { get; set; }
        public Dictionary<string, uint> stringTable { get; set; }
        public bool DebugMode { get; set; }

        public ContinueProgramResult()
        {
            res = new RiscVProgramResult();
            debugRes = new List<RiscVProgramResult>();
            commandsToExe = new Dictionary<uint, ExeCommand>();
            stringTable = new Dictionary<string, uint>();
        }

    }
}

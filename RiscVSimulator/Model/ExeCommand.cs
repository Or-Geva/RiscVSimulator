using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiscVSimulator.Model
{
    public class ExeCommand
    {
        public string Instraction { get; set; }
        public List<string> Args { get; set; }

        public ExeCommand()
        {
            Args = new List<string>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiscVSimulator.Model
{
    public class AlphanumericData
    {
        public int Line { get; set; }
        public string Input { get; set; }
        public int LastChar { get; set; }
        public List<string> Output { get; set; }


        public AlphanumericData()
        {
            Output = new List<string>();
            Line = -1;
            LastChar = -1;
        }
    }
}

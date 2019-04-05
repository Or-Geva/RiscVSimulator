using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiscVSimulator.Model
{
    [Serializable]
    public class Register
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
}

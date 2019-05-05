using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiscVSimulator.Model
{
    public class GraphicBorder
    {
        public List<Point> Points { get; set; }
        public string Color { get; set; }
    }

    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}

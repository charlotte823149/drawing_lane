using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace drawing_lane.Data
{
    public class damageData
    {
        public double Longth { get; set; }
        public double Width { get; set; }
        public double Area { get; set; }
        public string Level { get; set; }
        public string Category { get; set; }
        public Point LeftPoint { get; set; }
        public Point RightPoint { get; set; }
        public int Lane { get; set; }
    }
}

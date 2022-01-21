using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace drawing_lane.Data
{
    public class damageData
    {
        public string Category { get; set; }
        public string Level { get; set; }
        public int Lane { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Area { get; set; }
        public double[] Box { get; set; }
        public List<List<int[]>> contours { get; set; }

    }
}

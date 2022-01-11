using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drawing_lane.Data
{
    public class statisticsData
    {
        public List<double> Longth { get; set; }
        public List<double> Width { get; set; }
        public List<double> Area { get; set; }
        public string Level { get; set; }
        public string Category { get; set; }
        public double density { get; set; }
        public double dv { get; set; }
    }
}

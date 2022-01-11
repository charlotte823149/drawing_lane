using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drawing_lane.Data
{
    public class basicData
    {
        public string Date { get; set; }
        public string RoadName { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public int Stake { get; set; }
        public int Space { get; set; }
        public int LaneType { get; set; } = 0;
        public double LaneWidth { get; set; } = 0;
        public double LaneLongth { get; set; } = 0;
    }
}

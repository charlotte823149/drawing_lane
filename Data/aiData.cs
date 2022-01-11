using System.Collections.Generic;

namespace drawing_lane.Data
{
    public class aiData
    {
        public string Date { get; set; }
        public string License { get; set; }
        public string OriginalName { get; set; }
        public string AfterName { get; set; }
        public List<damageData> Damages { get; set; } = new List<damageData>();
        public double Longtitude { get; set; }
        public double Latitude { get; set; }
        public List<double> PCI { get; set; }
        public int Lane { get; set; } = 0;
    }
}

using System.Collections.Generic;

namespace drawing_lane.Data
{
    public class aiData
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public string License { get; set; }
        public string OriginalName { get; set; }
        public string AfterName { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public List<damageData> Shape { get; set; } = new List<damageData>();
        
        public List<double> PCI { get; set; }
        public int LaneNumber { get; set; } = 0;
    }
}

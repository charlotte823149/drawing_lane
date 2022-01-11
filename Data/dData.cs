using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drawing_lane.Data
{
    public class dData
    {
        public string classes { get; set;}
        public float scores { get; set; }
        public double[] box { get; set; }
        public List<List<int[]>> contours { get; set; }
    }
}

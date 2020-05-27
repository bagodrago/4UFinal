using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4UFinal
{
    class Room
    {
        // <declaration>
        private List<Prop> l_northFurniture = new List<Prop>(); // 0 => north, 1 => south
        private List<Prop> l_southFurniture = new List<Prop>();
        // </declaration>

        // <initialization>
        public Room(List<Prop> northFurniture, List<Prop> southFurniture)
        {
            l_northFurniture = northFurniture;
            l_southFurniture = southFurniture;
        }
        // </initialization>

        // <get-sets>
        public List<Prop> North
        {
            get => l_northFurniture;
            set
            {
                l_northFurniture = value;
            }
        }
        public List<Prop> South
        {
            get => l_southFurniture;
            set
            {
                l_southFurniture = value;
            }
        }
        // </get-sets>

        // <methods>
        public Room Clone()
        {
            Room temp = new Room(l_northFurniture.ToList(), l_southFurniture.ToList());
            return temp;
        }
        // </methods>
    }
}

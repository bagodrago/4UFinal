using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace _4UFinal
{
    class Item
    {
        // <declaration>
        private string l_name;
        private string l_desciption;
        private readonly BitmapImage l_portrait;
        // </declaration>

        // <initialization>
        public Item(string name, string description, BitmapImage portrait)
        {
            l_name = name;
            l_desciption = description;
            l_portrait = portrait;
        }
        // </initialization>

        // <get-sets>
        public string Name
        {
            get => l_name;
            set
            {
                l_name = value;
            }
        }

        public string Description
        {
            get => l_desciption;
            set
            {
                l_desciption = value;
            }
        }

        public BitmapImage Portrait
        {
            get => l_portrait;
        }
        // </get-sets>
    }
}

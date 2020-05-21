using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;

namespace _4UFinal
{
    class Item
    {
        // <declaration>
        private string l_name;
        private string l_desciption;
        private BitmapImage l_portrait;
        private BitmapImage l_sprite;
        private Point l_coordinates;
        // </declaration>

        // <initialization>
        public Item(string name, string description, BitmapImage portrait, BitmapImage sprite)
        {
            l_name = name;
            l_desciption = description;
            l_portrait = portrait;
            l_sprite = sprite;
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

        public BitmapImage Sprite
        {
            get => l_sprite;
        }

        public Point Coordinates
        {
            get => l_coordinates;
            set
            {
                l_coordinates = value;
            }
        }
        // </get-sets>
    }
}

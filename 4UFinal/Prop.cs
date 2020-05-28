using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace _4UFinal
{
    class Prop
    {
        // <declaration>
        private string l_name;
        private string l_desciption;
        private Image l_sprite = new Image();
        // </declaration>

        // <initialization> - Name>Description>Source>X>Y>Warp
        public Prop(string name, string description, BitmapImage sprite, Thickness dimensions)
        {
            l_name = name;
            l_desciption = description;
            l_sprite.Source = sprite;
            l_sprite.Margin = dimensions;
            l_sprite.Cursor = Cursors.Hand;
            l_sprite.Height = sprite.Height * 3.124993642171224333836864399119d;
            l_sprite.Width = sprite.Width * 3.124993642171224333836864399119d;
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

        public Image Sprite
        {
            get => l_sprite;
            set
            {
                l_sprite = value;
            }
        }
        // </get-sets>
    }
}

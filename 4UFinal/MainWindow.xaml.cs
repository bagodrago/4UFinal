using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace _4UFinal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // <RO Variables> - Read-only variables
        static List<BitmapImage> assets = new List<BitmapImage>() { }; // Static assets (do not change during the game)
        static List<BitmapImage> props = new List<BitmapImage>() { }; // Character portraits for text box
        static List<BitmapImage> items = new List<BitmapImage>() { }; // Item portraits for text box
        static List<BitmapImage> backgrounds = new List<BitmapImage>() { }; // Holds the backgrounds for the stage
        List<Image> invSlots = new List<Image>() { }; // A list of inventory slots that are parented to the inventory menu. This makes it easier to display the inventory items.
        List<Item> itemDB = new List<Item>() { }; // A database of all items to refer to when adding to inventory.
        // </RO Variables>

        // <RW Variables> - Read and write variables
        public bool isFullscreen = false; // Is F11 mode is on?
        public bool changingFullscreen = false; // Is F11 currently changing?
        public bool changingInventory = false; // Is the inventory menu currently being opened?
        public bool changingItemslot = false; // Is an item currently being selected?
        public bool changingRoom = false; // Is the stage switching between rooms?
        ImageBrush bk = new ImageBrush(); // TileBrush for the background

        List<Item> inventory = new List<Item>() {}; // Inventory
        List<Room> mansion = new List<Room>() {}; // All rooms in the game
        int currentRoom = 0;
        Item selectedItem; // The item that appears in the item slot
        bool facingNorth = true; // Which direction the player faces.
        // </RW Variables>
        
        List<bool> conditions = new List<bool>() //Events triggers
        {
            false, // [0] Red keycard picked up
            false, // [1] Door 0-1 unlocked
            false, // [2] Door 0-1 entered
            false,
            false
        };

        public MainWindow()
        {
            InitializeComponent();
            // <loadFiles>
            assets = LoadImages(@".\img\assets");
            props = LoadImages(@".\img\props");
            items = LoadImages(@".\img\items");
            backgrounds = LoadImages(@".\img\background");
            // </loadFiles>
            // <tiling> - Creates tiling background
            bk = new ImageBrush(assets[0])
            {
                TileMode = TileMode.Tile,
                Stretch = Stretch.None,
                Viewport = new Rect(0, 0, 32, 32),
                ViewportUnits = BrushMappingMode.Absolute
            };
            OuterScreen.Background = bk;
            // </tiling>
            // <loadAssets>
            TextCanvas.Background = new ImageBrush(assets[1]);
            InventoryCanvas.Background = new ImageBrush(assets[2]);
            TextPortrait.Source = null;
            ItemBox.Source = items[0];
            // </loadAssets>
            //<initializeSystems>
            CreateInventory();
            CreateMansion();
            CreateItemDB();
            ClearPrint();
            RefreshStage();
            // </initializeSystems>
            // <debugging>
            inventory.Add(itemDB[0]);
            inventory.Add(itemDB[1]);
            // </debugging>
        }

        private List<BitmapImage> LoadImages(string path) // Loads images from a specified folder and returns a list of ImageBrushs
        {
            string[] temp = Directory.GetFiles(path, "*.png");
            Array.Sort(temp);
            List<BitmapImage> container = new List<BitmapImage>();
            foreach (string file in temp) { container.Add(new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, file)))); }
            return container;
        }

        private void ShowHide(Canvas canv) // Toggles the visibility of a specified canvas. Mainly for menus.
        {
            if (canv.Visibility == Visibility.Visible)
            {
                canv.Visibility = Visibility.Hidden;
            }
            else
            {
                canv.Visibility = Visibility.Visible;
            }
        }

        private void RefreshStage() // Reload props and background for changing rooms.
        {
            Stage.Children.Clear();
            if (facingNorth)
            {
                for (int i = 0; i < mansion[currentRoom].North.Count(); i++)
                {
                    Stage.Children.Add(mansion[currentRoom].North[i].Sprite);
                }
            }
            else
            {
                for (int i = 0; i < mansion[currentRoom].South.Count(); i++)
                {
                    Stage.Children.Add(mansion[currentRoom].South[i].Sprite);
                }
            }
            Stage.Background = new ImageBrush() { ImageSource = backgrounds[currentRoom] };
        }

        private void RefreshItem()
        {
            selectedItem = null;
            ItemBox.Source = items[0];
        }

        private void RefreshProp(int room, string name, BitmapImage newImage)
        {
            mansion[room].North.Find((r => r.Name == name)).Sprite.Source = newImage;
        }

        private void RefreshProp(int room, string name, string newDescription)
        {
            mansion[room].North.Find((r => r.Name == name)).Description = newDescription;
        }

        //<initialization>
        private void CreateInventory() // Initializes inventory slots
        {
            Image img;
            double tempX = 20;
            double tempY = 20;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    img = new Image()
                    {
                        Height = 140,
                        Width = 140,
                        Margin = new Thickness(tempX, tempY, 0, 0),
                        Cursor = Cursors.Hand
                    };
                    img.MouseDown += InventorySlot_MouseDown;
                    img.MouseEnter += InventorySlot_MouseEnter;
                    img.MouseLeave += InventorySlot_MouseLeave;
                    invSlots.Add(img);
                    tempX += 192.5;
                }
                tempX = 20;
                tempY += 185;
            }

            foreach (Image slot in invSlots)
            {
                InventoryCanvas.Children.Add(slot);
            }
        }

        private void CreateMansion() // Reads room files and loads mansion with rooms and props.
        {
            string[] room;
            string[] tempProps;
            Room tempRoom;
            Prop tempProp;
            try
            {
                tempRoom = new Room(new List<Prop>(), new List<Prop>());
                for (int i = 0; i < 6; i++)
                {
                    room = File.ReadAllLines(@".\rooms\north\" + i + ".txt");
                    for (int j = 0; j < room.Length; j++)
                    {
                        tempProps = room[j].Split('>'); // Name>Description>Source>X>Y>Warp
                        tempProp = new Prop(tempProps[0], tempProps[1], props[int.Parse(tempProps[2])], new Thickness(int.Parse(tempProps[3]), int.Parse(tempProps[4]), 0, 0));
                        tempProp.Sprite.MouseEnter += Prop_MouseEnter;
                        tempProp.Sprite.MouseLeave += InventorySlot_MouseLeave;
                        tempProp.Sprite.MouseDown += PropPressed;
                        tempRoom.North.Add(tempProp);
                    }
                    room = File.ReadAllLines(@".\rooms\south\" + i + ".txt");
                    for (int j = 0; j < room.Length; j++)
                    {
                        tempProps = room[j].Split('>'); // Name>Description>Source>X>Y>Warp
                        tempProp = new Prop(tempProps[0], tempProps[1], props[int.Parse(tempProps[2])], new Thickness(int.Parse(tempProps[3]), int.Parse(tempProps[4]), 0, 0));
                        tempProp.Sprite.MouseEnter += Prop_MouseEnter;
                        tempProp.Sprite.MouseLeave += InventorySlot_MouseLeave;
                        tempRoom.South.Add(tempProp);
                    }

                    mansion.Add(tempRoom.Clone());
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), "Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateItemDB()
        {
            string[] tempLine;
            Item tempItem;
            try
            {
                string[] lines = File.ReadAllLines(@".\db\items.txt");
                for (int i = 0; i < lines.Length; i++)
                {
                    tempLine = lines[i].Split('>'); // Name>Description>Source
                    tempItem = new Item(tempLine[0], tempLine[1], items[int.Parse(tempLine[2])]);
                    itemDB.Add(tempItem);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), "Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //</initialization>

        //<dialogue>
        private void PrintObject(Item selected, bool name = true, bool description = true, bool portrait = false) // Displays on-screen information about the object
        {
            if (name) DialogueName.Text = selected.Name;
            if (description) DialogueBox.Text = selected.Description;
            if (portrait) TextPortrait.Source = selected.Portrait;
        }

        private void PrintObject(Prop selected, bool name = true, bool description = true) // Displays on-screen information about the object
        {
            if (name) DialogueName.Text = selected.Name;
            if (description) DialogueBox.Text = selected.Description;
        }

        private void PrintText(string dialogue)
        {
            ClearPrint();
            DialogueBox.Text = dialogue;
        }

        private void ClearPrint() // Removes text and images from the dialogue box
        {
            DialogueName.Text = string.Empty;
            DialogueBox.Text = string.Empty;
            TextPortrait.Source = null;
        }
        //</dialogue>

        //<propInteraction>
        private void PropPressed(object sender, MouseButtonEventArgs e)  //
        {
            Image pressed = e.Source as Image;
            string parent;
            if (facingNorth)
            {
                parent = mansion[currentRoom].North.Find(p => p.Sprite == pressed).Name;
            }
            else
            {
                parent = mansion[currentRoom].South.Find(p => p.Sprite == pressed).Name;
            }
            switch (currentRoom) // Interaction for all props.
            {
                // <Room 1>
                case 0:
                    switch (parent)
                    {
                        case "Card Reader":
                            if (selectedItem.Name == "Red Card")
                            {
                                inventory.Remove(inventory.Find((r => r == itemDB[0])));
                                PrintText("Nice! The door unlocked!");
                                conditions[1] = true;
                                RefreshProp(0, "Card Reader", props[3]);
                                RefreshProp(0, "Card Reader", "Looks like the door is unlocked now.");
                                RefreshProp(0, "Door", "This door leads to a new room.");
                                RefreshItem();
                            }
                            break;
                        case "Sofa Chair":
                            if (!conditions[0])
                            {
                                inventory.Add(itemDB[0]);
                                PrintText("Hey, there was a keycard under the cushion!");
                                conditions[0] = true;
                                RefreshProp(0, "Sofa Chair", "This chair looks pretty comfortable.");
                            }
                            break;
                    }
                    break;
                // </Room 1>

                // <Room 2>
                case 1:
                    switch (parent)
                    {

                    }
                    break;
                // </Room 2>

                // <Room 3>
                case 2:
                    switch (parent)
                    {

                    }
                    break;
                // </Room 3>

                // <Room 4>
                case 3:
                    switch (parent)
                    {

                    }
                    break;
                // </Room 4>

                // <Room 5>
                case 4:
                    switch (parent)
                    {

                    }
                    break;
                // </Room 5>

                default:
                    break;
            }
        }
        //</propInteraction>

        //<events>
        private void OuterScreen_KeyDown(object sender, KeyEventArgs e) // Activates and deactivates F11 Mode
        {
            if (e.Key == Key.F11 && !changingFullscreen)
            {
                changingFullscreen = true;
                switch (isFullscreen)
                {
                    case true:
                        WindowStyle = WindowStyle.SingleBorderWindow;
                        WindowState = WindowState.Maximized;
                        ResizeMode = ResizeMode.CanResize;
                        isFullscreen = false;
                        break;
                    default:
                        WindowStyle = WindowStyle.None;
                        WindowState = WindowState.Normal;
                        WindowState = WindowState.Maximized;
                        ResizeMode = ResizeMode.NoResize;
                        isFullscreen = true;
                        break;
                }
                changingFullscreen = false;
            }
        }

        private void ItemBox_MouseDown(object sender, MouseButtonEventArgs e) // Opens and closes the inventory menu
        {
            if (!changingInventory)
            {
                changingInventory = true;
                ShowHide(InventoryCanvas);
                for (int i = 0; i < 15; i++)
                {
                    if (i < inventory.Count) invSlots[i].Source = inventory[i].Portrait;
                    else invSlots[i].Source = items[0];
                }
                changingInventory = false;
            }
        }

        private void InventorySlot_MouseDown(object sender, MouseButtonEventArgs e) // Event handler for clicking each inventory slot
        {
            if (!changingItemslot)
            {
                changingItemslot = true;
                Image root = e.Source as Image;
                int index = invSlots.FindIndex((r => r == root));
                if (index > inventory.Count() - 1)
                {
                    selectedItem = null;
                }
                else
                {
                    selectedItem = inventory[index];
                }
                ItemBox.Source = (selectedItem == null ? items[0] : selectedItem.Portrait); // If there is nothing selected, the item slot is empty
                ShowHide(InventoryCanvas);
                changingItemslot = false;
            }
        }

        private void InventorySlot_MouseEnter(object sender, MouseEventArgs e) // Prints description and name of object the mouse is hovering over
        {
            Image root = e.Source as Image;
            int index = invSlots.FindIndex((r => r == root));
            if (index > inventory.Count() - 1)
            {
                ClearPrint();
            }
            else
            {
                PrintObject(inventory[index], true, true, false);
            }
        }

        public void Prop_MouseEnter(object sender, MouseEventArgs e) // Prints description of prop when mouse hovers over it
        {
            Image root = e.Source as Image;
            Prop temp;
            if (facingNorth)
            {
                temp = mansion[currentRoom].North.Find(r => r.Sprite == root);
            }
            else
            {
                temp = mansion[currentRoom].South.Find(r => r.Sprite == root);
            }
            PrintObject(temp);
        }

        private void InventorySlot_MouseLeave(object sender, MouseEventArgs e) // Clears the print when hovering over nothing
        {
            ClearPrint();
        }

        private void NSButton_MouseDown(object sender, MouseButtonEventArgs e) // Switches between north and south facing
        {
            if (!changingRoom)
            {
                changingRoom = true;
                facingNorth = !facingNorth;
                RefreshStage();
                changingRoom = false;
            }
        }

        //</events>
    }
}

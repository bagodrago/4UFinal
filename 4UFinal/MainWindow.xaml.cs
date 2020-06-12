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
using System.Windows.Threading;
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
        //DispatcherTimer timer = new DispatcherTimer(); // Timer used strictly for delay.
        // </RO Variables>

        // <RW Variables> - Read and write variables
        public bool isFullscreen = false; // Is F11 mode is on?
        public bool changingFullscreen = false; // Is F11 currently changing?
        public bool changingInventory = false; // Is the inventory menu currently being opened?
        public bool changingItemslot = false; // Is an item currently being selected?
        public bool changingRoom = false; // Is the stage switching between rooms?
        public bool pressingProp = false; // Is a PropPressed event currently being called?
        ImageBrush bk = new ImageBrush(); // TileBrush for the background
        object downOn = null;

        List<Item> inventory = new List<Item>() {}; // Inventory
        List<Room> mansion = new List<Room>() {}; // All rooms in the game
        int currentRoom = 0;
        Item selectedItem = new Item(); // The item that appears in the item slot
        bool facingNorth = true; // Which direction the player faces.
        // </RW Variables>
        
        List<bool> conditions = new List<bool>() //Events triggers
        {
            false, // [0] Red keycard picked up
            false, // [1] Door 0-1 unlocked
            false, // [2] Door 0-1 entered
            false, // [3] Door 1-2 entered
            false, // [4] Door 1-4 entered
            false, // [5] Torch picked up
            false, // [6] Torch soaked
            false, // [7] Torch lit
            false, // [8] Key picked up
            false, // [9] Blue keycard picked up
            false, // [10] Door 2-3 unlocked
            false, // [11] Door 2-3 entered
            false, // [12] Knife picked up
            false, // [13] Screwdriver picked up
            false, // [14] Hammer picked up
            false, // [15] Door 4-5 entered
            false, // [16] Yellow Keycard 1 found
            false, // [17] Yellow Keycard 2 found
            false, // [18] Yellow Keycard 3 found
            false, // [19] A yellow keycard found
            false, // [20] Lock A opened
            false, // [21] Lock B opened
            false, // [22] Lock C opened
            false  // [23] Torch stored successfully
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
            // <timer>
            //timer.Interval = TimeSpan.FromMilliseconds(40);
            //timer.Tick += TimerTick;
            // </timer>
            // <debugging>

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
            for (int i = 0; i < Stage.Children.Count; i++)
            {
                Stage.Children[i].MouseLeftButtonUp += PropPressed;
                Stage.Children[i].MouseLeftButtonDown += MouseButtonDown;
            }
        }

        private void RefreshItem()
        {
            selectedItem = new Item();
            ItemBox.Source = items[0];
        } // Empties the player's hand

        private void RefreshProp(int room, bool north, string name, BitmapImage newImage) // Edits description or image of a prop
        {
            if (north) mansion[room].North.Find((r => r.Name == name)).Sprite.Source = newImage;
            else mansion[room].South.Find((r => r.Name == name)).Sprite.Source = newImage;
        }

        private void RefreshProp(int room, bool north, string name, string newDescription)
        {
            if (north) mansion[room].North.Find((r => r.Name == name)).Description = newDescription;
            else mansion[room].South.Find((r => r.Name == name)).Description = newDescription;
        }

        private void RefreshProp(int room, bool north, string name, string newDescription, BitmapImage newImage)
        {
            Prop tempProp;
            if (north) tempProp = mansion[room].North.Find((r => r.Name == name));
            else tempProp = mansion[room].South.Find((r => r.Name == name));
            tempProp.Description = newDescription;
            tempProp.Sprite.Source = newImage;
        }

        private void AddItem(Item newItem) // Adds an item to the inventory
        {
            inventory.Add(newItem);
            selectedItem = newItem;
            ItemBox.Source = newItem.Portrait;
        }

        private void RemoveItem(Item oldItem) // Removes an item from the inventory
        {
            inventory.Remove(inventory.Find((r => r == oldItem)));
            RefreshItem();
        }

        private void Warp(int roomNumber) // Changes the room the player is in.
        {
            if (!changingRoom)
            {
                changingRoom = true;
                currentRoom = roomNumber;
                RefreshStage();
                changingRoom = false;
            }
        }

        //<timer>
        /*private void TimerTick(object sender, EventArgs e)
        {
            counter++;
        }

        private void Wait(int time)
        {
            timer.Start();
            while (counter < time) { }
            timer.Stop();
        }*/
        //</timer>

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
                    img.MouseLeftButtonUp += InventorySlot_MouseDown;
                    img.MouseLeftButtonDown += MouseButtonDown;
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
                for (int i = 0; i < 6; i++)
                {
                    tempRoom = new Room(new List<Prop>(), new List<Prop>());
                    room = File.ReadAllLines(@".\rooms\north\" + i + ".txt");
                    for (int j = 0; j < room.Length; j++)
                    {
                        tempProps = room[j].Split('>'); // Name>Description>Source>X>Y>Warp
                        tempProp = new Prop(tempProps[0], tempProps[1], props[int.Parse(tempProps[2])], new Thickness(int.Parse(tempProps[3]), int.Parse(tempProps[4]), 0, 0));
                        tempProp.Sprite.MouseEnter += Prop_MouseEnter;
                        tempProp.Sprite.MouseLeave += InventorySlot_MouseLeave;
                        tempProp.Sprite.MouseLeftButtonUp += PropPressed;
                        tempProp.Sprite.MouseLeftButtonDown += MouseButtonDown;
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
            if (downOn == sender)
            {
                if (!pressingProp)
                {
                    pressingProp = true;
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
                                        RemoveItem(itemDB[1]);
                                        PrintText("Nice! The door unlocked!");
                                        conditions[1] = true;
                                        RefreshProp(0, true, "Card Reader", "Looks like the door is unlocked now.", props[8]);
                                        RefreshProp(0, true, "Door", "This door leads to a new room.");
                                    }
                                    break;
                                case "Sofa Chair":
                                    if (!conditions[0])
                                    {
                                        AddItem(itemDB[1]);
                                        PrintText("Hey, there was a keycard under the cushion!");
                                        conditions[0] = true;
                                        RefreshProp(0, false, "Sofa Chair", "This chair looks pretty comfortable.");
                                    }
                                    break;
                                case "Fireplace":
                                    if (selectedItem.Name == "Torch")
                                    {
                                        PrintText("I might be on the right track, but if I light the torch by itself, it might burn the cloth and fall apart. Maybe if I could find something to use as fuel...");
                                    }
                                    if (selectedItem.Name == "Fuel-Soaked Torch")
                                    {
                                        RemoveItem(itemDB[3]);
                                        AddItem(itemDB[4]);
                                        PrintText("Perfect! Now I have a light!");
                                        conditions[7] = true;
                                    }
                                    break;
                                case "Door":
                                    if (conditions[1])
                                    {
                                        if (!conditions[2])
                                        {
                                            RefreshProp(0, true, "Door", "This door leads to the lobby.");
                                            conditions[2] = true;
                                        }
                                        Warp(1);
                                    }
                                    break;
                            }
                            break;
                        // </Room 1>

                        // <Room 2>
                        case 1:
                            switch (parent)
                            {
                                case "Fuel Canister":
                                    if (selectedItem.Name == "Torch")
                                    {
                                        RemoveItem(itemDB[2]);
                                        AddItem(itemDB[3]);
                                        PrintText("Nice, it worked! Now I can light it safely.");
                                        conditions[6] = true;
                                    }
                                    break;
                                case "Painting":
                                    if (selectedItem.Name == "Knife")
                                    {
                                        RemoveItem(itemDB[7]);
                                        AddItem(itemDB[10]);
                                        conditions[16] = true;
                                        if (!conditions[19])
                                        {
                                            PrintText("There was a yellow keycard hidden here! But what is it for...?");
                                            conditions[19] = true;
                                        }
                                        else
                                        {
                                            PrintText("Another yellow keycard? How many of these are there?");
                                        }
                                        RefreshProp(1, false, "Painting", "If that knife was a little sharper, that painting probably wouldn't have gotten this mangled...", props[23]);
                                    }
                                    break;
                                case "N Door":
                                    break;
                                case "E Door":
                                    if (!conditions[3])
                                    {
                                        RefreshProp(1, true, "E Door", "This door leads to the library.");
                                        conditions[3] = true;
                                    }
                                    Warp(2);
                                    break;
                                case "W Door":
                                    if (!conditions[4])
                                    {
                                        RefreshProp(1, true, "W Door", "This door leads to the mudroom.");
                                        conditions[4] = true;
                                    }
                                    Warp(4);
                                    break;
                                case "S Door":
                                    Warp(0);
                                    break;
                            }
                            break;
                        // </Room 2>

                        // <Room 3>
                        case 2:
                            switch (parent)
                            {
                                case "Filing Cabinet":
                                    if (selectedItem.Name == "Key")
                                    {
                                        RemoveItem(itemDB[5]);
                                        AddItem(itemDB[6]);
                                        PrintText("It opened! There's another keycard in here.");
                                        conditions[9] = true;
                                        RefreshProp(2, true, "Filing Cabinet", "There's nothing else in the other drawers. Seems like a waste of good storage space...");
                                    }
                                    break;
                                case "Card Reader":
                                    if (selectedItem.Name == "Blue Card")
                                    {
                                        RemoveItem(itemDB[6]);
                                        PrintText("The bookshelf just opened! I guess I found a new room...");
                                        conditions[10] = true;
                                        RefreshProp(2, false, "Bookshelf", "This opening leads to a new room...", props[17]);
                                    }
                                    break;
                                case "Bookshelf":
                                    if (conditions[10])
                                    {
                                        if (!conditions[11])
                                        {
                                            RefreshProp(2, false, "Bookshelf", "This opening leads to the control room.");
                                            conditions[11] = true;
                                        }
                                        Warp(3);
                                    }
                                    break;
                                case "Door":
                                    Warp(1);
                                    break;
                            }
                            break;
                        // </Room 3>

                        // <Room 4>
                        case 3:
                            switch (parent)
                            {
                                case "Toolbox":
                                    if (!conditions[12]) //Knife
                                    {
                                        AddItem(itemDB[7]);
                                        PrintText("A knife! I wonder what else is in there?");
                                        conditions[12] = true;
                                        RefreshProp(3, true, "Toolbox", "Looks like there's still more things in here...");
                                    }
                                    else if (!conditions[13]) //Screwdriver
                                    {
                                        AddItem(itemDB[8]);
                                        PrintText("A screwdriver? I wonder what this is for...");
                                        conditions[13] = true;
                                    }
                                    else if (!conditions[14]) //Hammer
                                    {
                                        AddItem(itemDB[9]);
                                        PrintText("That's the last thing in there.");
                                        conditions[14] = true;
                                        RefreshProp(3, true, "Toolbox", "The toolbox is empty now. Who keeps three tools in a toolbox this big...?");
                                    }
                                    break;
                                case "Card Reader A":
                                    if (!conditions[20] && selectedItem.Name == "Yellow Card")
                                    {
                                        RemoveItem(itemDB[10]);
                                        conditions[20] = true;
                                        if (conditions[20] && conditions[21] && conditions[22])
                                        {
                                            PrintText("That's the last one but I didn't see anything happen. Maybe it's not in this room?");
                                            RefreshProp(1, true, "N Door", "Hey! This door isn't locked anymore!");
                                        }
                                        else
                                            PrintText("Looks like it activated but I can't tell if anything happened...");
                                        RefreshProp(3, false, "Card Reader A", props[8]);
                                    }
                                    break;
                                case "Card Reader B":
                                    if (!conditions[21] && selectedItem.Name == "Yellow Card")
                                    {
                                        RemoveItem(itemDB[10]);
                                        conditions[21] = true;
                                        if (conditions[20] && conditions[21] && conditions[22])
                                        {
                                            PrintText("That's the last one but I didn't see anything happen. Maybe it's not in this room?");
                                            RefreshProp(1, true, "N Door", "Hey! This door isn't locked anymore!");
                                        }
                                        else
                                            PrintText("Looks like it activated but I can't tell if anything happened...");
                                        RefreshProp(3, false, "Card Reader B", props[9]);
                                    }
                                    break;
                                case "Card Reader C":
                                    if (!conditions[22] && selectedItem.Name == "Yellow Card")
                                    {
                                        RemoveItem(itemDB[10]);
                                        conditions[22] = true;
                                        if (conditions[20] && conditions[21] && conditions[22])
                                        {
                                            PrintText("That's the last one but I didn't see anything happen. Maybe it's not in this room?");
                                            RefreshProp(1, true, "N Door", "Hey! This door isn't locked anymore!");
                                        }
                                        else
                                            PrintText("Looks like it activated but I can't tell if anything happened...");
                                        RefreshProp(3, false, "Card Reader C", props[10]);
                                    }
                                    break;
                                case "Secret Entrance":
                                    Warp(2);
                                    break;
                            }
                            break;
                        // </Room 4>

                        // <Room 5>
                        case 4:
                            switch (parent)
                            {
                                case "Left Sconce":
                                    if (!conditions[5])
                                    {
                                        AddItem(itemDB[2]);
                                        PrintText("It came loose! Maybe it'll come in handy later...");
                                        conditions[5] = true;
                                        RefreshProp(4, true, "Left Sconce", "This sconce is used to hold torches. You don't see those very often anymore...", props[20]);
                                    }
                                    break;
                                case "Coat Rack":
                                    if (!conditions[8])
                                    {
                                        AddItem(itemDB[5]);
                                        PrintText("There's a key in the inside pocket. I'll take that for later...");
                                        conditions[8] = true;
                                    }
                                    break;
                                case "Dark Room":
                                    if (selectedItem.Name == "Lit Torch" || conditions[23])
                                    {
                                        if (!conditions[15])
                                        {
                                            RefreshProp(4, false, "Dark Room", "This door leads to the cellar.");
                                            conditions[15] = true;
                                        }
                                        Warp(5);
                                    }
                                    break;
                                case "Door":
                                    Warp(1);
                                    break;
                            }
                            break;
                        // </Room 5>

                        // <Room 6>
                        case 5:
                            switch (parent)
                            {
                                case "Sconce":
                                    if (selectedItem.Name == "Lit Torch")
                                    {
                                        RemoveItem(itemDB[4]);
                                        conditions[23] = true;
                                        PrintText("Alright, now it's time to search this room...");
                                        RefreshProp(5, false, "Sconce", "I guess if this is a wine cellar, it would make sense not to keep a torch burning down here. But, my vision is more important than the taste of this wine...", props[25]);
                                    }
                                    break;
                                case "Door":
                                    Warp(4);
                                    break;
                            }
                            break;
                        // </Room 6>

                        default:
                            break;
                    }
                    pressingProp = false;
                }
            }
            downOn = null;
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
            if (downOn == sender)
            {
                if (conditions[23] == false && currentRoom == 5)
                {
                    PrintText("If I put away my torch, I'll be stuck in the dark. Maybe if I can mount it somewhere...");
                }
                else if (!changingInventory)
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
            downOn = null;
        }

        private void InventorySlot_MouseDown(object sender, MouseButtonEventArgs e) // Event handler for clicking each inventory slot
        {
            if (downOn == sender)
            {
                if (!changingItemslot)
                {
                    Image root = e.Source as Image;
                    changingItemslot = true;
                    int index = invSlots.FindIndex((r => r == root));
                    if (index > inventory.Count() - 1)
                    {
                        selectedItem = new Item();
                    }
                    else
                    {
                        selectedItem = inventory[index];
                    }
                    ItemBox.Source = (selectedItem.Name == "" ? items[0] : selectedItem.Portrait); // If there is nothing selected, the item slot is empty
                    ShowHide(InventoryCanvas);
                    changingItemslot = false;
                }
            }
            downOn = null;
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
            if (downOn == sender)
            {
                if (!changingRoom)
                {
                    changingRoom = true;
                    facingNorth = !facingNorth;
                    RefreshStage();
                    changingRoom = false;
                }
            }
            downOn = null;
        }

        private void MouseButtonDown(object sender, MouseButtonEventArgs e) { downOn = sender; } // All objects that are moused down must also be moused up.
        //</events>
    }
}

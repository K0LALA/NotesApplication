using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

/**
 * Pen : f304
 * Warning : 21
 * Check : f00c
 */

namespace TODO_App.Pages
{
    public partial class Home : Page
    {
        BrushConverter bc = new BrushConverter();

        // StackPanel containing the profiles' borders
        private StackPanel profilesStackPanel;

        // Used to know if a name is being edited or not
        private bool isEditingName = false;
        // Used to stop event propogation from the profileNameState to the profile MouseLeftButtonDown
        private bool click = false;
        // Used to keep track of the old text in a TextBox
        string previousText = "";

        // Directory for application's data, all files are json files containing different profiles
        private Uri dataApplicationPath = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\NotesApplication\\", UriKind.Relative);

        public Home()
        {
            InitializeComponent();

            // Create the application's data folder if it doesn't exist
            if (!Directory.Exists(dataApplicationPath.ToString()))
                Directory.CreateDirectory(dataApplicationPath.ToString());

            profilesStackPanel = FindName("ProfilesStackPanel") as StackPanel;

            CreateProfiles();
        }

        /// <summary>
        /// Enable the click variable for 100ms
        /// </summary>
        private async void ClickEnabled()
        {
            click = true;
            await Task.Delay(100);
            click = false;
        }

        /// <summary>
        /// Returns if a name already exists in the application directory
        /// </summary>
        /// <param name="name">The name of the file to check (without .json at the end)</param>
        /// <returns>true if the name already exists, false if it doesn't</returns>
        private bool NameAlreadyExists(string name, string previous = "")
        {
            foreach (string file in Directory.GetFiles(dataApplicationPath.ToString(), "*.json"))
            {
                string fileName = file.Replace(dataApplicationPath.ToString(), "").Replace(".json", "");
                if (Equals(name, fileName) && !Equals(fileName, previous))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Create a profile and add it to the profilesStackPanel
        /// </summary>
        /// <param name="name_">Name of the profile</param>
        private void CreateProfile(string name_)
        {
            // Border holding the whole profile's components
            Border profile = new Border();
            profile.MouseLeftButtonDown += ProfileSelected;
            profile.MouseEnter += ProfileMouseEnter;
            profile.MouseLeave += ProfileMouseLeave;
            string name = "p" + (profilesStackPanel.Children.Count - 1).ToString();
            profile.Name = name;
            RegisterName(name, profile);

            // Grid to layout the components
            Grid profileGrid = new Grid();

            TextBlock profileName = new TextBlock
            {
                Text = name_.Replace(dataApplicationPath.ToString(), "").Replace(".json", "")
            };

            TextBox profileNameEditable = new TextBox
            {
                Text = profileName.Text,
                Opacity = 0.0,
                IsEnabled = false
            };
            profileNameEditable.KeyDown += ProfileKeyDown;
            profileNameEditable.TextChanged += ProfileTextChanged;

            TextBlock profileNameState = new TextBlock
            {
                Text = "\uf304",
                Opacity = 0.0
            };
            profileNameState.Style = FindResource("TextBlockStyle") as Style;
            profileNameState.MouseLeftButtonDown += ProfileNameState;

            profileGrid.Children.Add(profileName);
            profileGrid.Children.Add(profileNameEditable);
            profileGrid.Children.Add(profileNameState);

            profile.Child = profileGrid;

            profilesStackPanel.Children.Insert(profilesStackPanel.Children.Count - 1, profile);
        }

        /// <summary>
        /// Create the profiles based on the json files in the application directory
        /// </summary>
        private void CreateProfiles()
        {
            foreach (string file in Directory.GetFiles(dataApplicationPath.ToString(), "*.json"))
            {
                CreateProfile(file);
            }
        }

        /// <summary>
        /// Update the names inside the profilesStackPanel after removing one of the children
        /// </summary>
        /// <param name="childNumber">The index of the removed child</param>
        private void UpdateNames(int childNumber)
        {
            // Get the list of children
            UIElementCollection list = profilesStackPanel.Children;
            // For each element (except the last one) from the removed child's index
            for (int i = childNumber; i < list.Count - 1; i++)
            {
                // Rename the children
                int index = list.IndexOf(list[i]);
                string name = "p" + index.ToString();
                (list[i] as Border).Name = name;
                RegisterName(name, list[i]);
                UnregisterName("p" + (index + 1).ToString());
            }
        }

        /// <summary>
        /// Enable or disable all interactions for the borders
        /// </summary>
        /// <param name="enabled">Enable or disable the interactions</param>
        private void BordersEnabled(bool enabled)
        {
            foreach(Border child in profilesStackPanel.Children)
                child.IsEnabled = enabled;
        }

        /// <summary>
        /// Button to rename profiles
        /// </summary>
        /// <param name="sender">Sender of the event that's calling this function</param>
        private void UpdateState(object sender)
        {
            Grid grid = (sender as TextBlock).Parent as Grid;
            TextBlock textBlock = sender as TextBlock;
            ClickEnabled();

            if(!isEditingName)
            {
                isEditingName = true;
                textBlock.Text = "\uf00c";
                // DIsable the TextBlock and enable the TextBox
                grid.Children[0].Opacity = 0.0;
                grid.Children[0].IsEnabled = false;
                previousText = (grid.Children[0] as TextBlock).Text;
                grid.Children[1].Opacity = 1.0;
                grid.Children[1].IsEnabled = true;
                // Disable all the borders
                BordersEnabled(false);
                // Except the current one
                (grid.Parent as Border).IsEnabled = true;
            }

            else if (textBlock.Text.Equals("\uf00c"))
            {
                ClickEnabled();
                isEditingName = false;

                textBlock.Text = "\uf304";

                string newName = (grid.Children[1] as TextBox).Text;

                // Change the name of the file
                Uri oldFilePath = new Uri(dataApplicationPath.ToString() + "\\" + (grid.Children[0] as TextBlock).Text + ".json", UriKind.Relative);
                Uri newFilePath = new Uri(dataApplicationPath.ToString() + "\\" + newName + ".json", UriKind.Relative);
                File.Move(oldFilePath.ToString(), newFilePath.ToString());
                Console.WriteLine("Changed " + (grid.Children[0] as TextBlock).Text + ".json to " + newName + ".json");

                // Enable back the TextBlock and disable the TextBox
                (grid.Children[0] as TextBlock).Text = newName;
                grid.Children[0].Opacity = 1.0;
                grid.Children[0].IsEnabled = true;
                grid.Children[1].Opacity = 0.0;
                grid.Children[1].IsEnabled = false;
                // Enable back the other borders
                BordersEnabled(true);
                return;
            }

            
        }

        /// <summary>
        /// Event for each profile's border, triggered when mouse entered
        /// </summary>
        /// <param name="sender">The border the mouse is on</param>
        /// <param name="e">Arguments for the event</param>
        private void ProfileMouseEnter(object sender, MouseEventArgs e)
        {
            if(!isEditingName)
            {
                Cursor = Cursors.Hand;
                // If the Control key is pressed
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    // Make the profile red
                    (sender as Border).Background = bc.ConvertFromString("#E55934") as Brush;
                }
                else
                {
                    TextBlock profileNameState = ((sender as Border).Child as Grid).Children[2] as TextBlock;
                    DoubleAnimation opacityAnimation = new DoubleAnimation();
                    opacityAnimation.From = 0D;
                    opacityAnimation.To = 1D;
                    opacityAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(150));

                    Storyboard.SetTarget(opacityAnimation, profileNameState);
                    Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Canvas.OpacityProperty));

                    Storyboard opacityStoryboard = new Storyboard();
                    opacityStoryboard.Children.Add(opacityAnimation);

                    opacityStoryboard.Begin(profileNameState);
                }
            }
            else
                Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Event for each profile's border, triggered when mouse leaved
        /// </summary>
        /// <param name="sender">The border the mouse was on</param>
        /// <param name="e">Arguments for the event</param>
        private void ProfileMouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
            if(!isEditingName)
            {
                // Make the profile blue if it was red
                Border border = sender as Border;
                if (border.Background != bc.ConvertFromString("#0C8CE9") as Brush)
                    border.Background = bc.ConvertFromString("#0C8CE9") as Brush;

                TextBlock profileNameState = (border.Child as Grid).Children[2] as TextBlock;
                DoubleAnimation opacityAnimation = new DoubleAnimation();
                opacityAnimation.From = 1D;
                opacityAnimation.To = 0D;
                opacityAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(150));

                Storyboard.SetTarget(opacityAnimation, profileNameState);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Canvas.OpacityProperty));

                Storyboard opacityStoryboard = new Storyboard();
                opacityStoryboard.Children.Add(opacityAnimation);

                opacityStoryboard.Begin(profileNameState);
            }
        }

        /// <summary>
        /// Raised when a key is pressed inside the TextBox of a profile
        /// </summary>
        /// <param name="sender">The TextBox that sent the event</param>
        /// <param name="e">Arguments for the event</param>
        private void ProfileKeyDown(object sender, KeyEventArgs e)
        {
            // If the Enter key is pressed
            if(e.Key == Key.Enter)
            {
                // Do like the validate button
                TextBlock textBlock = ((sender as TextBox).Parent as Grid).Children[2] as TextBlock;
                UpdateState(textBlock);
                return;
            }

            // Check if the pressed key is accepted in the TextBox
            int[] acceptedChar = new [] { 18, 40, 42, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83 };
            if ((sender as TextBox).Text.Length <= 25 && acceptedChar.Contains((int)e.Key) && !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl) && !Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt))
            {
                // Accept changes
                e.Handled = false;
                return;
            }
            // Deny changes
            e.Handled = true;
        }

        /// <summary>
        /// Raised when the text of TextBox of a profile changed
        /// </summary>
        /// <param name="sender">The TextBox that raised the event</param>
        /// <param name="e">Arguments for the event</param>
        private void ProfileTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            Grid grid = textBox.Parent as Grid;
            Border parent = grid.Parent as Border;
            string newText = textBox.Text;

            // If the name already exists or is empty, make the border red and remove the check for a warning
            if (NameAlreadyExists(newText, previousText) || newText.Length == 0)
            {
                parent.Background = bc.ConvertFromString("#E55934") as Brush;
                (grid.Children[2] as TextBlock).Text = "!";
            }
            // Make the border blue and keep the check
            else
            {
                parent.Background = bc.ConvertFromString("#0C8CE9") as Brush;
                (grid.Children[2] as TextBlock).Text = "\uf00c";
            }
        }

        /// <summary>
        /// Raised when the TextBlock to rename/validate is clicked
        /// </summary>
        /// <param name="sender">The TextBlock that is clicked</param>
        /// <param name="e">Arguments for the event</param>
        private void ProfileNameState(object sender, MouseEventArgs e)
        {
            UpdateState(sender);
        }

        /// <summary>
        /// Raised when a profile is clicked
        /// </summary>
        /// <param name="sender">The clicked profile</param>
        /// <param name="e">Arguments for the event</param>
        private void ProfileSelected(object sender, MouseButtonEventArgs e)
        {
            if(!click && !isEditingName)
            {
                Border border = sender as Border;
                isEditingName = false;
                string profileName = ((border.Child as Grid).Children[0] as TextBlock).Text;

                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    // Get the index of the deleted child
                    int childNumber = profilesStackPanel.Children.IndexOf(border);
                    profilesStackPanel.Children.Remove(border);
                    // Unregister the name
                    UnregisterName(border.Name);
                    UpdateNames(childNumber);
                    Uri filePath = new Uri(dataApplicationPath + "\\" + profileName + ".json", UriKind.Relative);
                    // Delete the json file
                    File.Delete(filePath.ToString());
                    Console.WriteLine("Deleted " + profileName + ".json");
                }
                else
                {
                    // Save the profileName as an application variable, so it can be accessed by any page
                    App app = (App)Application.Current;
                    app.ActiveProfile = profileName;

                    // Change to the profile page
                    NavigationService.Navigate(new Uri("Pages/ProfilePage.xaml", UriKind.Relative));
                }
            }
        }

        /// <summary>
        /// Raised when the NewProfile button is clicked
        /// </summary>
        /// <param name="sender">The NewProfile button</param>
        /// <param name="e">Arguments for the event</param>
        private void NewProfile(object sender, MouseButtonEventArgs e)
        {
            // Name for the new profile
            string name = "New Profile";
            int attempts = 1;
            // If the name already exists, add a number at the end and check again, the number is incremented if it doesn't work
            while(NameAlreadyExists(name))
            {
                name = name.Substring(0, 11) + attempts.ToString();
                attempts++;
            }
            Uri filePath = new Uri(dataApplicationPath.ToString() + "\\" + name + ".json", UriKind.Relative);
            // Create the json file
            File.Create(filePath.ToString());
            Console.WriteLine("Created " + name + ".json");
            // Create the profile and add it the profilesStackPanel
            CreateProfile(name);
        }
    }
}

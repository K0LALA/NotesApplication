using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

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

        private StackPanel profilesStackPanel;

        private string previousText;
        private string currentText;

        private bool isEditingName = false;
        private int currentEditedName;

        // Used to stop event propogation from the profileNameState to the profile MouseLeftButtonDown
        private bool click = false;

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

        private void CreateProfiles()
        {
            foreach (string file in Directory.GetFiles(dataApplicationPath.ToString(), "*.json"))
            {
                Border profile = new Border();
                profile.MouseLeftButtonDown += ProfileSelected;
                profile.MouseEnter += ProfileMouseEnter;
                profile.MouseLeave += ProfileMouseLeave;
                string name = "p" + (profilesStackPanel.Children.Count - 1).ToString();
                profile.Name = name;
                RegisterName(name, profile);
                Grid profileGrid = new Grid();

                TextBlock profileName = new TextBlock
                {
                    Text = file.Replace(dataApplicationPath.ToString(), "").Replace(".json", "")
                };

                TextBox profileNameEditable = new TextBox
                {
                    Text = profileName.Text,
                    Opacity = 0.0
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
        }

        private void ProfileMouseEnter(object sender, MouseEventArgs e)
        {
            if(!isEditingName)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    // Make the note red
                    Border border = sender as Border;
                    border.Background = bc.ConvertFromString("#E55934") as Brush;
                }
                else
                {
                    TextBlock profileNameState = ((sender as Border).Child as Grid).Children[2] as TextBlock;
                    // TODO: Animate
                    profileNameState.Opacity = 1.0;
                }
            }
        }

        private void ProfileMouseLeave(object sender, MouseEventArgs e)
        {
            if(!isEditingName)
            {
                click = false;
                // Make the note blue if it was red
                Border border = sender as Border;
                if (border.Background != bc.ConvertFromString("#0C8CE9") as Brush)
                    border.Background = bc.ConvertFromString("#0C8CE9") as Brush;

                TextBlock profileNameState = (border.Child as Grid).Children[2] as TextBlock;
                // TODO: Animate
                profileNameState.Opacity = 0.0;
            }
        }

        private void UpdateState(object sender)
        {
            if (!isEditingName)
            {
                click = true;
                isEditingName = true;
                TextBlock textBlock = sender as TextBlock;
                Border profileBorder = (textBlock.Parent as Grid).Parent as Border;
                currentEditedName = int.Parse(profileBorder.Name.Substring(1));
                textBlock.Text = "\uf00c";
                Grid grid = textBlock.Parent as Grid;
                grid.Children[0].Opacity = 0.0;
                grid.Children[1].Opacity = 1.0;
            }
            else if ((sender as TextBlock).Text.Equals("\uf00c"))
            {
                click = true;
                isEditingName = false;

                (sender as TextBlock).Text = "\uf304";

                Grid grid = (sender as TextBlock).Parent as Grid;
                string newName = (grid.Children[1] as TextBox).Text;

                Uri oldFilePath = new Uri(dataApplicationPath.ToString() + "\\" + (grid.Children[0] as TextBlock).Text + ".json", UriKind.Relative);
                Uri newFilePath = new Uri(dataApplicationPath.ToString() + "\\" + newName + ".json", UriKind.Relative);
                File.Move(oldFilePath.ToString(), newFilePath.ToString());
                Console.WriteLine("Changed " + (grid.Children[0] as TextBlock).Text + ".json to " + newName + ".json");

                (grid.Children[0] as TextBlock).Text = newName;
                grid.Children[0].Opacity = 1.0;
                grid.Children[1].Opacity = 0.0;
            }
        }

        private void ProfileKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                TextBlock textBlock = ((sender as TextBox).Parent as Grid).Children[2] as TextBlock;
                UpdateState(textBlock);
                return;
            }

            int[] acceptedChar = new [] { 18, 40, 42, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83 };
            if ((sender as TextBox).Text.Length <= 25 && acceptedChar.Contains((int)e.Key) && !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl) && !Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt))
            {
                e.Handled = false;
                return;
            }
            e.Handled = true;
        }

        private bool NameAlreadyExists(string name)
        {
            foreach (string file in Directory.GetFiles(dataApplicationPath.ToString(), "*.json"))
            {
                string fileName = file.Replace(dataApplicationPath.ToString(), "").Replace(".json", "");
                if (Equals(name, fileName))
                    return true;
            }

            return false;
        }

        private void ProfileTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            Grid grid = textBox.Parent as Grid;
            Border parent = grid.Parent as Border;
            string newText = textBox.Text;

            if (NameAlreadyExists(newText) || newText.Length == 0)
            {
                parent.Background = bc.ConvertFromString("#E55934") as Brush;
                (grid.Children[2] as TextBlock).Text = "!";
            }
            else
            {
                parent.Background = bc.ConvertFromString("#0C8CE9") as Brush;
                (grid.Children[2] as TextBlock).Text = "\uf00c";
            }
        }

        private void ProfileNameState(object sender, MouseEventArgs e)
        {
            UpdateState(sender);
        }

        private void ProfileSelected(object sender, MouseButtonEventArgs e)
        {
            if(!click)
            {
                Border border = sender as Border;
                isEditingName = false;

                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    // Delete the note
                    UnregisterName(border.Name);
                }
                else
                {
                    string profileName = ((border.Child as Grid).Children[0] as TextBlock).Text;

                    // Give the profileName to the frame navigation for the profilePage
                    App app = (App)Application.Current;
                    app.ActiveProfile = profileName;

                    NavigationService.Navigate(new Uri("Pages/ProfilePage.xaml", UriKind.Relative));
                }
            }
        }
    }
}

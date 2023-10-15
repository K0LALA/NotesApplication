using System;
using System.IO;
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
                profile.MouseEnter += ProfileHover;
                Grid profileGrid = new Grid();

                TextBox profileName = new TextBox();
                profileName.Text = file.Replace(dataApplicationPath.ToString(), "").Replace(".json", "");

                TextBlock profileNameState = new TextBlock();
                profileNameState.Text = "\uf304";
                profileNameState.Opacity = 0.0;

                profileGrid.Children.Add(profileName);
                profileGrid.Children.Add(profileNameState);

                profile.Child = profileGrid;

                profilesStackPanel.Children.Insert(profilesStackPanel.Children.Count - 1, profile);
            }
        }

        private void ProfileHover(object sender, MouseEventArgs e)
        {
            TextBlock profileNameState = ((sender as Border).Child as Grid).Children[1] as TextBlock;
            // TODO: Animate
            profileNameState.Opacity = 1.0;
        }

        /*private void ProfileKeyDown(object sender, KeyEventArgs e)
        {
            // Capture the previous text before any changes are made
            previousText = (sender as TextBox).Text;
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
            Regex reg = new Regex("[^a-zA-Z0-9 _-]");

            TextBox textBox = sender as TextBox;
            Border parent = textBox.Parent as Border;
            string newText = textBox.Text;

            Console.WriteLine(newText);

            if(NameAlreadyExists(newText))
            {
                // Make the border's background red
                parent.Background = bc.ConvertFromString("#E55934") as Brush;

                // Add an icon to show there's an error


                return;
            }

            if(!reg.IsMatch(newText))
            {
                // Set the border's background to red
                parent.Background = bc.ConvertFromString("#0C8CE9") as Brush;
                // Remove the icon

                Uri oldFilePath = new Uri(dataApplicationPath.ToString() + "\\" + previousText + ".json", UriKind.Relative);
                Uri newFilePath = new Uri(dataApplicationPath.ToString() + "\\" + newText + ".json", UriKind.Relative);
                File.Move(oldFilePath.ToString(), newFilePath.ToString());
                Console.WriteLine("Changed " + previousText + ".json to " + newText + ".json");
            }
            else
            {
                textBox.Text = previousText;
            }
        }*/

        private void ProfileSelected(object sender, MouseButtonEventArgs e)
        {
            string profileName = ((sender as Border).Child as TextBox).Text;

            // Give the profileName to the frame navigation for the profilePage
            App app = (App)Application.Current;
            app.ActiveProfile = profileName;

            NavigationService.Navigate(new Uri("Pages/ProfilePage.xaml", UriKind.Relative));
        }
    }
}

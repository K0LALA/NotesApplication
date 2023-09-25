using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace TODO_App
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private MainWindow mainWindow;
        private ScrollViewer tabsScrollViewer;
        private StackPanel tabsStackPanel;
        private TextBox createTextBox;
        private ScrollViewer scrollViewer;
        private StackPanel notesStackPanel;
        private Grid mainGrid;

        private BrushConverter bc = new BrushConverter();
        // Directory for application's data, all files are json files containing different profiles
        private Uri dataApplicationPath = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\NotesApplication\\");

        // Shortcut commands
        public static RoutedCommand NewTabCommand = new RoutedCommand();

        /// <summary>
        /// Constructor of MainWindow class
        /// </summary>
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();

            if (!System.IO.Directory.Exists(dataApplicationPath.ToString()))
                System.IO.Directory.CreateDirectory(dataApplicationPath.ToString());

            mainWindow = GetWindow(this) as MainWindow;
            mainWindow.MouseWheel += GlobalMouseWheelEvent;
            tabsScrollViewer = FindName("TabsScrollViewer") as ScrollViewer;
            tabsScrollViewer.PreviewMouseWheel += TabsMouseWheelEvent;
            tabsStackPanel = FindName("TabsStackPanel") as StackPanel;
            createTextBox = FindName("CreateTextBox") as TextBox;
            scrollViewer = FindName("NotesScrollViewer") as ScrollViewer;
            notesStackPanel = FindName("NotesStackPanel") as StackPanel;
            mainGrid = FindName("MainGrid") as Grid;

            RegisterCommands();
        }

        /// <summary>
        /// Register a command
        /// </summary>
        /// <param name="command">The command to register</param>
        /// <param name="keyGesture">Set of keys to call the command</param>
        /// <param name="eventHandler">Function to call when the command is triggered</param>
        private void RegisterCommand(RoutedCommand command, KeyGesture keyGesture, ExecutedRoutedEventHandler eventHandler)
        {
            command.InputGestures.Add(keyGesture);
            CommandBinding binding = new CommandBinding();
            binding.Command = command;
            binding.Executed += eventHandler;
            mainWindow.CommandBindings.Add(binding);
        }

        /// <summary>
        /// Register for all commands
        /// </summary>
        private void RegisterCommands()
        {
            RegisterCommand(NewTabCommand, new KeyGesture(Key.T, ModifierKeys.Control), NewTabExecuted);
        }


        // Data Binding
        // TODO: Change this because of the multiple tabs

        private string _tabTitle = "Title";
        public string TabTitle
        {
            get { return _tabTitle; }
            set 
            { 
                if(_tabTitle != value)
                {
                    _tabTitle = value;
                    OnPropertyChanged();
                }
            }
        }
        
        // TODO: get on the tabs.json file
        private bool _anyNote = true;
        public bool AnyNote
        {
            get { return _anyNote; }
            set
            {
                _anyNote = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Detect in the whoole window if the scroll wheel is used
        /// Now used to scroll the notes no matter where is the cursor
        /// </summary>
        private void GlobalMouseWheelEvent(object sender, MouseWheelEventArgs e)
        {
            if (mainGrid.IsMouseOver)
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 2.5);
        }

        /// <summary>
        /// Creates a note with specified arguments
        /// </summary>
        /// <param name="name">Name of the notes</param>
        /// <returns>The <code>Border</code> containing the note</returns>
        private Border CreateNote(String name)
        {
            Border mainBorder = new Border();
            mainBorder.PreviewMouseLeftButtonDown += Border_PreviewMouseLeftButtonDown;

            Grid mainGrid = new Grid();
            ColumnDefinition draggerColumn = new ColumnDefinition();
            draggerColumn.Width = new GridLength(34.0);
            ColumnDefinition nameColumn = new ColumnDefinition();
            nameColumn.Width = new GridLength(450);
            ColumnDefinition historyColumn = new ColumnDefinition();
            historyColumn.Width = new GridLength(1, GridUnitType.Star);
            mainGrid.ColumnDefinitions.Add(draggerColumn);
            mainGrid.ColumnDefinitions.Add(nameColumn);
            mainGrid.ColumnDefinitions.Add(historyColumn);

            // Dragger
            Border draggerBorder = new Border();
            Style draggerStyle = this.FindResource("Dragger") as Style;
            draggerBorder.Style = draggerStyle;

            Canvas draggerCanvas = new Canvas();
            draggerCanvas.Width = 22;
            draggerCanvas.Height = 40;

            Style draggerEllipsesStyle = new Style();
            Setter draggerEllipsesWidth = new Setter(WidthProperty, 10);
            Setter draggerEllipsesHeight = new Setter(HeightProperty, 10);

            bool marginFirst = false;
            int marginSecond = 0;
            for (int i = 0; i < 6; i++)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = 10;
                ellipse.Height = 10;
                ellipse.Fill = (Brush)bc.ConvertFromString("#DDDDDD");

                ellipse.Margin = new Thickness(marginFirst ? 12 : 0, marginSecond, 0, 0);

                marginFirst = !marginFirst;
                if (!marginFirst)
                    marginSecond += 15;

                draggerCanvas.Children.Add(ellipse);
            }

            draggerBorder.Child = draggerCanvas;

            // Name
            TextBox noteName = new TextBox();
            noteName.KeyDown += NoteNameChanged;
            noteName.Text = name;

            // History
            Grid historyGrid = new Grid();
            historyGrid.Height = 91;
            historyGrid.Margin = new Thickness(5, 0, 15, 0);
            historyGrid.VerticalAlignment = VerticalAlignment.Center;
            ColumnDefinition iconColumn = new ColumnDefinition();
            iconColumn.Width = new GridLength(1, GridUnitType.Auto);
            ColumnDefinition dateColumn = new ColumnDefinition();
            dateColumn.Width = new GridLength(1, GridUnitType.Star);
            historyGrid.ColumnDefinitions.Add(iconColumn);
            historyGrid.ColumnDefinitions.Add(dateColumn);

            Canvas createLogoCanvas = new Canvas();
            createLogoCanvas.Width = 25;
            createLogoCanvas.Height = 25;
            createLogoCanvas.Margin = new Thickness(4);
            createLogoCanvas.VerticalAlignment = VerticalAlignment.Center;

            Path createLogo = new Path();
            createLogo.Style = this.FindResource("CreateIconPath") as Style;
            createLogo.Fill = (Brush)bc.ConvertFromString("#0C8CE9");
            createLogoCanvas.Children.Add(createLogo);

            TextBlock createDate = new TextBlock();
            createDate.Style = this.FindResource("HistoryTextBlock") as Style;
            createDate.VerticalAlignment = VerticalAlignment.Center;
            String dateTime = DateTime.Now.ToString("dd/MM/yyyy - HH:mm");
            createDate.Text = dateTime;

            historyGrid.Children.Add(createLogoCanvas);
            Grid.SetColumn(createLogoCanvas, 0);
            historyGrid.Children.Add(createDate);

            // Add three elements to main grid
            mainGrid.Children.Add(draggerBorder);
            Grid.SetColumn(draggerBorder, 0);
            mainGrid.Children.Add(noteName);
            Grid.SetColumn(noteName, 1);
            mainGrid.Children.Add(historyGrid);
            Grid.SetColumn(historyGrid, 2);


            mainBorder.Child = mainGrid;
            return mainBorder;
        }

        /// <summary>
        /// Redirect the focus to the main grid
        /// </summary>
        private void LoseFocus(object sender, MouseButtonEventArgs e)
        {
            (sender as Grid).Focus();
        }

        /// <summary>
        /// Lose focus when enter key is pressed and the createTextBox is focused
        /// </summary>
        private void LoseFocusEnterKey(object sender, KeyEventArgs e)
        {
            if ((!(createTextBox.IsFocused || createTextBox.IsKeyboardFocused)
                && e.Key == Key.Enter) ||
                sender == createTextBox)
                (mainGrid.Parent as Grid).Focus();
        }

        /// <summary>
        /// Scroll hozirontally through the tabs
        /// </summary>
        private void TabsMouseWheelEvent(object sender, MouseWheelEventArgs e)
        {
            tabsScrollViewer.ScrollToHorizontalOffset(tabsScrollViewer.HorizontalOffset - e.Delta / 50);
        }

        /// <summary>
        /// Add the note when we press enter in the createTextBox
        /// </summary>
        private void CreateTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (!string.IsNullOrEmpty(textBox.Text) && e.Key == Key.Enter)
            {
                // TODO: check if we need to add on the top or on the bottom
                // TODO: check if we need to lose focus
                notesStackPanel.Children.Add(CreateNote(textBox.Text));
                AnyNote = false;
                Console.WriteLine("Element '{0}' added to notesStackPanel", textBox.Text);
                textBox.Clear();
                LoseFocusEnterKey(sender, e);
            }
        }

        /// <summary>
        /// Event called when the name of a note is changed
        /// </summary>
        private void NoteNameChanged(object sender, KeyEventArgs e)
        {
            Grid historyGrid = ((sender as TextBox).Parent as Grid).Children[2] as Grid;
            if(historyGrid.Children.Count < 3)
            {
                (historyGrid.Children[0] as Canvas).VerticalAlignment = VerticalAlignment.Top;
                (historyGrid.Children[1] as TextBlock).VerticalAlignment = VerticalAlignment.Top;

                Canvas modifyLogoCanvas = new Canvas();
                modifyLogoCanvas.Width = 25;
                modifyLogoCanvas.Height = 25;
                modifyLogoCanvas.Margin = new Thickness(4);
                modifyLogoCanvas.VerticalAlignment = VerticalAlignment.Bottom;

                Path modifyLogo = new Path();
                modifyLogo.Style = this.FindResource("ModifyIconPath") as Style;
                modifyLogo.Fill = (Brush)bc.ConvertFromString("#0C8CE9");
                modifyLogoCanvas.Children.Add(modifyLogo);

                TextBlock modifyDate = new TextBlock();
                modifyDate.Style = this.FindResource("HistoryTextBlock") as Style;
                modifyDate.VerticalAlignment = VerticalAlignment.Bottom;
                String dateTime = DateTime.Now.ToString("dd/MM/yyyy - HH:mm");
                modifyDate.Text = dateTime;

                historyGrid.Children.Add(modifyLogoCanvas);
                Grid.SetColumn(modifyLogoCanvas, 0);
                historyGrid.Children.Add(modifyDate);
            }
            else
            {
                (historyGrid.Children[3] as TextBlock).Text = DateTime.Now.ToString("dd/MM/yyyy - HH:mm");
            }
        }

        /// <summary>
        /// Button to open a new tab
        /// </summary>
        private void NewTabButton(object sender, MouseButtonEventArgs e)
        {
            NewTab();
        }

        /// <summary>
        /// Command to open a new tab via the shortcut
        /// </summary>
        private void NewTabExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            NewTab();
        }

        /// <summary>
        /// Create a new tab, is called by <code>NewTabButton</code> and <code>NewTabExecuted</code>
        /// </summary>
        private void NewTab()
        {
            Border tab = new Border();
            tab.Style = this.FindResource("TabStyle") as Style;

            Grid tabGrid = new Grid();
            ColumnDefinition tabGridName = new ColumnDefinition();
            tabGridName.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition tabGridCloseIcon = new ColumnDefinition();
            tabGridCloseIcon.Width = new GridLength(1, GridUnitType.Auto);
            tabGrid.ColumnDefinitions.Add(tabGridName);
            tabGrid.ColumnDefinitions.Add(tabGridCloseIcon);

            tab.Name = "t" + (tabsStackPanel.Children.Count - 1).ToString();
            this.RegisterName(tab.Name, tab);

            TextBox tabName = new TextBox();
            tabName.Text = "Title";
            tabName.Foreground = bc.ConvertFromString("#0C8CE9") as Brush;
            tabName.CaretBrush = bc.ConvertFromString("#0C8CE9") as Brush;
            tabGrid.Children.Add(tabName);

            Canvas closeIconCanvas = new Canvas();
            closeIconCanvas.Name = tab.Name + "icon";
            this.RegisterName(tab.Name + "icon", closeIconCanvas);
            closeIconCanvas.Width = 25;
            closeIconCanvas.Height = 25;
            closeIconCanvas.Cursor = Cursors.Hand;
            closeIconCanvas.HorizontalAlignment = HorizontalAlignment.Center;
            closeIconCanvas.Margin = new Thickness(10, 0, 10, 0);
            closeIconCanvas.Background = Brushes.Transparent;
            closeIconCanvas.MouseLeftButtonDown += CloseTabButton;

            Path closeIconPath = new Path();
            closeIconPath.Style = FindResource("CloseIconPath") as Style;
            closeIconPath.Fill = bc.ConvertFromString("#0C8CE9") as Brush;

            closeIconCanvas.Children.Add(closeIconPath);
            tabGrid.Children.Add(closeIconCanvas);
            Grid.SetColumn(closeIconCanvas, 1);

            tab.MouseEnter += new MouseEventHandler(delegate (Object o, MouseEventArgs a)
            {
                DoubleAnimation opacityAnimation = new DoubleAnimation();
                opacityAnimation.From = 0D;
                opacityAnimation.To = 1D;
                opacityAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(150));

                Storyboard.SetTargetName(opacityAnimation, closeIconCanvas.Name);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Canvas.OpacityProperty));

                Storyboard opacityStoryboard = new Storyboard();
                opacityStoryboard.Children.Add(opacityAnimation);

                opacityStoryboard.Begin(closeIconCanvas);
            });

            tab.MouseLeave += new MouseEventHandler(delegate (Object o, MouseEventArgs a)
            {
                DoubleAnimation opacityAnimation = new DoubleAnimation();
                opacityAnimation.From = 1D;
                opacityAnimation.To = 0D;
                opacityAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(150));

                Storyboard.SetTargetName(opacityAnimation, closeIconCanvas.Name);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Canvas.OpacityProperty));

                Storyboard opacityStoryboard = new Storyboard();
                opacityStoryboard.Children.Add(opacityAnimation);

                opacityStoryboard.Begin(closeIconCanvas);
            });

            tab.Child = tabGrid;
            tabsStackPanel.Children.Insert(tabsStackPanel.Children.Count - 1, tab);

            // Open Animation
            DoubleAnimation tabCreatedAnimation = new DoubleAnimation();
            tabCreatedAnimation.From = 0;
            tabCreatedAnimation.To = 370;
            tabCreatedAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(100));

            Storyboard.SetTargetName(tabCreatedAnimation, tab.Name);
            Storyboard.SetTargetProperty(tabCreatedAnimation, new PropertyPath(Border.WidthProperty));

            Storyboard tabCreatedStoryboard = new Storyboard();
            tabCreatedStoryboard.Children.Add(tabCreatedAnimation);

            // TODO: Add to the json file


            tabCreatedStoryboard.Begin(tab);
        }

        /// <summary>
        /// Button to close a tab
        /// </summary>
        private void CloseTabButton(object sender, MouseButtonEventArgs e)
        {
            Border tab = ((sender as Canvas).Parent as Grid).Parent as Border;
            StackPanel tabs = tab.Parent as StackPanel;

            // TODO: Remove from the json file

            // Close Animation
            DoubleAnimation tabDeletedAnimation = new DoubleAnimation();
            tabDeletedAnimation.From = 370;
            tabDeletedAnimation.To = 0;
            tabDeletedAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(100));

            Storyboard.SetTargetName(tabDeletedAnimation, tab.Name);
            Storyboard.SetTargetProperty(tabDeletedAnimation, new PropertyPath(Border.WidthProperty));

            Storyboard tabDeletedStoryboard = new Storyboard();
            tabDeletedStoryboard.Children.Add(tabDeletedAnimation);
            tabDeletedStoryboard.Completed += new EventHandler(delegate (Object o, EventArgs a)
            {
                int childNumber = int.Parse(tab.Name.Substring(1));
                tabs.Children.RemoveAt(childNumber);
                this.UnregisterName(tab.Name);
                this.UnregisterName(tab.Name + "icon");
            });
            tabDeletedStoryboard.Begin(tab);
        }

        /// <summary>
        /// Function to drag and drop the notes, WIP
        /// </summary>
        private void Border_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/adorners-overview?view=netframeworkdesktop-4.8&redirectedfrom=MSDN
            // TODO: Drag the border and not the window so `this.DragMove();` doesn't work
            Console.WriteLine("Border should be dragged");
        }
    }
}

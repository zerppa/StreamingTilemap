namespace Editor
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// The picker dependency property.
        /// </summary>
        public static readonly DependencyProperty PickerProperty = DependencyProperty.Register(
            nameof(Picker),
            typeof(Rect),
            typeof(MainWindow),
            new PropertyMetadata(default(Rect)));

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            this.Loaded += (sender, args) => this.ShowToolWindows();

            this.DataContext = new MainViewModel(loadMap: !DesignerProperties.GetIsInDesignMode(this));
        }

        /// <summary>
        /// Gets or sets the picker.
        /// </summary>
        public Rect Picker
        {
            get => (Rect)this.GetValue(PickerProperty);
            set => this.SetValue(PickerProperty, value);
        }

        /// <summary>
        /// Displays the map browser and tileset tool windows.
        /// </summary>
        private async void ShowToolWindows()
        {
            this.UpdateLayout();
            await Task.Delay(100);

            var mapBrowserWindow = new MapBrowserWindow();
            mapBrowserWindow.Show();
            mapBrowserWindow.AsToolWindow();

            var tilesetWindow = new TilesetWindow();
            tilesetWindow.Show();
            tilesetWindow.AsToolWindow();
        }

        /// <summary>
        /// Called when the <see cref="System.Windows.UIElement.MouseDown"/> event is raised.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnChunkMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                this.HandlePaint(element, e.GetPosition(element), e.LeftButton == MouseButtonState.Pressed, e.RightButton == MouseButtonState.Pressed);
            }
        }

        /// <summary>
        /// Called when the <see cref="System.Windows.UIElement.MouseMove"/> event is raised.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnChunkMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                this.HandlePaint(element, e.GetPosition(element), e.LeftButton == MouseButtonState.Pressed, e.RightButton == MouseButtonState.Pressed);
            }
        }

        /// <summary>
        /// Updates the viewmodel based on mouse painting.
        /// </summary>
        /// <param name="element">The element that sent the event.</param>
        /// <param name="relativePosition">The relative mouse position.</param>
        /// <param name="leftButton"><c>true</c> is the left mouse button is down.</param>
        /// <param name="rightButton"><c>true</c> is the right mouse button is down.</param>
        private void HandlePaint(FrameworkElement element, Point relativePosition, bool leftButton, bool rightButton)
        {
            if (!(element?.DataContext is ChunkViewModel viewmodel))
            {
                return;
            }

            var location = (x: (int)relativePosition.X / Configuration.TileSize, y: (int)relativePosition.Y / Configuration.TileSize);

            if (leftButton)
            {
                (this.DataContext as MainViewModel)?.OnPaint(viewmodel, location);
            }
            else if (rightButton)
            {
                (this.DataContext as MainViewModel)?.OnErase(viewmodel, location);
            }
        }

        /// <summary>
        /// Called when the <see cref="System.Windows.UIElement.MouseMove"/> event is raised.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnContainerMouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition((UIElement)sender);
            var (x, y) = ((int)pos.X / Configuration.TileSize, (int)pos.Y / Configuration.TileSize);

            this.Picker = new Rect(
                new Point(x * Configuration.TileSize, y * Configuration.TileSize),
                new Size(Configuration.TileSize, Configuration.TileSize));
        }
    }
}

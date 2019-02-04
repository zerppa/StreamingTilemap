namespace Editor
{
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media.Animation;

    /// <summary>
    /// Interaction logic for TilesetWindow.xaml.
    /// </summary>
    public partial class TilesetWindow
    {
        /// <summary>
        /// The picker dependency property.
        /// </summary>
        public static readonly DependencyProperty PickerProperty = DependencyProperty.Register(
            nameof(Picker),
            typeof(Rect),
            typeof(TilesetWindow),
            new PropertyMetadata(default(Rect)));

        /// <summary>
        /// Initializes a new instance of the <see cref="TilesetWindow"/> class.
        /// </summary>
        public TilesetWindow()
        {
            this.InitializeComponent();

            this.SizeChanged += this.OnSizeChanged;

            this.DataContext = new TilesetViewModel();
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
        /// Called when the <see cref="FrameworkElement.SizeChanged"/> event is raised.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var mainWindow = Application.Current?.MainWindow;
            if (mainWindow != null)
            {
                this.Top = mainWindow.Top;
                this.Left = mainWindow.Left + mainWindow.ActualWidth + 10;
            }
        }

        /// <summary>
        /// Called when the <see cref="System.Windows.UIElement.MouseDown"/> event is raised.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is FrameworkElement element))
            {
                return;
            }

            var pos = e.GetPosition(element);
            var (x, y) = ((int)pos.X / Configuration.TileSize, (int)pos.Y / Configuration.TileSize);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                (this.DataContext as TilesetViewModel)?.OnPickFore(x, y);
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                (this.DataContext as TilesetViewModel)?.OnPickBack(x, y);
            }

            this.UpdateVisuals(pos);
        }

        /// <summary>
        /// Called when the <see cref="System.Windows.UIElement.MouseMove"/> event is raised.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                this.UpdateVisuals(e.GetPosition(element));
            }
        }

        /// <summary>
        /// Updates the picker visuals.
        /// </summary>
        /// <param name="relativePosition">The relative mouse position.</param>
        private void UpdateVisuals(Point relativePosition)
        {
            var (x, y) = ((int)relativePosition.X / Configuration.TileSize, (int)relativePosition.Y / Configuration.TileSize);

            this.Picker = new Rect(
                new Point(x * Configuration.TileSize, y * Configuration.TileSize),
                new Size(Configuration.TileSize, Configuration.TileSize));

            if (this.FindResource("PlayAnimation") is Storyboard storyboard)
            {
                Storyboard.SetTarget(storyboard, this.PickerRectangle);
                storyboard.Begin();
            }
        }
    }
}

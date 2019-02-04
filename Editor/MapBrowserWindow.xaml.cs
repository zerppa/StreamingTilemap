namespace Editor
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for MapBrowserWindow.xaml.
    /// </summary>
    public partial class MapBrowserWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapBrowserWindow"/> class.
        /// </summary>
        public MapBrowserWindow()
        {
            this.DataContextChanged += this.OnDataContextChanged;

            this.InitializeComponent();

            this.SizeChanged += this.OnSizeChanged;

            this.DataContext = new MapBrowserViewModel();
        }

        /// <summary>
        /// Called when the <see cref="FrameworkElement.DataContextChanged"/> event is raised.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Since the view model subscribes to some events, make sure to provide lifetime methods for proper unsubscribing (to prevent memory leaks)
            (e.OldValue as MapBrowserViewModel)?.OnDetached();
            (e.NewValue as MapBrowserViewModel)?.OnAttached();
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
                this.Left = mainWindow.Left - this.ActualWidth - 10;
            }
        }
    }
}

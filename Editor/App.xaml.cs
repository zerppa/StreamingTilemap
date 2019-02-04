namespace Editor
{
    using System.IO;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App
    {
        /// <inheritdoc />
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ensure that the default map is empty at the start
            Directory.CreateDirectory($@"{Configuration.MapsFolder}\{Configuration.DefaultMapName}");
            new DirectoryInfo($@"{Configuration.MapsFolder}\{Configuration.DefaultMapName}").EnumerateFiles().ForEach(file => file.Delete());
        }
    }
}

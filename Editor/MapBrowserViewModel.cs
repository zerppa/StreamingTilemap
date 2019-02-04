namespace Editor
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// View model for the map browser window.
    /// </summary>
    /// <seealso cref="Editor.ViewModel" />
    public class MapBrowserViewModel : ViewModel
    {
        /// <summary>
        /// Stores the characters that are forbidden in a map name.
        /// </summary>
        private static readonly char[] InvalidNameCharacters = Path.GetInvalidPathChars().Union(Path.GetInvalidPathChars()).ToArray();

        /// <summary>
        /// Stores the file system watcher for map folders.
        /// </summary>
        private readonly FileSystemWatcher fileSystemWatcher;

        /// <summary>
        /// The backend field for the property.
        /// </summary>
        private WriteableBitmap preview;

        /// <summary>
        /// The backend field for the property.
        /// </summary>
        private MapViewModel currentMap;

        /// <summary>
        /// The backend field for the property.
        /// </summary>
        private string newMapName;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapBrowserViewModel"/> class.
        /// </summary>
        public MapBrowserViewModel()
        {
            this.Preview = new WriteableBitmap(
                1,
                1,
                96,
                96,
                PixelFormats.Bgr32,
                null);

            this.fileSystemWatcher = new FileSystemWatcher { Path = Configuration.MapsFolder };

            this.CreateNewMapCommand = new Command<string>(this.CreateNewMap, this.IsValidNewMapName);

            this.Refresh();
        }

        /// <summary>
        /// Gets the create new map command.
        /// </summary>
        public ICommand CreateNewMapCommand { get; }

        /// <summary>
        /// Gets the maps.
        /// </summary>
        public ObservableCollection<MapViewModel> Maps { get; } = new ObservableCollection<MapViewModel>();

        /// <summary>
        /// Gets the preview image.
        /// </summary>
        public WriteableBitmap Preview
        {
            get => this.preview;
            private set => this.SetValue(ref this.preview, value);
        }

        /// <summary>
        /// Gets or sets the current map.
        /// </summary>
        public MapViewModel CurrentMap
        {
            get
            {
                return this.currentMap;
            }

            set
            {
                this.SetValue(ref this.currentMap, value);
                this.Context.CurrentMap = value?.Name;
                this.RefreshPreview(value?.Name);
            }
        }

        /// <summary>
        /// Gets or sets the new map name.
        /// </summary>
        public string NewMapName
        {
            get => this.newMapName;
            set => this.SetValue(ref this.newMapName, value);
        }

        /// <summary>
        /// Removes the specified <see cref="MapViewModel"/> from the map list.
        /// </summary>
        /// <param name="item">The item.</param>
        internal void DeleteMap(MapViewModel item)
        {
            if (item != null && this.Maps.Contains(item) && ConfirmDelete())
            {
                item.DeleteChunks();
                Directory.Delete(item.FullName);
            }

            bool ConfirmDelete() =>
                MessageBox.Show(
                    $"Permanently delete map \"{item.Name}\"?",
                    "Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation,
                    MessageBoxResult.No) == MessageBoxResult.Yes;
        }

        /// <summary>
        /// Called when the view model is attached and should subscribe to events.
        /// </summary>
        internal void OnAttached()
        {
            this.fileSystemWatcher.Created += this.OnFolderCreated;
            this.fileSystemWatcher.Renamed += this.OnFolderRenamed;
            this.fileSystemWatcher.Deleted += this.OnFolderDeleted;
            this.fileSystemWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Called when the view model is detached and should unsubscribe from events.
        /// </summary>
        internal void OnDetached()
        {
            this.fileSystemWatcher.EnableRaisingEvents = false;
            this.fileSystemWatcher.Created -= this.OnFolderCreated;
            this.fileSystemWatcher.Renamed -= this.OnFolderRenamed;
            this.fileSystemWatcher.Deleted -= this.OnFolderDeleted;
        }

        /// <summary>
        /// Called when the file system watcher detects a created entry.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FileSystemEventArgs"/> instance containing the event data.</param>
        private void OnFolderCreated(object sender, FileSystemEventArgs e) => Application.Current.Dispatcher.Invoke(this.Refresh);

        /// <summary>
        /// Called when the file system watcher detects a renamed entry.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FileSystemEventArgs"/> instance containing the event data.</param>
        private void OnFolderRenamed(object sender, FileSystemEventArgs e) => Application.Current.Dispatcher.Invoke(this.Refresh);

        /// <summary>
        /// Called when the file system watcher detects a deleted entry.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FileSystemEventArgs"/> instance containing the event data.</param>
        private void OnFolderDeleted(object sender, FileSystemEventArgs e) => Application.Current.Dispatcher.Invoke(this.Refresh);

        /// <summary>
        /// Adds the specified map.
        /// </summary>
        /// <param name="name">The name.</param>
        private void CreateNewMap(string name)
        {
            Debug.Assert(this.IsValidNewMapName(name), "The map name is invalid.");

            Directory.CreateDirectory(Path.Combine(Configuration.MapsFolder, name));

            this.NewMapName = string.Empty;
        }

        /// <summary>
        /// Determines whether the specified name is suitable for a map.
        /// </summary>
        /// <param name="name">The map name candidate.</param>
        /// <returns>
        /// <c>true</c> if name is valid; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValidNewMapName(string name)
        {
            return !string.IsNullOrWhiteSpace(name)
                && !this.Maps.Any(map => string.Equals(name, map.Name, StringComparison.InvariantCultureIgnoreCase))
                && !InvalidNameCharacters.Any(name.Contains);
        }

        /// <summary>
        /// Refreshes the map list.
        /// </summary>
        private void Refresh()
        {
            var previousSelection = this.CurrentMap;

            this.Maps.Clear();

            this.Maps.AddRange(
                new DirectoryInfo(Configuration.MapsFolder)
                    .EnumerateDirectories()
                    .OrderByDescending(folder => string.Equals(folder.Name, Configuration.DefaultMapName, StringComparison.InvariantCultureIgnoreCase))
                    .ThenBy(folder => folder.Name)
                    .Select(folder => new MapViewModel(this, folder)));

            this.CurrentMap = this.Maps.FirstOrDefault(map => string.Equals(map.Name, previousSelection?.Name, StringComparison.InvariantCultureIgnoreCase))
                ?? this.Maps.FirstOrDefault(map => string.Equals(map.Name, Configuration.DefaultMapName, StringComparison.InvariantCultureIgnoreCase))
                ?? this.Maps.FirstOrDefault();
        }

        /// <summary>
        /// Refreshes the preview image for the specified map.
        /// </summary>
        /// <param name="mapName">Name of the map.</param>
        private void RefreshPreview(string mapName)
        {
            if (string.IsNullOrWhiteSpace(mapName))
            {
                Clear();
                return;
            }

            var chunkFiles = Directory.GetFiles(Path.Combine(Configuration.MapsFolder, mapName), "*.chunk").Select(Path.GetFileName);
            var coordinateTuples = chunkFiles.Select(Parse).Where(coordinates => coordinates != null).Cast<(int x, int y)>().ToArray();

            if (coordinateTuples.Any())
            {
                var minX = coordinateTuples.Min(t => t.x);
                var maxX = coordinateTuples.Max(t => t.x);
                var minY = coordinateTuples.Min(t => t.y);
                var maxY = coordinateTuples.Max(t => t.y);

                var width = maxX - minX + 1;
                var height = maxY - minY + 1;

                Debug.WriteLine($"x:({minX} → {maxX}), y:({minY} → {maxY}), size:({width}, {height})");

                var bitmap = this.Preview.Resize(width, height, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
                this.Preview = bitmap;
                bitmap.Clear(Colors.Black);

                if (bitmap.TryLock(Duration.Forever))
                {
                    unsafe
                    {
                        var pBackBuffer = bitmap.BackBuffer;
                        var pBuff = (byte*)pBackBuffer.ToPointer();
                        var stride = bitmap.BackBufferStride;

                        foreach (var coordinateTuple in coordinateTuples)
                        {
                            Plot(coordinateTuple.x, coordinateTuple.y, Colors.White);
                        }

                        Plot(0, 0, Colors.Red);

                        void Plot(int x, int y, Color color)
                        {
                            var index = (y - minY) * stride + (x - minX) * 4;
                            pBuff[index] = color.B;
                            pBuff[index + 1] = color.G;
                            pBuff[index + 2] = color.R;
                            pBuff[index + 3] = 255;
                        }
                    }

                    bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
                    bitmap.Unlock();
                }
            }
            else
            {
                Clear();
            }

            (int x, int y)? Parse(string name)
            {
                var parts = name.Split(',', '.');
                if (parts.Length != 3 || !int.TryParse(parts[0], out var x) || !int.TryParse(parts[1], out var y))
                {
                    return null;
                }

                return (x, y);
            }

            void Clear()
            {
                this.Preview = this.Preview.Resize(1, 1, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
                this.Preview.Clear(Colors.Black);
            }
        }
    }
}

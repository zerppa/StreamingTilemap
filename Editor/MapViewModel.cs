namespace Editor
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows.Input;

    /// <summary>
    /// View model for a single map item.
    /// </summary>
    public class MapViewModel
    {
        /// <summary>
        /// Stores the <see cref="MapBrowserViewModel"/> who owns this item.
        /// </summary>
        private readonly MapBrowserViewModel parent;

        /// <summary>
        /// Stores the associated <see cref="DirectoryInfo"/>.
        /// </summary>
        private readonly DirectoryInfo directory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapViewModel"/> class.
        /// </summary>
        /// <param name="browser">The browser.</param>
        /// <param name="directory">The directory.</param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="browser"/> cannot be <c>null</c>.
        /// or
        /// The <paramref name="directory"/> cannot be <c>null</c>.
        /// </exception>
        public MapViewModel(MapBrowserViewModel browser, DirectoryInfo directory)
        {
            this.parent = browser ?? throw new ArgumentNullException(nameof(browser), $"The {nameof(browser)} cannot be null.");
            this.directory = directory ?? throw new ArgumentNullException(nameof(directory), $"The {nameof(directory)} cannot be null.");

            this.DeleteCommand = new Command<MapViewModel>(
                item => this.parent.DeleteMap(item),
                item => !string.Equals(item?.Name, Configuration.DefaultMapName, StringComparison.InvariantCultureIgnoreCase));

            this.CleanCommand = new Command(this.DeleteEmptyChunks);
        }

        /// <summary>
        /// Gets the delete command.
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// Gets the clean command.
        /// </summary>
        public ICommand CleanCommand { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name => this.directory.Name;

        /// <summary>
        /// Gets the absolute path.
        /// </summary>
        public string FullName => this.directory.FullName;

        /// <summary>
        /// Deletes all chunks from this map.
        /// </summary>
        internal void DeleteChunks()
        {
            this.directory.EnumerateFiles().ForEach(file => file.Delete());
        }

        /// <summary>
        /// Examines all chunks that belong to this map and deletes those that contain no visible tile data.
        /// </summary>
        private void DeleteEmptyChunks()
        {
            foreach (var chunkFile in this.directory.EnumerateFiles("*.chunk").ToArray())
            {
                chunkFile.Refresh();
                if (!chunkFile.Exists)
                {
                    continue;
                }

                var filename = chunkFile.FullName;
                var fileContainsNonEmptyValue = false;

                try
                {
                    using (var stream = chunkFile.OpenRead())
                    {
                        int b;
                        while ((b = stream.ReadByte()) >= 0)
                        {
                            if (b > 0)
                            {
                                fileContainsNonEmptyValue = true;
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    continue;
                }

                if (!fileContainsNonEmptyValue)
                {
                    Trace.TraceInformation($"Deleting chunk file '{chunkFile.Name}'...");
                    File.Delete(filename);
                }
            }
        }
    }
}

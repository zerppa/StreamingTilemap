namespace Editor
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;

    /// <summary>
    /// View model for the main window.
    /// </summary>
    /// <seealso cref="Editor.ViewModel" />
    public class MainViewModel : ViewModel
    {
        /// <summary>
        /// The backend field for the property.
        /// </summary>
        private int cameraX;

        /// <summary>
        /// The backend field for the property.
        /// </summary>
        private int cameraY;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="loadMap"><c>true</c> to immediately load the map.</param>
        public MainViewModel(bool loadMap = true)
        {
            // Initialize the chunk pool
            for (var y = 0; y < 3; y++)
            {
                for (var x = 0; x < 3; x++)
                {
                    this.Chunks[x, y] = new ChunkViewModel();
                }
            }

            // Synchronize data changes from the Context
            this.Context.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(this.Context.CurrentMap))
                    {
                        this.RaisePropertyChanged(nameof(this.CurrentMap));
                        this.LoadMap();
                    }
                };

            // Create commands
            this.MoveLeftCommand = new Command(() => this.MoveCamera(--this.CameraX, this.CameraY));
            this.MoveRightCommand = new Command(() => this.MoveCamera(++this.CameraX, this.CameraY));
            this.MoveUpCommand = new Command(() => this.MoveCamera(this.CameraX, --this.CameraY));
            this.MoveDownCommand = new Command(() => this.MoveCamera(this.CameraX, ++this.CameraY));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
            : this(false)
        {
        }

        /// <summary>
        /// Gets the move left command.
        /// </summary>
        public ICommand MoveLeftCommand { get; }

        /// <summary>
        /// Gets the move right command.
        /// </summary>
        public ICommand MoveRightCommand { get; }

        /// <summary>
        /// Gets the move up command.
        /// </summary>
        public ICommand MoveUpCommand { get; }

        /// <summary>
        /// Gets the move down command.
        /// </summary>
        public ICommand MoveDownCommand { get; }

        /// <summary>
        /// Gets the visible chunks.
        /// </summary>
        /// <remarks>
        /// Enumerates the chunk array from left to right, from top to bottom.
        /// </remarks>
        public IEnumerable<ChunkViewModel> VisibleChunks => Enumerable.Range(0, 3).SelectMany(y => Enumerable.Range(0, 3).Select(x => this.Chunks[x, y]));

        /// <summary>
        /// Gets the current map name.
        /// </summary>
        public string CurrentMap => this.Context.CurrentMap;

        /// <summary>
        /// Gets the camera X coordinate, in chunk units.
        /// </summary>
        public int CameraX
        {
            get => this.cameraX;
            private set => this.SetValue(ref this.cameraX, value);
        }

        /// <summary>
        /// Gets the camera Y coordinate, in chunk units.
        /// </summary>
        public int CameraY
        {
            get => this.cameraY;
            private set => this.SetValue(ref this.cameraY, value);
        }

        /// <summary>
        /// Gets the visible chunks.
        /// </summary>
        private ChunkViewModel[,] Chunks { get; } = new ChunkViewModel[3, 3];

        /// <summary>
        /// Called when the 'paint' action has occurred.
        /// </summary>
        /// <param name="chunk">The chunk.</param>
        /// <param name="location">The location.</param>
        internal void OnPaint(ChunkViewModel chunk, (int x, int y) location)
        {
            if (chunk == null
                || location.x < 0
                || location.x >= Configuration.ChunkWidth
                || location.y < 0
                || location.y >= Configuration.ChunkHeight)
            {
                return;
            }

            var tile = this.Context.ForeTileIndex;
            if (chunk.Tiles[location.x, location.y] != tile)
            {
                chunk.Tiles[location.x, location.y] = tile;

                chunk.Save();
                chunk.Render(this.Context.AssetManager.GetTileset());
            }
        }

        /// <summary>
        /// Called when the 'erase' action has occurred.
        /// </summary>
        /// <param name="chunk">The chunk.</param>
        /// <param name="location">The location.</param>
        internal void OnErase(ChunkViewModel chunk, (int x, int y) location)
        {
            if (chunk == null
                || location.x < 0
                || location.x >= Configuration.ChunkWidth
                || location.y < 0
                || location.y >= Configuration.ChunkHeight)
            {
                return;
            }

            var tile = this.Context.BackTileIndex;
            if (chunk.Tiles[location.x, location.y] != tile)
            {
                chunk.Tiles[location.x, location.y] = tile;

                chunk.Save();
                chunk.Render(this.Context.AssetManager.GetTileset());
            }
        }

        /// <summary>
        /// Moves the camera so that the chunk at the specified coordinates is centered.
        /// </summary>
        /// <param name="x">The X coordinate, in chunk units.</param>
        /// <param name="y">The Y coordinate, in chunk units.</param>
        private void MoveCamera(int x, int y)
        {
            for (var yy = 0; yy < 3; yy++)
            {
                for (var xx = 0; xx < 3; xx++)
                {
                    var chunk = this.Chunks[xx, yy];
                    var chunkX = x + xx - 1;
                    var chunkY = y + yy - 1;
                    chunk.X = chunkX;
                    chunk.Y = chunkY;

                    chunk.Load();
                    chunk.Render(this.Context.AssetManager.GetTileset());
                }
            }
        }

        /// <summary>
        /// Resets the camera and moves it to position (0, 0).
        /// </summary>
        private void ResetCamera()
        {
            this.MoveCamera(this.CameraX = 0, this.CameraY = 0);
        }

        /// <summary>
        /// Loads the map.
        /// </summary>
        private void LoadMap()
        {
            this.ResetCamera();
        }
    }
}

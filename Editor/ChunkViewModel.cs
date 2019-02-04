namespace Editor
{
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// View model for a chunk.
    /// </summary>
    /// <seealso cref="Editor.ViewModel" />
    public class ChunkViewModel : ViewModel
    {
        /// <summary>
        /// The color used for clearing a tile when it has no value.
        /// </summary>
        private static readonly Color ClearColor = Colors.Black;

        /// <summary>
        /// The backend field for the property.
        /// </summary>
        private int x;

        /// <summary>
        /// The backend field for the property.
        /// </summary>
        private int y;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChunkViewModel"/> class.
        /// </summary>
        public ChunkViewModel()
        {
            this.Bitmap = new WriteableBitmap(
                (int)this.PixelWidth,
                (int)this.PixelHeight,
                96,
                96,
                PixelFormats.Bgr32,
                null);
        }

        /// <summary>
        /// Gets the bitmap used for rendering the chunk.
        /// </summary>
        public WriteableBitmap Bitmap { get; }

        /// <summary>
        /// Gets the pixel width of the chunk.
        /// </summary>
        public double PixelWidth => Configuration.ChunkWidth * Configuration.TileSize;

        /// <summary>
        /// Gets the pixel height of the chunk.
        /// </summary>
        public double PixelHeight => Configuration.ChunkHeight * Configuration.TileSize;

        /// <summary>
        /// Gets the X coordinate, in chunk units.
        /// </summary>
        public int X
        {
            get => this.x;
            internal set => this.SetValue(ref this.x, value);
        }

        /// <summary>
        /// Gets the Y coordinate, in chunk units.
        /// </summary>
        public int Y
        {
            get => this.y;
            internal set => this.SetValue(ref this.y, value);
        }

        /// <summary>
        /// Gets the tile data.
        /// </summary>
        public int[,] Tiles { get; } = new int[Configuration.ChunkWidth, Configuration.ChunkHeight];

        /// <summary>
        /// Updates the chunk bitmap using the specified tileset.
        /// </summary>
        /// <param name="tileset">The tileset.</param>
        internal void Render(Tileset tileset)
        {
            if (tileset == null)
            {
                this.Bitmap.Clear(ClearColor);
                return;
            }

            for (var y = 0; y < Configuration.ChunkHeight; y++)
            {
                for (var x = 0; x < Configuration.ChunkWidth; x++)
                {
                    var value = this.Tiles[x, y];
                    if (value > 0)
                    {
                        tileset.BlitTo(
                            value - 1,
                            this.Bitmap,
                            x * Configuration.TileSize,
                            y * Configuration.TileSize);
                    }
                    else
                    {
                        this.Bitmap.FillRectangle(
                            x * Configuration.TileSize,
                            y * Configuration.TileSize,
                            (x + 1) * Configuration.TileSize,
                            (y + 1) * Configuration.TileSize,
                            ClearColor);
                    }
                }
            }
        }

        /// <summary>
        /// Loads the chunk data.
        /// </summary>
        internal void Load()
        {
            this.Context.AssetManager.LoadTiles(this.X, this.Y, this.Tiles);
        }

        /// <summary>
        /// Saves the chunk data.
        /// </summary>
        internal void Save()
        {
            this.Context.AssetManager.SaveTiles(this.X, this.Y, this.Tiles);
        }
    }
}

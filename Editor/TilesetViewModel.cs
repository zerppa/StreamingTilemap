namespace Editor
{
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// View model for the tileset window.
    /// </summary>
    /// <seealso cref="Editor.ViewModel" />
    public class TilesetViewModel : ViewModel
    {
        /// <summary>
        /// Represents the color of 'no value'.
        /// </summary>
        private static readonly Color ClearColor = Colors.Black;

        /// <summary>
        /// Stores the associated tileset.
        /// </summary>
        private readonly Tileset tileset;

        /// <summary>
        /// Initializes a new instance of the <see cref="TilesetViewModel"/> class.
        /// </summary>
        public TilesetViewModel()
        {
            this.tileset = this.Context.AssetManager.GetTileset();

            this.Bitmap = new WriteableBitmap(
                this.tileset.PixelWidth,
                this.tileset.PixelHeight,
                96,
                96,
                PixelFormats.Bgr32,
                null);

            this.tileset.BlitTo(this.Bitmap);

            this.ForeTile = new WriteableBitmap(
                Configuration.TileSize,
                Configuration.TileSize,
                96,
                96,
                PixelFormats.Bgr32,
                null);

            this.BackTile = new WriteableBitmap(
                Configuration.TileSize,
                Configuration.TileSize,
                96,
                96,
                PixelFormats.Bgr32,
                null);

            this.ClearForeCommand = new Command(() => this.ForeTileIndex = 0);
            this.ClearBackCommand = new Command(() => this.BackTileIndex = 0);

            this.ForeTileIndex = this.Context.ForeTileIndex;
            this.BackTileIndex = this.Context.BackTileIndex;
        }

        /// <summary>
        /// Gets the clear fore command.
        /// </summary>
        public ICommand ClearForeCommand { get; }

        /// <summary>
        /// Gets the clear back command.
        /// </summary>
        public ICommand ClearBackCommand { get; }

        /// <summary>
        /// Gets the bitmap.
        /// </summary>
        public WriteableBitmap Bitmap { get; }

        /// <summary>
        /// Gets <see cref="Configuration.TileSize"/>.
        /// </summary>
        public double TileWidth => Configuration.TileSize;

        /// <summary>
        /// Gets <see cref="Configuration.TileSize"/>.
        /// </summary>
        public double TileHeight => Configuration.TileSize;

        /// <summary>
        /// Gets the fore tile bitmap.
        /// </summary>
        public WriteableBitmap ForeTile { get; }

        /// <summary>
        /// Gets the back tile bitmap.
        /// </summary>
        public WriteableBitmap BackTile { get; }

        /// <summary>
        /// Gets or sets tile index for the fore tile. Zero clears, otherwise the 1-based index within the tileset.
        /// </summary>
        public int ForeTileIndex
        {
            get
            {
                return this.Context.ForeTileIndex;
            }

            set
            {
                this.Context.ForeTileIndex = value;
                this.RaisePropertyChanged();
                this.UpdateForeTile();
            }
        }

        /// <summary>
        /// Gets or sets tile index for the back tile. Zero clears, otherwise the 1-based index within the tileset.
        /// </summary>
        public int BackTileIndex
        {
            get
            {
                return this.Context.BackTileIndex;
            }

            set
            {
                this.Context.BackTileIndex = value;
                this.RaisePropertyChanged();
                this.UpdateBackTile();
            }
        }

        /// <summary>
        /// Called when user picked a tile in the given coordinates. Coordinates are in 0-based tile units.
        /// </summary>
        /// <param name="x">The X coordinate in tile units.</param>
        /// <param name="y">The Y coordinate in tile units.</param>
        internal void OnPickFore(int x, int y) => this.ForeTileIndex = this.tileset.IndexAt(y, x) + 1;

        /// <summary>
        /// Called when user picked a tile in the given coordinates. Coordinates are in 0-based tile units.
        /// </summary>
        /// <param name="x">The X coordinate in tile units.</param>
        /// <param name="y">The Y coordinate in tile units.</param>
        internal void OnPickBack(int x, int y) => this.BackTileIndex = this.tileset.IndexAt(y, x) + 1;

        /// <summary>
        /// Updates the selected fore tile image.
        /// </summary>
        private void UpdateForeTile()
        {
            var index = this.ForeTileIndex;
            if (index <= 0)
            {
                this.ForeTile.Clear(ClearColor);
            }
            else
            {
                this.tileset.BlitTo(index - 1, this.ForeTile, 0, 0);
            }
        }

        /// <summary>
        /// Updates the selected back tile image.
        /// </summary>
        private void UpdateBackTile()
        {
            var index = this.BackTileIndex;
            if (index <= 0)
            {
                this.BackTile.Clear(ClearColor);
            }
            else
            {
                this.tileset.BlitTo(index - 1, this.BackTile, 0, 0);
            }
        }
    }
}

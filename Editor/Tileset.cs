namespace Editor
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Represents the texture atlas that contains all tiles usable in a map.
    /// </summary>
    public class Tileset
    {
        /// <summary>
        /// Stores the atlas bitmap.
        /// </summary>
        private readonly WriteableBitmap bitmap;

        /// <summary>
        /// Stores the tile size.
        /// </summary>
        private readonly Size tileSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tileset"/> class.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="tileSize">The tile size, in pixels.</param>
        /// <exception cref="System.ArgumentException">
        /// The <paramref name="filename"/> cannot be <c>null</c> or empty.
        /// or
        /// The <paramref name="tileSize"/> cannot be less or equal to zero.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">The bitmap dimensions should be exactly a multiple of the tile size.</exception>
        public Tileset(string filename, int tileSize)
        {
            Debug.Assert(tileSize == Configuration.TileSize, "The tile size should equal to that of Configuration.");

            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException($"The {nameof(filename)} cannot be null or empty.", nameof(filename));
            }

            if (tileSize <= 0)
            {
                throw new ArgumentException($"The {nameof(tileSize)} cannot be less or equal to zero.", nameof(tileSize));
            }

            this.bitmap = new WriteableBitmap(new BitmapImage(new Uri(filename, UriKind.Relative)));

            if (tileSize > this.bitmap.PixelWidth
                || tileSize > this.bitmap.PixelHeight
                || this.bitmap.PixelWidth % tileSize != 0
                || this.bitmap.PixelHeight % tileSize != 0)
            {
                throw new InvalidOperationException("The bitmap dimensions should be exactly a multiple of the tile size.");
            }

            this.Filename = filename;
            this.tileSize = new Size(tileSize, tileSize);
            this.Width = this.bitmap.PixelWidth / tileSize;
            this.Height = this.bitmap.PixelHeight / tileSize;
        }

        /// <summary>
        /// Gets the filename.
        /// </summary>
        public string Filename { get; }

        /// <summary>
        /// Gets the tileset's asset name.
        /// </summary>
        public string Name => Path.GetFileNameWithoutExtension(this.Filename);

        /// <summary>
        /// Gets the width, in tile units.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the height, in tile units.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the total tile count.
        /// </summary>
        public int Count => this.Width * this.Height;

        /// <summary>
        /// Gets the tileset bitmap width in pixels.
        /// </summary>
        public int PixelWidth => this.bitmap.PixelWidth;

        /// <summary>
        /// Gets the tileset bitmap height in pixels.
        /// </summary>
        public int PixelHeight => this.bitmap.PixelHeight;

        /// <summary>
        /// Gets the tile index at the given row and column. The index is zero-based.
        /// </summary>
        /// <param name="row">The row. Zero-based.</param>
        /// <param name="column">The column. Zero-based.</param>
        /// <returns>The zero-based tile index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The row was out of range.
        /// or
        /// The column was out of range.
        /// </exception>
        public int IndexAt(int row, int column)
        {
            if (row < 0 || row >= this.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(row), $"The {nameof(row)} was out of range. Expected [0, {this.Height - 1}].");
            }

            if (column < 0 || column >= this.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(column), $"The {nameof(column)} was out of range. Expected [0, {this.Width - 1}].");
            }

            return row * this.Width + column;
        }

        /// <summary>
        /// Gets the row and column for the corresponding index. All units are zero-based.
        /// </summary>
        /// <param name="index">The index. Zero-based.</param>
        /// <returns>The row and column. Zero-based.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">The index was out of range.</exception>
        public (int row, int column) FromIndex(int index)
        {
            if (index < 0 || index >= this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"The {nameof(index)} was out of range. Expected [0, {this.Count - 1}].");
            }

            var row = index / this.Width;
            var column = index % this.Width;

            return (row, column);
        }

        /// <summary>
        /// Copies the pixels of the tile at the given index into the target bitmap at the given location. The location is given in pixel units.
        /// </summary>
        /// <param name="tileIndex">The tile index. Zero-based.</param>
        /// <param name="target">The target <see cref="WriteableBitmap"/>.</param>
        /// <param name="targetX">The target X coordinate, in pixels.</param>
        /// <param name="targetY">The target Y coordinate, in pixels.</param>
        public void BlitTo(int tileIndex, WriteableBitmap target, int targetX, int targetY)
        {
            var (row, column) = this.FromIndex(tileIndex);
            var sourceRect = new Rect(new Point(column * this.tileSize.Width, row * this.tileSize.Height), this.tileSize);

            target?.Blit(new Rect(new Point(targetX, targetY), this.tileSize), this.bitmap, sourceRect);
        }

        /// <summary>
        /// Copies the entire tileset into the target bitmap.
        /// </summary>
        /// <param name="target">The target <see cref="WriteableBitmap"/>.</param>
        public void BlitTo(WriteableBitmap target) => target?.BlitRender(this.bitmap);
    }
}

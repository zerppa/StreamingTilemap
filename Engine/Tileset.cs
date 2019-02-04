namespace Engine
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Provides tileset specific calculations.
    /// </summary>
    public class Tileset
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tileset"/> class.
        /// </summary>
        /// <param name="pixelWidth">The width of the tileset atlas texture, in pixels.</param>
        /// <param name="pixelHeight">The height of the tileset atlas texture, in pixels.</param>
        /// <exception cref="InvalidOperationException">Tileset must have positive area.</exception>
        public Tileset(uint pixelWidth, uint pixelHeight)
        {
            if (pixelWidth == 0 || pixelHeight == 0)
            {
                throw new InvalidOperationException("Tileset must have positive area.");
            }

            this.Width = (int)pixelWidth / Configuration.TileSize;
            this.Height = this.Width = (int)pixelHeight / Configuration.TileSize;
        }

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
        /// Gets the texture UV coordinates for the specified tile index.
        /// </summary>
        /// <param name="index">The index. Zero-based.</param>
        /// <returns>The <see cref="ValueTuple{T1, T2}"/> representing the top-left and bottom-right corners of the texture region.
        /// Each <see cref="float"/> is between 0 and 1 where 0 is left/top and 1 right/bottom.</returns>
        public (Vector2 topLeft, Vector2 bottomRight) GetTileUV(int index)
        {
            if (index < 0 || index > this.Count - 1)
            {
                // Consider error
                return (new Vector2(0f), new Vector2(0f));
            }

            var (row, column) = this.FromIndex(index);

            return (new Vector2(column / (float)this.Width, row / (float)this.Height), new Vector2((column + 1) / (float)this.Width, (row + 1) / (float)this.Height));
        }
    }
}

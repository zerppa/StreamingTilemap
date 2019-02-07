namespace Engine
{
    using System;

    /// <summary>
    /// Represents a chunk (tilemaps are made out of chunks).
    /// </summary>
    public class Chunk
    {
#pragma warning disable SA1401 // Fields should be private
        /// <summary>
        /// The flag indicating whether this chunk is currently active (and not in the pool).
        /// </summary>
        internal volatile bool IsInUse;

        /// <summary>
        /// The lock for multi-threaded access.
        /// </summary>
        private readonly object syncRoot = new object();

        /// <summary>
        /// Points to the currently used tile buffer.
        /// </summary>
        private ushort[] currentBuffer;

        /// <summary>
        /// Points to the currently used tile buffer.
        /// </summary>
        private ushort[] backBuffer;

        /// <summary>
        /// Stores a flag indicating whether the tile buffer has been rewritten.
        /// </summary>
        private volatile bool hasPendingChanges;
#pragma warning restore SA1401 // Fields should be private

        /// <summary>
        /// Initializes a new instance of the <see cref="Chunk"/> class.
        /// </summary>
        public Chunk()
        {
            this.currentBuffer = new ushort[Configuration.ChunkWidth * Configuration.ChunkHeight];
            this.backBuffer = new ushort[Configuration.ChunkWidth * Configuration.ChunkHeight];
        }

        /// <summary>
        /// Gets or sets the X coordinate, in chunk units.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate, in chunk units.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Gets the tile array.
        /// </summary>
        /// <remarks>
        /// Although chunks are 2-dimensional, the tiles are stored in a 1-dimensional array for performance reasons.
        /// Calculate the tile index manually using <see cref="Configuration.ChunkWidth"/> and <see cref="Configuration.ChunkHeight"/>.
        /// </remarks>>
        public ushort[] Tiles => this.currentBuffer;

        /// <summary>
        /// Resets all properties.
        /// </summary>
        public void Clear()
        {
            lock (this.syncRoot)
            {
                this.X = 0;
                this.Y = 0;
                Array.Clear(this.currentBuffer, 0, this.Tiles.Length);
                Array.Clear(this.backBuffer, 0, this.Tiles.Length);
                this.hasPendingChanges = false;
            }
        }

        /// <summary>
        /// Flips the tile buffers if there are pending changes.
        /// </summary>
        internal void ApplyPendingChanges()
        {
            if (!this.IsInUse)
            {
                return;
            }

            lock (this.syncRoot)
            {
                if (this.hasPendingChanges)
                {
                    var temp = this.backBuffer;
                    this.backBuffer = this.currentBuffer;
                    this.currentBuffer = temp;
                    this.hasPendingChanges = false;
                }
            }
        }

        /// <summary>
        /// Updates the chunk based on the supplied information.
        /// The tile data in <paramref name="data"/> is copied to the chunk's internal array.
        /// The <paramref name="data"/> array size should be exactly <c>ChunkWidth * ChunkHeight</c>.
        /// </summary>
        /// <param name="x">The chunk X coordinate, in chunk units.</param>
        /// <param name="y">The chunk Y coordinate, in chunk units.</param>
        /// <param name="data">The data.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="data"/> cannot be <c>null</c>.</exception>
        internal void SetData(int x, int y, ushort[] data)
        {
            if (!this.IsInUse || x != this.X || y != this.Y)
            {
                return;
            }

            lock (this.syncRoot)
            {
                Array.Copy(
                    data ?? throw new ArgumentNullException(nameof(data), $"The {nameof(data)} cannot be null."),
                    this.backBuffer,
                    data.Length);

                this.hasPendingChanges = true;
            }
        }
    }
}

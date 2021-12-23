namespace Engine
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using static Utility;

    /// <summary>
    /// Manages the set of active chunks around the camera.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class ChunkManager : IDisposable
    {
        /// <summary>
        /// Represents the empty default tile data for an unloaded chunk.
        /// </summary>
        private static readonly ushort[] EmptyChunkData = new ushort[Configuration.ChunkWidth * Configuration.ChunkHeight];

        /// <summary>
        /// Stores the internal chunk pool.
        /// </summary>
        private readonly Pool<Chunk> chunkPool;

        /// <summary>
        /// Stores the list of currently managed active chunks.
        /// </summary>
        private readonly Dictionary<(int x, int y), Chunk> chunkCache;

        /// <summary>
        /// The temporary list of chunks that have fallen off the managed area and are subject of being recycled.
        /// </summary>
        private readonly List<KeyValuePair<(int, int), Chunk>> doomedChunks = new ();

        /// <summary>
        /// Contains the pre-calculated ordered list of offsets that can be applied to chunk coordinates
        /// in order to create a spiral outwards of it until it exceeds the view range from the origin.
        /// </summary>
        private readonly IReadOnlyList<(int dx, int dy, int sd)> preCalculatedNearbyChunkOffsets;

        /// <summary>
        /// Stores the internal chunk processor.
        /// </summary>
        private readonly ChunkProcessor chunkProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChunkManager"/> class.
        /// </summary>
        /// <param name="contentManager">The content manager.</param>
        public ChunkManager(ContentManager contentManager)
        {
            this.chunkProcessor = new ChunkProcessor(contentManager);

            // Create the pre-calculated surrounding chunk offsets based on range
            var chunkOffsets = new List<(int ox, int oy, int squaredDistance)>((int)Math.Ceiling(Math.PI * (Configuration.ChunkViewRange * Configuration.ChunkViewRange)));
            const int squaredMaxDistance = Configuration.ChunkViewRange * Configuration.ChunkViewRange;
            for (var y = -Configuration.ChunkViewRange; y <= Configuration.ChunkViewRange; y++)
            {
                for (var x = -Configuration.ChunkViewRange; x <= Configuration.ChunkViewRange; x++)
                {
                    var dx = Math.Abs(x);
                    var dy = Math.Abs(y);
                    var squaredDistance = dx * dx + dy * dy;
                    if (squaredDistance <= squaredMaxDistance)
                    {
                        chunkOffsets.Add((x, y, squaredDistance));
                    }
                }
            }

            this.preCalculatedNearbyChunkOffsets = chunkOffsets.OrderBy(tuple => tuple.squaredDistance).ToList();

            // Create the chunk pool and cache
            var managedChunkCount = (int)Math.Ceiling(Math.PI * (Configuration.ChunkViewFalloffRange * Configuration.ChunkViewFalloffRange));
            this.chunkPool = new Pool<Chunk>(managedChunkCount);
            this.chunkCache = new Dictionary<(int x, int y), Chunk>(managedChunkCount);
        }

        /// <summary>
        /// Sets the focused chunk where the camera is centered at and updates the managed chunk list accordingly.
        /// Chunks that are too far off from the focused chunk will be recycled.
        /// New chunks are then asynchronously loaded surrounding the focused chunk.
        /// </summary>
        /// <param name="chunkX">The focus chunk X coordinate, in chunk units.</param>
        /// <param name="chunkY">The focus chunk Y coordinate, in chunk units.</param>
        public void SetCameraAt(int chunkX, int chunkY)
        {
            // Inform the chunk loader about the current camera position so that it can discard obsolete jobs
            this.chunkProcessor.SetOrigin((chunkX, chunkY));

            // Recycle the chunks that are too far away from the camera
            const int squaredFalloffDistance = Configuration.ChunkViewFalloffRange * Configuration.ChunkViewFalloffRange;
            foreach (var entry in this.chunkCache)
            {
                var chunk = entry.Value;
                if (SquaredDistance((chunkX, chunkY), (chunk.X, chunk.Y)) > squaredFalloffDistance)
                {
                    this.doomedChunks.Add(entry);
                }
            }

            foreach (var (key, chunk) in this.doomedChunks)
            {
                var removed = this.chunkCache.Remove(key);
                Debug.Assert(removed, "The chunk was not found in the cache. Removal failed.");

                this.chunkPool.Return(chunk);
                chunk.Clear();
                chunk.IsInUse = false;
            }

            this.doomedChunks.Clear();

            // Load chunks surrounding the camera
            foreach (var requiredChunk in this.GetChunksAround(chunkX, chunkY))
            {
                var position = (requiredChunk.x, requiredChunk.y);
                if (!this.chunkCache.TryGetValue(position, out _))
                {
                    if (this.chunkPool.CanWithdraw)
                    {
                        var chunk = this.chunkPool.Withdraw();
                        chunk.X = position.x;
                        chunk.Y = position.y;
                        chunk.IsInUse = true;
                        this.chunkCache.Add(position, chunk);
                        this.chunkProcessor.Enqueue(position, chunk);
                    }
                    else
                    {
                        throw new InvalidOperationException("The chunk pool is exhausted.");
                    }
                }
            }
        }

        /// <summary>
        /// Applies all pending chunk changes.
        /// </summary>
        /// <remarks>This method should be called from the main thread.</remarks>
        public void ApplyAllPendingChanges()
        {
            foreach (var chunk in this.chunkCache.Values)
            {
                chunk.ApplyPendingChanges();
            }
        }

        /// <summary>
        /// Requests the tile data for the specific chunk. If no data is available at those coordinates, an empty array (of suitable size) is returned.
        /// </summary>
        /// <param name="chunkX">The chunk X coordinate, in chunk units.</param>
        /// <param name="chunkY">The chunk Y coordinate, in chunk units.</param>
        /// <returns>The array containing tile data for the chunk.</returns>
        /// <remarks>
        /// The empty default array is cached, and will be the same instance every time.
        /// The array is mutable, but you should never do that.
        /// </remarks>
        public ushort[] GetChunkData(int chunkX, int chunkY) => this.chunkCache.TryGetValue((chunkX, chunkY), out var chunk) ? chunk.Tiles : EmptyChunkData;

        /// <summary>
        /// Determines whether tile data is available for the specified chunk.
        /// </summary>
        /// <param name="chunkX">The chunk X coordinate, in chunk units.</param>
        /// <param name="chunkY">The chunk Y coordinate, in chunk units.</param>
        /// <returns>
        /// <c>true</c> if the chunk is currently being managed; otherwise, <c>false</c>.
        /// </returns>
        public bool IsChunkDataAvailable(int chunkX, int chunkY) => this.chunkCache.ContainsKey((chunkX, chunkY));

        /// <inheritdoc />
        public void Dispose()
        {
            this.chunkProcessor?.Dispose();
        }

        /// <summary>
        /// Gets the chunks surrounding and including the center chunk.
        /// The returned collection is sorted from closest to farthest.
        /// </summary>
        /// <param name="centerChunkX">The center chunk X coordinate, in chunk units.</param>
        /// <param name="centerChunkY">The center chunk Y coordinate, in chunk units.</param>
        /// <returns>Collection of chunk coordinates.</returns>
        private IEnumerable<(int x, int y)> GetChunksAround(int centerChunkX, int centerChunkY)
        {
            foreach (var offset in this.preCalculatedNearbyChunkOffsets)
            {
                yield return (centerChunkX + offset.dx, centerChunkY + offset.dy);
            }
        }
    }
}

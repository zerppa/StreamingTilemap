namespace Engine
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using static Utility;

    /// <summary>
    /// Provides asynchronous chunk loading services.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class ChunkProcessor : IDisposable
    {
        /// <summary>
        /// Stores the content manager.
        /// </summary>
        private readonly ContentManager contentManager;

        /// <summary>
        /// The producer-consumer implementation.
        /// </summary>
        private readonly BlockingCollection<(int, int, Chunk)> jobProcessor = new (100);

        /// <summary>
        /// Keeps track of the currently queued chunks.
        /// </summary>
        private readonly ConcurrentDictionary<(int x, int y), Chunk> queuedJobs = new ();

        /// <summary>
        /// The origin around which chunks are managed. Loading jobs that are too far away from the origin will be skipped.
        /// </summary>
        private (int x, int y) origin;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChunkProcessor"/> class.
        /// </summary>
        /// <param name="contentManager">The content manager.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="contentManager"/> cannot be <c>null</c>.</exception>
        public ChunkProcessor(ContentManager contentManager)
        {
            this.contentManager = contentManager ?? throw new ArgumentNullException(nameof(contentManager), $"The {nameof(contentManager)} cannot be null.");

            Task.Run(this.Loop);
        }

        /// <summary>
        /// Places an order for the certain chunk's data to be loaded and populated.
        /// </summary>
        /// <param name="position">The chunk's position, in chunk units.</param>
        /// <param name="chunk">The target <see cref="Chunk"/> object, whom the loaded data is pushed to.</param>
        public void Enqueue((int x, int y) position, Chunk chunk)
        {
            if (this.queuedJobs.ContainsKey(position))
            {
                // Already queued -> skip
                return;
            }

            if (this.queuedJobs.TryAdd(position, chunk))
            {
                this.jobProcessor.Add((position.x, position.y, chunk));
            }
        }

        /// <summary>
        /// Sets the origin.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <remarks>
        /// Loading jobs that are too far away from the origin will be skipped.
        /// </remarks>
        public void SetOrigin((int chunkX, int chunkY) position) => this.origin = position;

        /// <inheritdoc />
        public void Dispose()
        {
            this.jobProcessor.CompleteAdding();
            this.jobProcessor.Dispose();
        }

        /// <summary>
        /// Processes all the incoming loading requests.
        /// </summary>
        /// <returns>The <see cref="Task"/> that persists until the application closes.</returns>
        private async Task Loop()
        {
            const int squaredFalloffDistance = Configuration.ChunkViewFalloffRange * Configuration.ChunkViewFalloffRange;

            var staging = new ushort[Configuration.ChunkWidth * Configuration.ChunkHeight];

            while (!this.jobProcessor.IsCompleted)
            {
                (int x, int y, Chunk chunk) data;
                try
                {
                    data = this.jobProcessor.Take();
                }
                catch (InvalidOperationException)
                {
                    break;
                }

                var position = (data.x, data.y);

                // If the job is obsolete (the chunk to be loaded is too far from the camera), just skip it
                if (SquaredDistance(position, this.origin) > squaredFalloffDistance)
                {
                    this.queuedJobs.TryRemove(position, out _);
                    continue;
                }

                await this.contentManager.LoadChunkData(position, staging).ConfigureAwait(false);

                data.chunk.SetData(data.x, data.y, staging);
                this.queuedJobs.TryRemove(position, out _);

                Array.Clear(staging, 0, staging.Length);
            }

            this.jobProcessor.Dispose();
        }
    }
}

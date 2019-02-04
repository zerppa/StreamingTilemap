namespace Engine
{
    /// <summary>
    /// Global settings for the chunk engine.
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// The tile width and height, in pixels.
        /// </summary>
        public const int TileSize = 32;

        /// <summary>
        /// The chunk width, in tile units.
        /// </summary>
        public const int ChunkWidth = 9;

        /// <summary>
        /// The chunk height, in tile units.
        /// </summary>
        public const int ChunkHeight = 9;

        /// <summary>
        /// The distance how far ahead the chunks get preloaded in relation to the current chunk (where the character or camera is located).
        /// Lower values mean more aggressive loading. Higher values pre-load further in advance, but more at a time.
        /// </summary>
        public const int ChunkViewRange = 2;

        /// <summary>
        /// The distance how far off a chunk can get until it gets recycled.
        /// </summary>
        public const int ChunkViewFalloffRange = ChunkViewRange + 2;
    }
}

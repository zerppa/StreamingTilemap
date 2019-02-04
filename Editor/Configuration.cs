namespace Editor
{
    /// <summary>
    /// Constants.
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
        /// The folder that contains maps.
        /// </summary>
        public const string MapsFolder = "Maps";

        /// <summary>
        /// The default map name.
        /// </summary>
        public const string DefaultMapName = "Default";
    }
}

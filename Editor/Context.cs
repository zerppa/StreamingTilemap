namespace Editor
{
    /// <summary>
    /// Shared application state.
    /// </summary>
    /// <seealso cref="Editor.BindingSource" />
    public sealed class Context : BindingSource
    {
        /// <summary>
        /// The backend field for the property.
        /// </summary>
        private string currentMap;

        /// <summary>
        /// The backend field for the property.
        /// </summary>
        private int foreTileIndex = 1;

        /// <summary>
        /// The backend field for the property.
        /// </summary>
        private int backTileIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class.
        /// </summary>
        internal Context()
        {
            this.AssetManager = new AssetManager();
        }

        /// <summary>
        /// Gets the asset manager.
        /// </summary>
        public AssetManager AssetManager { get; }

        /// <summary>
        /// Gets or sets the current map.
        /// </summary>
        public string CurrentMap
        {
            get
            {
                return this.currentMap;
            }

            set
            {
                // Note: Set the asset manager first because other event listeners may assume that the asset manager is 'ready'
                this.AssetManager?.SetCurrentMap(value);
                this.SetValue(ref this.currentMap, value);
            }
        }

        /// <summary>
        /// Gets or sets tile index for the fore tile. Zero clears, otherwise the 1-based index within the tileset.
        /// </summary>
        public int ForeTileIndex
        {
            get => this.foreTileIndex;
            set => this.SetValue(ref this.foreTileIndex, value);
        }

        /// <summary>
        /// Gets or sets tile index for the back tile. Zero clears, otherwise the 1-based index within the tileset.
        /// </summary>
        public int BackTileIndex
        {
            get => this.backTileIndex;
            set => this.SetValue(ref this.backTileIndex, value);
        }
    }
}

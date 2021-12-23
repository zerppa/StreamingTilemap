namespace Editor
{
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Provides access to game assets.
    /// </summary>
    public class AssetManager
    {
        /// <summary>
        /// Stores the global tileset.
        /// </summary>
        private static Tileset cachedTileset;

        /// <summary>
        /// Keeps track of the current map folder.
        /// </summary>
        private string currentMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetManager"/> class.
        /// </summary>
        internal AssetManager()
        {
        }

        /// <summary>
        /// Gets the tileset.
        /// </summary>
        /// <returns>The <see cref="Tileset"/>.</returns>
        public Tileset GetTileset()
        {
            return cachedTileset ?? (cachedTileset = new Tileset("tileset.png", Configuration.TileSize));
        }

        /// <summary>
        /// Sets the current map.
        /// </summary>
        /// <param name="name">The name.</param>
        public void SetCurrentMap(string name) => this.currentMap = name != null ? Path.Combine(Configuration.MapsFolder, name) : null;

        /// <summary>
        /// Gets the specified chunk's data and sets the <paramref name="tiles"/> array with it.
        /// </summary>
        /// <param name="chunkX">The chunk X coordinate in chunk units.</param>
        /// <param name="chunkY">The chunk Y coordinate in chunk units.</param>
        /// /// <param name="tiles">The tiles.</param>
        public void LoadTiles(int chunkX, int chunkY, int[,] tiles)
        {
            var folder = this.currentMap;
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                FillEmpty();
                return;
            }

            var chunkFilename = Path.Combine(folder, FilenameForChunk(chunkX, chunkY));
            if (!File.Exists(chunkFilename))
            {
                FillEmpty();
                return;
            }

            try
            {
                using (var stream = File.OpenRead(chunkFilename))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        for (var y = 0; y < Configuration.ChunkHeight; y++)
                        {
                            for (var x = 0; x < Configuration.ChunkWidth; x++)
                            {
                                var value = reader.ReadUInt16();
                                tiles[x, y] = value;
                            }
                        }
                    }
                }
            }
            catch (EndOfStreamException)
            {
                Trace.TraceError($"The chunk file '{chunkFilename}' had incomplete data.");
                FillEmpty();
            }

            void FillEmpty()
            {
                for (var y = 0; y < Configuration.ChunkHeight; y++)
                {
                    for (var x = 0; x < Configuration.ChunkWidth; x++)
                    {
                        tiles[x, y] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Stores the specified chunk's data.
        /// </summary>
        /// <param name="chunkX">The chunk X coordinate in chunk units.</param>
        /// <param name="chunkY">The chunk Y coordinate in chunk units.</param>
        /// <param name="tiles">The tiles.</param>
        public void SaveTiles(int chunkX, int chunkY, int[,] tiles)
        {
            var folder = this.currentMap;
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                Debug.Fail("The map directory is not properly set.");
                return;
            }

            var chunkFilename = Path.Combine(folder, FilenameForChunk(chunkX, chunkY));

            try
            {
                using (var stream = File.OpenWrite(chunkFilename))
                {
                    using (var writer = new BinaryWriter(stream))
                    {
                        for (var y = 0; y < Configuration.ChunkHeight; y++)
                        {
                            for (var x = 0; x < Configuration.ChunkWidth; x++)
                            {
                                var value = (ushort)tiles[x, y];
                                writer.Write(value);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
            }
        }

        /// <summary>
        /// Determines a filename for the given chunk coordinates.
        /// </summary>
        /// <param name="x">The chunk X coordinate.</param>
        /// <param name="y">The chunk Y coordinate.</param>
        /// <returns>The filename.</returns>
        private static string FilenameForChunk(int x, int y) => $"{x},{y}.chunk";
    }
}

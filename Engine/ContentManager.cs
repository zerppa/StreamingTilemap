namespace Engine
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    using Veldrid;
    using Veldrid.ImageSharp;

    /// <summary>
    /// Provides access to the game assets.
    /// </summary>
    public class ContentManager
    {
        /// <summary>
        /// Stores the associated <see cref="Game"/>.
        /// </summary>
        private readonly Game game;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentManager"/> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="game"/> cannot be <c>null</c>.</exception>
        public ContentManager(Game game)
        {
            this.game = game ?? throw new ArgumentNullException(nameof(game), $"The {nameof(game)} cannot be null.");
        }

        /// <summary>
        /// Loads the specified map chunk.
        /// </summary>
        /// <param name="position">The position, in chunk units.</param>
        /// <param name="fillTo">The target array to which the data will be written.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="fillTo"/> cannot be <c>null</c>.</exception>
        public async Task LoadChunkData((int x, int y) position, ushort[] fillTo)
        {
            if (fillTo == null)
            {
                throw new ArgumentNullException(nameof(fillTo), $"The {nameof(fillTo)} cannot be null.");
            }

            // Simulate slight network latency
            await Task.Delay(100).ConfigureAwait(false);

            var chunkFile = FilenameForChunk(position.x, position.y);
            if (!File.Exists(chunkFile))
            {
                Array.Clear(fillTo, 0, fillTo.Length);
                return;
            }

            try
            {
                await using (var stream = File.OpenRead(chunkFile))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        for (var i = 0; i < Configuration.ChunkWidth * Configuration.ChunkHeight; i++)
                        {
                            var value = reader.ReadUInt16();
                            fillTo[i] = value;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Cannot load chunk data ({position.x}, {position.y}) from disk. {e}");
                Array.Clear(fillTo, 0, fillTo.Length);
            }
        }

        /// <summary>
        /// Loads the shader.
        /// </summary>
        /// <param name="factory">The resource factory.</param>
        /// <param name="set">The shader name.</param>
        /// <param name="stage">Either 'Vertex' (for vertex shader), or 'Fragment' (for fragment/pixel shader).</param>
        /// <param name="entryPoint">The entry point.</param>
        /// <returns>The <see cref="Shader"/>.</returns>
        /// <remarks>Supports HLSL, GLSL, Metal and Vulcan shaders.</remarks>
        public Shader LoadShader(ResourceFactory factory, string set, ShaderStages stage, string entryPoint)
        {
            var name = $"{nameof(Engine)}.{set}-{stage}.{GetShaderExtension(factory.BackendType)}";
            return factory.CreateShader(new ShaderDescription(stage, this.game.ReadEmbeddedAssetBytes(name), entryPoint));
        }

        /// <summary>
        /// Loads the texture.
        /// </summary>
        /// <param name="factory">The resource factory.</param>
        /// <param name="filename">The filename.</param>
        /// <returns>The <see cref="Texture"/>.</returns>
        public (Texture, TextureView) LoadTexture(ResourceFactory factory, string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                return this.LoadTexture(factory, stream);
            }
        }

        /// <summary>
        /// Loads the texture.
        /// </summary>
        /// <param name="factory">The resource factory.</param>
        /// <param name="stream">The stream.</param>
        /// <returns>The <see cref="Texture"/>.</returns>
        public (Texture, TextureView) LoadTexture(ResourceFactory factory, Stream stream)
        {
            var image = new ImageSharpTexture(stream, false);
            var texture = image.CreateDeviceTexture(this.game.GraphicsDevice, factory);
            var textureView = factory.CreateTextureView(texture);

            return (texture, textureView);
        }

        /// <summary>
        /// Gets the shader file extension based on the graphics backend.
        /// </summary>
        /// <param name="backendType">The backend type.</param>
        /// <returns>The file extension.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Unknown graphics backend.</exception>
        private static string GetShaderExtension(GraphicsBackend backendType)
        {
            var isMacOS = RuntimeInformation.OSDescription.Contains("Darwin", StringComparison.InvariantCultureIgnoreCase);

            return backendType switch
            {
                GraphicsBackend.Direct3D11 => "hlsl.bytes",
                GraphicsBackend.Vulkan => "450.spv",
                GraphicsBackend.OpenGL => "330.glsl",
                GraphicsBackend.Metal => isMacOS ? "metallib" : "ios.metallib",
                GraphicsBackend.OpenGLES => "300.glsles",
                _ => throw new ArgumentOutOfRangeException(nameof(backendType), backendType, null)
            };
        }

        /// <summary>
        /// Determines a filename for the given chunk coordinates.
        /// </summary>
        /// <param name="x">The chunk X coordinate.</param>
        /// <param name="y">The chunk Y coordinate.</param>
        /// <returns>The filename.</returns>
        private static string FilenameForChunk(int x, int y) => $"Maps/Overworld/{x},{y}.chunk";
    }
}

namespace Engine
{
    using System.Numerics;

    using Veldrid;

    using static Utility;

    /// <summary>
    /// Our sample game.
    /// </summary>
    /// <seealso cref="Engine.Game" />
    public class SampleGame : Game
    {
        private CommandList commandList;
        private Pipeline pipeline;

        private Texture tilesetTexture;
        private TextureView tilesetTextureView;
        private ResourceSet tilesetResourceSet;
        private Tileset tileset;

        private Shader vertexShader;
        private Shader fragmentShader;

        private DeviceBuffer projectionMatrixBuffer;
        private ResourceSet mainResourceSet;

        private DeviceBuffer vertexBuffer;
        private DeviceBuffer indexBuffer;

        private ResourceLayout mainLayout;
        private ResourceLayout textureLayout;

        private VertexPositionTextureColor[] tilemapVertexData;
        private ushort[] tilemapIndexData;

        private double cameraX;
        private double cameraY;

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleGame"/> class.
        /// </summary>
        /// <param name="window">The game window.</param>
        public SampleGame(IGameWindow window)
            : base(window)
        {
            this.Content = new ContentManager(this);
            this.Chunks = new ChunkManager(this.Content);
        }

        /// <summary>
        /// Gets the content manager.
        /// </summary>
        internal ContentManager Content { get; }

        /// <summary>
        /// Gets the chunk manager.
        /// </summary>
        internal ChunkManager Chunks { get; }

        /// <inheritdoc />
        protected override void CreateResources(ResourceFactory factory)
        {
            (this.tilemapVertexData, this.tilemapIndexData) = GenerateTilemapVertices();

            // Load textures
            (this.tilesetTexture, this.tilesetTextureView) = this.Content.LoadTexture(factory, "assets/tileset.png");
            this.tileset = new Tileset(this.tilesetTexture.Width, this.tilesetTexture.Height);

            // Load shaders
            this.vertexShader = this.Content.LoadShader(factory, "Basic", ShaderStages.Vertex, "VS");
            this.fragmentShader = this.Content.LoadShader(factory, "Basic", ShaderStages.Fragment, "FS");

            // Create constant buffer
            var projectionMatrixBufferDescription = new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic);
            this.projectionMatrixBuffer = factory.CreateBuffer(projectionMatrixBufferDescription);

            // Create vertex buffer
            var vertexBufferDescription = new BufferDescription((uint)this.tilemapVertexData.Length * VertexPositionTextureColor.SizeInBytes, BufferUsage.VertexBuffer | BufferUsage.Dynamic);
            this.vertexBuffer = factory.CreateBuffer(vertexBufferDescription);
            this.GraphicsDevice.UpdateBuffer(this.vertexBuffer, 0, this.tilemapVertexData);

            // Create index buffer
            var indexBufferDescription = new BufferDescription((uint)this.tilemapIndexData.Length * sizeof(ushort), BufferUsage.IndexBuffer);
            this.indexBuffer = factory.CreateBuffer(indexBufferDescription);
            this.GraphicsDevice.UpdateBuffer(this.indexBuffer, 0, this.tilemapIndexData);

            // Create layouts
            var vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                new VertexElementDescription("Texture", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float4));

            this.mainLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("SpriteSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            this.textureLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("SpriteTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment)));

            // Create pipeline
            var pipelineDescription = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.Disabled,
                RasterizerState = RasterizerStateDescription.CullNone,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ResourceLayouts = new[] { this.mainLayout, this.textureLayout },
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new[] { vertexLayout },
                    shaders: new[] { this.vertexShader, this.fragmentShader }),
                Outputs = this.GraphicsDevice.SwapchainFramebuffer.OutputDescription,
            };

            this.pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

            // Create resource sets
            this.tilesetResourceSet = factory.CreateResourceSet(
                new ResourceSetDescription(
                    this.textureLayout,
                    this.tilesetTextureView));

            this.mainResourceSet = factory.CreateResourceSet(
                new ResourceSetDescription(
                    this.mainLayout,
                    this.projectionMatrixBuffer,
                    this.GraphicsDevice.PointSampler));

            // Create command list
            this.commandList = factory.CreateCommandList();
        }

        /// <inheritdoc />
        protected override void FreeResources()
        {
            // NOTE: The ResourceFactory we used for most resources actually keeps track of the created resources so the clean-up is automatic
            this.Chunks.Dispose();
        }

        /// <inheritdoc />
        protected override void Draw(float deltaSeconds)
        {
            this.commandList.Begin();

            this.commandList.SetFramebuffer(this.GraphicsDevice.SwapchainFramebuffer);
            this.commandList.ClearColorTarget(0, RgbaFloat.Red);

            var matrix = Matrix4x4.CreateOrthographicOffCenter(
                0f,
                this.GraphicsDevice.SwapchainFramebuffer.Width,
                this.GraphicsDevice.SwapchainFramebuffer.Height,
                0f,
                -1f,
                1f);

            this.GraphicsDevice.UpdateBuffer(this.projectionMatrixBuffer, 0, ref matrix);

            // Determine which tiles to render and update the vertex buffer
            var camX = (int)this.cameraX;
            var camY = (int)this.cameraY;

            var renderOffsetX = -ModFloor(camX, Configuration.TileSize);
            var renderOffsetY = -ModFloor(camY, Configuration.TileSize);

            for (var y = 0; y < Configuration.ChunkHeight + 1; y++)
            {
                var tileY = DivFloor(camY, Configuration.TileSize) + y;
                var chunkY = DivFloor(tileY, Configuration.ChunkHeight);
                var chunkLocalY = tileY - chunkY * Configuration.ChunkHeight;

                for (var x = 0; x < Configuration.ChunkWidth + 1; x++)
                {
                    var tileX = DivFloor(camX, Configuration.TileSize) + x;
                    var chunkX = DivFloor(tileX, Configuration.ChunkWidth);
                    var chunkLocalX = tileX - chunkX * Configuration.ChunkWidth;

                    var chunkLocalIndex = chunkLocalY * Configuration.ChunkWidth + chunkLocalX;

                    // Tile=0 is considered 'no value' (black) and positive values as 1-based tile index
                    var tile = this.Chunks.GetChunkData(chunkX, chunkY)[chunkLocalIndex];
                    var uv = this.tileset.GetTileUV(tile - 1);
                    var color = new RgbaFloat(new Vector4(tile == 0 ? 0f : 1f));

                    var vindex = 4 * (y * (Configuration.ChunkWidth + 1) + x);

                    this.tilemapVertexData[vindex].Position = new Vector2(x * Configuration.TileSize + renderOffsetX, y * Configuration.TileSize + renderOffsetY);
                    this.tilemapVertexData[vindex].UV = new Vector2(uv.topLeft.X, uv.topLeft.Y);
                    this.tilemapVertexData[vindex].Color = color;

                    this.tilemapVertexData[vindex + 1].Position = new Vector2((x + 1) * Configuration.TileSize + renderOffsetX, y * Configuration.TileSize + renderOffsetY);
                    this.tilemapVertexData[vindex + 1].UV = new Vector2(uv.bottomRight.X, uv.topLeft.Y);
                    this.tilemapVertexData[vindex + 1].Color = color;

                    this.tilemapVertexData[vindex + 2].Position = new Vector2(x * Configuration.TileSize + renderOffsetX, (y + 1) * Configuration.TileSize + renderOffsetY);
                    this.tilemapVertexData[vindex + 2].UV = new Vector2(uv.topLeft.X, uv.bottomRight.Y);
                    this.tilemapVertexData[vindex + 2].Color = color;

                    this.tilemapVertexData[vindex + 3].Position = new Vector2((x + 1) * Configuration.TileSize + renderOffsetX, (y + 1) * Configuration.TileSize + renderOffsetY);
                    this.tilemapVertexData[vindex + 3].UV = new Vector2(uv.bottomRight.X, uv.bottomRight.Y);
                    this.tilemapVertexData[vindex + 3].Color = color;
                }
            }

            this.GraphicsDevice.UpdateBuffer(this.vertexBuffer, 0, this.tilemapVertexData);

            this.commandList.SetPipeline(this.pipeline);

            this.commandList.SetVertexBuffer(0, this.vertexBuffer);
            this.commandList.SetIndexBuffer(this.indexBuffer, IndexFormat.UInt16);
            this.commandList.SetGraphicsResourceSet(0, this.mainResourceSet);
            this.commandList.SetGraphicsResourceSet(1, this.tilesetResourceSet);

            this.commandList.DrawIndexed(
                indexCount: (uint)this.tilemapIndexData.Length,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);

            this.commandList.End();
            this.GraphicsDevice.SubmitCommands(this.commandList);

            this.GraphicsDevice.SwapBuffers();
        }

        /// <inheritdoc />
        protected override void Update(float deltaSeconds)
        {
            // Ensure that just loaded chunk data is complete
            this.Chunks.ApplyAllPendingChanges();

            // Hold the Shift key to increase camera speed
            var speed = InputTracker.GetKey(Key.ShiftLeft) || InputTracker.GetKey(Key.ShiftRight) ? 4f : 1f;

            if (InputTracker.GetKey(Key.Left))
            {
                this.cameraX -= 60 * deltaSeconds * speed;
            }

            if (InputTracker.GetKey(Key.Right))
            {
                this.cameraX += 60 * deltaSeconds * speed;
            }

            if (InputTracker.GetKey(Key.Up))
            {
                this.cameraY -= 60 * deltaSeconds * speed;
            }

            if (InputTracker.GetKey(Key.Down))
            {
                this.cameraY += 60 * deltaSeconds * speed;
            }

            // Q = Reset camera to chunk (0, 0)
            if (InputTracker.GetKeyDown(Key.Q))
            {
                this.cameraX = 0;
                this.cameraY = 0;
            }

            // W = Some special camera position
            if (InputTracker.GetKeyDown(Key.W))
            {
                this.cameraX = -288 * 10;
                this.cameraY = -288;
            }

            // Set the chunk manager's "camera center"
            var chunkX = DivFloor((int)(this.cameraX + this.Window.Width / 2.0), Configuration.TileSize * Configuration.ChunkWidth);
            var chunkY = DivFloor((int)(this.cameraY + this.Window.Height / 2.0), Configuration.TileSize * Configuration.ChunkHeight);

            this.Chunks.SetCameraAt(chunkX, chunkY);
        }

        /// <inheritdoc />
        protected override void OnKeyDown(KeyEvent state)
        {
            if (state.Key == Key.Escape)
            {
                this.Exit();
            }
        }

        /// <summary>
        /// Generates the tilemap vertices.
        /// </summary>
        /// <returns>Generates the vertex and index data for the grid used for rendering the tilemap.</returns>
        private static (VertexPositionTextureColor[] vertexData, ushort[] indexData) GenerateTilemapVertices()
        {
            var iindex = 0;
            var vertexData = new VertexPositionTextureColor[4 * (Configuration.ChunkWidth + 1) * (Configuration.ChunkHeight + 1)];
            var indexData = new ushort[6 * (Configuration.ChunkWidth + 1) * (Configuration.ChunkHeight + 1)];
            for (var y = 0; y < Configuration.ChunkHeight + 1; y++)
            {
                for (var x = 0; x < Configuration.ChunkWidth + 1; x++)
                {
                    var vindex = 4 * (y * (Configuration.ChunkWidth + 1) + x);
                    vertexData[vindex] = new VertexPositionTextureColor(new Vector2(x * Configuration.TileSize, y * Configuration.TileSize), new Vector2(0f, 0f));
                    vertexData[vindex + 1] = new VertexPositionTextureColor(new Vector2((x + 1) * Configuration.TileSize, y * Configuration.TileSize), new Vector2(1f, 0f));
                    vertexData[vindex + 2] = new VertexPositionTextureColor(new Vector2(x * Configuration.TileSize, (y + 1) * Configuration.TileSize), new Vector2(0f, 1f));
                    vertexData[vindex + 3] = new VertexPositionTextureColor(new Vector2((x + 1) * Configuration.TileSize, (y + 1) * Configuration.TileSize), new Vector2(1f, 1f));

                    indexData[iindex++] = (ushort)vindex;
                    indexData[iindex++] = (ushort)(vindex + 1);
                    indexData[iindex++] = (ushort)(vindex + 2);
                    indexData[iindex++] = (ushort)(vindex + 2);
                    indexData[iindex++] = (ushort)(vindex + 1);
                    indexData[iindex++] = (ushort)(vindex + 3);
                }
            }

            return (vertexData, indexData);
        }

        /// <summary>
        /// Defines a vertex point that contains Position, Texture, and Color info.
        /// </summary>
        private struct VertexPositionTextureColor
        {
            /// <summary>
            /// The size of this structure, in bytes.
            /// </summary>
            public const uint SizeInBytes = 32;

            /// <summary>
            /// The position.
            /// </summary>
            public Vector2 Position;

            /// <summary>
            /// The texture coordinates.
            /// </summary>
            public Vector2 UV;

            /// <summary>
            /// The color.
            /// </summary>
            public RgbaFloat Color;

            /// <summary>
            /// Initializes a new instance of the <see cref="VertexPositionTextureColor"/> struct.
            /// </summary>
            /// <param name="position">The position.</param>
            /// <param name="uv">The texture coordinates.</param>
            /// <param name="color">The color.</param>
            public VertexPositionTextureColor(Vector2 position, Vector2 uv, RgbaFloat color)
            {
                this.Position = position;
                this.UV = uv;
                this.Color = color;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="VertexPositionTextureColor"/> struct.
            /// </summary>
            /// <param name="position">The position.</param>
            /// <param name="uv">The texture coordinates.</param>
            public VertexPositionTextureColor(Vector2 position, Vector2 uv)
                : this(position, uv, RgbaFloat.White)
            {
            }
        }
    }
}

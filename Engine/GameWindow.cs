namespace Engine
{
    using System;
    using System.Diagnostics;

    using Veldrid;
    using Veldrid.Sdl2;
    using Veldrid.StartupUtilities;
    using Veldrid.Utilities;

    /// <summary>
    /// The game window.
    /// </summary>
    /// <seealso cref="Engine.IGameWindow" />
    public class GameWindow : IGameWindow
    {
        /// <summary>
        /// The SDL window.
        /// </summary>
        private readonly Sdl2Window window;

        /// <summary>
        /// The graphics device.
        /// </summary>
        private GraphicsDevice graphicsDevice;

        /// <summary>
        /// The resource factory.
        /// </summary>
        private DisposeCollectorResourceFactory resourceFactory;

        /// <summary>
        /// Temporarily stores a flag indicating whether the window has just been resized.
        /// </summary>
        private bool windowResized = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameWindow"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        public GameWindow(string title)
        {
            var windowCreateInfo = new WindowCreateInfo
            {
                X = Sdl2Native.SDL_WINDOWPOS_CENTERED,
                Y = Sdl2Native.SDL_WINDOWPOS_CENTERED,
                WindowWidth = Configuration.ChunkWidth * Configuration.TileSize,
                WindowHeight = Configuration.ChunkHeight * Configuration.TileSize,
                WindowTitle = title,
            };

            this.window = VeldridStartup.CreateWindow(ref windowCreateInfo);
            this.window.Resizable = false;
            this.window.Resized += () => { this.windowResized = true; };
            this.window.KeyDown += this.OnKeyDown;
        }

        /// <inheritdoc />
        public event Action<float> Rendering;

        /// <inheritdoc />
        public event Action<GraphicsDevice, ResourceFactory, Swapchain> GraphicsDeviceCreated;

        /// <inheritdoc />
        public event Action GraphicsDeviceDestroyed;

        /// <inheritdoc />
        public event Action Resized;

        /// <inheritdoc />
        public event Action<KeyEvent> KeyPressed;

        /// <inheritdoc />
        public uint Width => (uint)this.window.Width;

        /// <inheritdoc />
        public uint Height => (uint)this.window.Height;

        /// <inheritdoc />
        public string Title
        {
            get => this.window.Title;
            set => this.window.Title = value;
        }

        /// <inheritdoc />
        public void Close() => this.window.Close();

        /// <inheritdoc />
        public void Run(GraphicsBackend graphicsAPI = GraphicsBackend.Direct3D11)
        {
            var options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: null,
                syncToVerticalBlank: true,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: false,
                preferStandardClipSpaceYDirection: false);
#if DEBUG
            options.Debug = true;
#endif
            this.graphicsDevice = VeldridStartup.CreateGraphicsDevice(this.window, options, graphicsAPI);
            this.resourceFactory = new DisposeCollectorResourceFactory(this.graphicsDevice.ResourceFactory);
            this.GraphicsDeviceCreated?.Invoke(this.graphicsDevice, this.resourceFactory, this.graphicsDevice.MainSwapchain);

            var timer = Stopwatch.StartNew();
            var previousElapsed = timer.Elapsed.TotalSeconds;

            while (this.window.Exists)
            {
                var newElapsed = timer.Elapsed.TotalSeconds;
                var deltaSeconds = (float)(newElapsed - previousElapsed);

                var inputSnapshot = this.window.PumpEvents();
                InputTracker.UpdateFrameInput(inputSnapshot);

                if (this.window.Exists)
                {
                    previousElapsed = newElapsed;
                    if (this.windowResized)
                    {
                        this.windowResized = false;

                        this.graphicsDevice.ResizeMainWindow((uint)this.window.Width, (uint)this.window.Height);
                        this.Resized?.Invoke();
                    }

                    this.Rendering?.Invoke(deltaSeconds);
                }
            }

            this.graphicsDevice.WaitForIdle();
            this.resourceFactory.DisposeCollector.DisposeAll();
            this.graphicsDevice.Dispose();
            this.GraphicsDeviceDestroyed?.Invoke();
        }

        /// <summary>
        /// Called when a key has been pressed.
        /// </summary>
        /// <param name="keyEvent">The key event.</param>
        protected void OnKeyDown(KeyEvent keyEvent)
        {
            this.KeyPressed?.Invoke(keyEvent);
        }
    }
}

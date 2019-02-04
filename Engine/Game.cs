namespace Engine
{
    using System;
    using System.IO;

    using Veldrid;

    /// <summary>
    /// The game base class.
    /// </summary>
    public abstract class Game
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <exception cref="ArgumentException">The <paramref name="window"/> cannot be <c>null</c>.</exception>
        protected Game(IGameWindow window)
        {
            this.Window = window ?? throw new ArgumentNullException(nameof(window), $"The {nameof(window)} cannot be null.");

            this.Window.Resized += () => this.OnWindowResized(this.Window.Width, this.Window.Height);
            this.Window.GraphicsDeviceCreated += this.OnGraphicsDeviceCreated;
            this.Window.GraphicsDeviceDestroyed += this.OnDeviceDestroyed;
            this.Window.Rendering += this.Update;
            this.Window.Rendering += this.Draw;
            this.Window.KeyPressed += this.OnKeyDown;
        }

        /// <summary>
        /// Gets the window.
        /// </summary>
        public IGameWindow Window { get; }

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the resource factory.
        /// </summary>
        public ResourceFactory ResourceFactory { get; private set; }

        /// <summary>
        /// Gets the swapchain.
        /// </summary>
        public Swapchain Swapchain { get; private set; }

        /// <summary>
        /// Opens the embedded asset stream.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <returns>The <see cref="Stream"/>.</returns>
        public Stream OpenEmbeddedAssetStream(string name) => this.GetType().Assembly.GetManifestResourceStream(name);

        /// <summary>
        /// Reads the embedded asset into a byte array.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <returns>The byte array.</returns>
        public byte[] ReadEmbeddedAssetBytes(string name)
        {
            using (var stream = this.OpenEmbeddedAssetStream(name))
            {
                var bytes = new byte[stream.Length];
                using (var memory = new MemoryStream(bytes))
                {
                    stream.CopyTo(memory);
                    return bytes;
                }
            }
        }

        /// <summary>
        /// Loads the game resources.
        /// </summary>
        /// <param name="factory">The resource factory.</param>
        protected abstract void CreateResources(ResourceFactory factory);

        /// <summary>
        /// Frees all allocated resources.
        /// </summary>
        protected abstract void FreeResources();

        /// <summary>
        /// Updates the game state before rendering.
        /// </summary>
        /// <param name="deltaSeconds">The elapsed seconds since the last frame.</param>
        protected virtual void Update(float deltaSeconds)
        {
            // Override in the derived class.
        }

        /// <summary>
        /// Renders the game.
        /// </summary>
        /// <param name="deltaSeconds">The elapsed seconds since the last frame.</param>
        protected abstract void Draw(float deltaSeconds);

        /// <summary>
        /// Called when the window has been resized.
        /// </summary>
        /// <param name="width">The new pixel width.</param>
        /// <param name="height">The new pixel height.</param>
        protected virtual void OnWindowResized(uint width, uint height)
        {
            // Override in a derived class.
        }

        /// <summary>
        /// Called when a key has been pressed down.
        /// </summary>
        /// <param name="state">The key state info.</param>
        protected virtual void OnKeyDown(KeyEvent state)
        {
            // Override in a derived class.
        }

        /// <summary>
        /// Closes the application.
        /// </summary>
        protected void Exit()
        {
            this.Window?.Close();
        }

        /// <summary>
        /// Called when the graphics device has been created.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="factory">The resource factory.</param>
        /// <param name="swapchain">The swapchain.</param>
        private void OnGraphicsDeviceCreated(GraphicsDevice device, ResourceFactory factory, Swapchain swapchain)
        {
            this.GraphicsDevice = device;
            this.ResourceFactory = factory;
            this.Swapchain = swapchain;
            this.CreateResources(factory);
        }

        /// <summary>
        /// Called when the device has been destroyed.
        /// </summary>
        private void OnDeviceDestroyed()
        {
            this.FreeResources();
            this.GraphicsDevice = null;
            this.ResourceFactory = null;
            this.Swapchain = null;
        }
    }
}

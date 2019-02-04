namespace Engine
{
    using System;

    using Veldrid;

    /// <summary>
    /// The game window.
    /// </summary>
    public interface IGameWindow
    {
        /// <summary>
        /// Occurs when the game is rendering.
        /// </summary>
        event Action<float> Rendering;

        /// <summary>
        /// Occurs when the graphics device has been created.
        /// </summary>
        event Action<GraphicsDevice, ResourceFactory, Swapchain> GraphicsDeviceCreated;

        /// <summary>
        /// Occurs when the graphics device has been destroyed.
        /// </summary>
        event Action GraphicsDeviceDestroyed;

        /// <summary>
        /// Occurs when the game window has been resized (and the buffers might need resized as well).
        /// </summary>
        event Action Resized;

        /// <summary>
        /// Occurs when a key has been pressed.
        /// </summary>
        event Action<KeyEvent> KeyPressed;

        /// <summary>
        /// Gets the window width, in pixels.
        /// </summary>
        uint Width { get; }

        /// <summary>
        /// Gets the window height, in pixels.
        /// </summary>
        uint Height { get; }

        /// <summary>
        /// Gets or sets the window title.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Closes the window.
        /// </summary>
        void Close();

        /// <summary>
        /// Runs the game.
        /// </summary>
        /// <param name="graphicsAPI">The graphics API.</param>
        void Run(GraphicsBackend graphicsAPI);
    }
}

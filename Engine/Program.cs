namespace Engine
{
    using Veldrid;

    /// <summary>
    /// The demo app.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            var window = new GameWindow("Streaming Tilemap");
            var game = new SampleGame(window);
            window.Run(GraphicsBackend.Direct3D11);
        }
    }
}

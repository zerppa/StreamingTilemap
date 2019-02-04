namespace Editor
{
    using System;

    /// <summary>
    /// Base class for view models.
    /// </summary>
    /// <seealso cref="Editor.BindingSource" />
    public class ViewModel : BindingSource
    {
        /// <summary>
        /// The singleton context factory.
        /// </summary>
        private static readonly Lazy<Context> SharedContext = new Lazy<Context>(() => new Context());

        /// <summary>
        /// Gets the context.
        /// </summary>
        protected Context Context => SharedContext.Value;
    }
}

namespace Editor
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides common extension methods.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Adds the items into the target collection.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="destination">The destination collection.</param>
        /// <param name="source">The items to add.</param>
        public static void AddRange<T>(this ICollection<T> destination, IEnumerable<T> source)
        {
            if (destination is List<T> list)
            {
                list.AddRange(source);
            }
            else
            {
                foreach (var item in source)
                {
                    destination.Add(item);
                }
            }
        }
    }
}

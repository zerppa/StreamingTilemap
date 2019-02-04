namespace Editor
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides common extension methods.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Performs an action for each element in the enumerable sequence.
        /// </summary>
        /// <typeparam name="T">The sequence element type.</typeparam>
        /// <param name="sequence">The enumerable sequence.</param>
        /// <param name="action">The delegate to a method that performs the action for an element.</param>
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (var element in sequence)
            {
                action(element);
            }
        }

        /// <summary>
        /// Performs an action for each element in the enumerable sequence.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="sequence">The enumerable sequence.</param>
        /// <param name="action">The delegate to a method that performs the action for an element.</param>
        public static void ForEach<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> sequence, Action<TKey, TValue> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (var element in sequence)
            {
                action(element.Key, element.Value);
            }
        }

        /// <summary>
        /// Performs an action for each element in the enumerable sequence.
        /// </summary>
        /// <typeparam name="T1">The type of the first item.</typeparam>
        /// <param name="sequence">The enumerable sequence.</param>
        /// <param name="action">The delegate to a method that performs the action for an element.</param>
        public static void ForEach<T1>(this IEnumerable<Tuple<T1>> sequence, Action<T1> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (var element in sequence)
            {
                action(element.Item1);
            }
        }

        /// <summary>
        /// Performs an action for each element in the enumerable sequence.
        /// </summary>
        /// <typeparam name="T1">The type of the first item.</typeparam>
        /// <typeparam name="T2">The type of the second item.</typeparam>
        /// <param name="sequence">The enumerable sequence.</param>
        /// <param name="action">The delegate to a method that performs the action for an element.</param>
        public static void ForEach<T1, T2>(this IEnumerable<Tuple<T1, T2>> sequence, Action<T1, T2> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (var (item1, item2) in sequence)
            {
                action(item1, item2);
            }
        }

        /// <summary>
        /// Performs an action for each element in the enumerable sequence.
        /// </summary>
        /// <typeparam name="T1">The type of the first item.</typeparam>
        /// <typeparam name="T2">The type of the second item.</typeparam>
        /// <typeparam name="T3">The type of the third item.</typeparam>
        /// <param name="sequence">The enumerable sequence.</param>
        /// <param name="action">The delegate to a method that performs the action for an element.</param>
        public static void ForEach<T1, T2, T3>(this IEnumerable<Tuple<T1, T2, T3>> sequence, Action<T1, T2, T3> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (var (item1, item2, item3) in sequence)
            {
                action(item1, item2, item3);
            }
        }

        /// <summary>
        /// Performs an action for each element in the enumerable sequence.
        /// </summary>
        /// <typeparam name="T1">The type of the first item.</typeparam>
        /// <typeparam name="T2">The type of the second item.</typeparam>
        /// <typeparam name="T3">The type of the third item.</typeparam>
        /// <typeparam name="T4">The type of the fourth item.</typeparam>
        /// <param name="sequence">The enumerable sequence.</param>
        /// <param name="action">The delegate to a method that performs the action for an element.</param>
        public static void ForEach<T1, T2, T3, T4>(this IEnumerable<Tuple<T1, T2, T3, T4>> sequence, Action<T1, T2, T3, T4> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (var (item1, item2, item3, item4) in sequence)
            {
                action(item1, item2, item3, item4);
            }
        }

        /// <summary>
        /// Performs an action for each element in the enumerable sequence.
        /// </summary>
        /// <typeparam name="T1">The type of the first item.</typeparam>
        /// <typeparam name="T2">The type of the second item.</typeparam>
        /// <typeparam name="T3">The type of the third item.</typeparam>
        /// <typeparam name="T4">The type of the fourth item.</typeparam>
        /// <typeparam name="T5">The type of the fifth item.</typeparam>
        /// <param name="sequence">The enumerable sequence.</param>
        /// <param name="action">The delegate to a method that performs the action for an element.</param>
        public static void ForEach<T1, T2, T3, T4, T5>(this IEnumerable<Tuple<T1, T2, T3, T4, T5>> sequence, Action<T1, T2, T3, T4, T5> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (var (item1, item2, item3, item4, item5) in sequence)
            {
                action(item1, item2, item3, item4, item5);
            }
        }

        /// <summary>
        /// Performs an action for each element in the enumerable sequence.
        /// </summary>
        /// <typeparam name="T1">The type of the first item.</typeparam>
        /// <typeparam name="T2">The type of the second item.</typeparam>
        /// <typeparam name="T3">The type of the third item.</typeparam>
        /// <typeparam name="T4">The type of the fourth item.</typeparam>
        /// <typeparam name="T5">The type of the fifth item.</typeparam>
        /// <typeparam name="T6">The type of the sixth item.</typeparam>
        /// <param name="sequence">The enumerable sequence.</param>
        /// <param name="action">The delegate to a method that performs the action for an element.</param>
        public static void ForEach<T1, T2, T3, T4, T5, T6>(this IEnumerable<Tuple<T1, T2, T3, T4, T5, T6>> sequence, Action<T1, T2, T3, T4, T5, T6> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (var (item1, item2, item3, item4, item5, item6) in sequence)
            {
                action(item1, item2, item3, item4, item5, item6);
            }
        }

        /// <summary>
        /// Performs an action for each element in the enumerable sequence.
        /// </summary>
        /// <typeparam name="T1">The type of the first item.</typeparam>
        /// <typeparam name="T2">The type of the second item.</typeparam>
        /// <typeparam name="T3">The type of the third item.</typeparam>
        /// <typeparam name="T4">The type of the fourth item.</typeparam>
        /// <typeparam name="T5">The type of the fifth item.</typeparam>
        /// <typeparam name="T6">The type of the sixth item.</typeparam>
        /// <typeparam name="T7">The type of the seventh item.</typeparam>
        /// <param name="sequence">The enumerable sequence.</param>
        /// <param name="action">The delegate to a method that performs the action for an element.</param>
        public static void ForEach<T1, T2, T3, T4, T5, T6, T7>(this IEnumerable<Tuple<T1, T2, T3, T4, T5, T6, T7>> sequence, Action<T1, T2, T3, T4, T5, T6, T7> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (var (item1, item2, item3, item4, item5, item6, item7) in sequence)
            {
                action(item1, item2, item3, item4, item5, item6, item7);
            }
        }

        /// <summary>
        /// Performs an action for each element in the enumerable sequence.
        /// </summary>
        /// <typeparam name="T1">The type of the first item.</typeparam>
        /// <typeparam name="T2">The type of the second item.</typeparam>
        /// <typeparam name="T3">The type of the third item.</typeparam>
        /// <typeparam name="T4">The type of the fourth item.</typeparam>
        /// <typeparam name="T5">The type of the fifth item.</typeparam>
        /// <typeparam name="T6">The type of the sixth item.</typeparam>
        /// <typeparam name="T7">The type of the seventh item.</typeparam>
        /// <typeparam name="T8">The type of the eight item.</typeparam>
        /// <param name="sequence">The enumerable sequence.</param>
        /// <param name="action">The delegate to a method that performs the action for an element.</param>
        public static void ForEach<T1, T2, T3, T4, T5, T6, T7, T8>(this IEnumerable<Tuple<T1, T2, T3, T4, T5, T6, T7, T8>> sequence, Action<T1, T2, T3, T4, T5, T6, T7, T8> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (var element in sequence)
            {
                action(element.Item1, element.Item2, element.Item3, element.Item4, element.Item5, element.Item6, element.Item7, element.Rest);
            }
        }
    }
}

namespace Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Implements a pool of recyclable resources.
    /// </summary>
    /// <typeparam name="T">The object type>.</typeparam>
    public class Pool<T>
        where T : new()
    {
        /// <summary>
        /// Stores the internal object instance reserve.
        /// </summary>
        private readonly Queue<T> pool;

        /// <summary>
        /// Keeps track of the withdrawn objects.
        /// </summary>
        private readonly HashSet<T> borrowedItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pool{T}"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <exception cref="ArgumentException">The <paramref name="capacity"/> must be greater than zero.</exception>
        public Pool(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException($"{nameof(capacity)} must be greater than zero", nameof(capacity));
            }

            this.pool = new Queue<T>(capacity);
            this.borrowedItems = new HashSet<T>(capacity);
            foreach (var _ in Enumerable.Range(1, capacity))
            {
                this.pool.Enqueue(new T());
            }
        }

        /// <summary>
        /// Gets a value indicating whether the pool has any items left for withdrawal.
        /// </summary>
        public bool CanWithdraw => this.pool.Count > 0;

        /// <summary>
        /// Pulls a free item from the pool.
        /// </summary>
        /// <returns>The item.</returns>
        /// <exception cref="InvalidOperationException">The pool is exhausted.</exception>
        public T Withdraw()
        {
            if (!this.CanWithdraw)
            {
                throw new InvalidOperationException("The pool is exhausted.");
            }

            var availableItem = this.pool.Dequeue();
            this.borrowedItems.Add(availableItem);
            return availableItem;
        }

        /// <summary>
        /// Returns the specified item back to the pool.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <exception cref="InvalidOperationException">The returned item did not belong to the pool.</exception>
        public void Return(T item)
        {
            if (this.borrowedItems.Remove(item))
            {
                this.pool.Enqueue(item);
            }
            else
            {
                throw new InvalidOperationException("The returned item did not belong to the pool.");
            }
        }
    }
}

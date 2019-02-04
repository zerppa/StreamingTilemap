namespace Editor
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides data binding support.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public abstract class BindingSource : INotifyPropertyChanged
    {
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The property name. If not specified, defaults to the caller member name.</param>
        public virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName1">The first property name.</param>
        /// <param name="propertyName2">The second property name.</param>
        /// <param name="propertyNames">The other property names.</param>
        public void RaisePropertyChanged(string propertyName1, string propertyName2, params string[] propertyNames)
        {
            if (propertyName1 == null)
            {
                throw new ArgumentNullException(nameof(propertyName1));
            }

            if (propertyName2 == null)
            {
                throw new ArgumentNullException(nameof(propertyName2));
            }

            if (propertyNames == null)
            {
                throw new ArgumentNullException(nameof(propertyNames));
            }

            this.RaisePropertyChanged(propertyName1);
            this.RaisePropertyChanged(propertyName2);

            foreach (var propertyName in propertyNames)
            {
                this.RaisePropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Sets the property value. Raises the <see cref="PropertyChanged" /> event, if needed.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="field">The property backing field.</param>
        /// <param name="value">The new property value.</param>
        /// <param name="propertyName">The property name. If not specified, defaults to the caller member name.</param>
        /// <returns>True if the property value was modified, otherwise false.</returns>
        protected bool SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (!object.Equals(field, value))
            {
                field = value;
                this.RaisePropertyChanged(propertyName);
                return true;
            }

            return false;
        }
    }
}

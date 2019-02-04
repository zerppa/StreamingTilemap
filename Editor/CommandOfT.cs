namespace Editor
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// The <see cref="ICommand" /> implementation.
    /// </summary>
    /// <typeparam name="T">The command parameter's type.</typeparam>
    /// <seealso cref="System.Windows.Input.ICommand" />
    public class Command<T> : ICommand
    {
        /// <summary>
        /// Stores the execute delegate.
        /// </summary>
        private readonly Action<T> execute;

        /// <summary>
        /// Stores the can execute delegate.
        /// </summary>
        private readonly Func<T, bool> canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="Command{T}"/> class.
        /// </summary>
        /// <param name="execute">The execute delegate.</param>
        /// <param name="canExecute">The can execute delegate.</param>
        public Command(Action<T> execute, Func<T, bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Command{T}"/> class.
        /// </summary>
        /// <param name="execute">The execute delegate.</param>
        public Command(Action<T> execute)
            : this(execute, null)
        {
        }

        /// <inheritdoc />
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <inheritdoc />
        public bool CanExecute(object parameter)
        {
            return this.canExecute?.Invoke((T)parameter) ?? false;
        }

        /// <inheritdoc />
        public void Execute(object parameter)
        {
            this.execute?.Invoke((T)parameter);
        }
    }
}

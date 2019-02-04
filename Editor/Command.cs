namespace Editor
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// The <see cref="ICommand"/> implementation.
    /// </summary>
    /// <seealso cref="System.Windows.Input.ICommand" />
    public class Command : ICommand
    {
        /// <summary>
        /// Stores the execute delegate.
        /// </summary>
        private readonly Action execute;

        /// <summary>
        /// Stores the can execute delegate.
        /// </summary>
        private readonly Func<bool> canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="execute">The execute delegate.</param>
        /// <param name="canExecute">The can execute delegate.</param>
        public Command(Action execute, Func<bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="execute">The execute delegate.</param>
        public Command(Action execute)
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
            return this.canExecute == null || this.canExecute.Invoke();
        }

        /// <inheritdoc />
        public void Execute(object parameter)
        {
            this.execute.Invoke();
        }
    }
}

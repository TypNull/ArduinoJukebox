using System;
using System.Windows.Input;

namespace ArduinoJukebox.Core
{
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// Action that will be excecuted
        /// </summary>
        private readonly Action<object> _execute;
        /// <summary>
        /// Condition if the Command can be ececuted
        /// </summary>
        private readonly Func<object, bool>? _canExecute;

        /// <summary>
        /// An Event that will raise if the value from CanExecute changed
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Raises the CanExecuteChanged event
        /// </summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());

        /// <summary>
        /// Creates a RelayCommand for ICommand that can be executed from the GUI
        /// </summary>
        /// <param name="execute">Action that will be executed</param>
        /// <param name="canExecute">Contition if the Action can be executed</param>
        public RelayCommand(Action<object> execute, Func<object, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Returns a value the indicated if the Command can be executed
        /// </summary>
        /// <param name="parameter">Parameter that can change the Condition</param>
        /// <returns>A boolean with the value true if it can be executed</returns>
        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter!);

        /// <summary>
        /// Executes the Action
        /// </summary>
        /// <param name="parameter">A Parameter that can be overgiven</param>
        public void Execute(object? parameter) => _execute(parameter!);

    }
}

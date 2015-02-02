using System;
using System.Windows.Input;

namespace geoecpApp.Model
{
	public class RelayCommand : ICommand
	{
		#region Fields readonly

		/// <summary>
		///     The predicate defining if execute is possible.
		/// </summary>
		private readonly Predicate<object> _canExecute;

		/// <summary>
		///     The execute action of the command..
		/// </summary>
		private readonly Action<object> _execute;

		#endregion // Fields

		#region Constructors public

		/// <summary>
		///     Initializes a new instance of the <see cref="RelayCommand" /> class.
		/// </summary>
		/// <param name="execute">The execute action of the command.</param>
		/// <param name="canExecute"></param>
		public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
		{
			if (execute == null) throw new ArgumentNullException("execute");
			_execute = execute;
			_canExecute = canExecute;
		}

		#endregion // Constructors

		#region ICommand Members

		/// <summary>
		///     Gets a value indicating wether the command can be executed or not.
		/// </summary>
		/// <param name="parameter">Parameter object.</param>
		/// <returns>True if execute is allowed, false otherwise.</returns>
		public bool CanExecute(object parameter)
		{
			return _canExecute == null || _canExecute.Invoke(parameter);
		}

		/// <summary>
		///     Adds or removes an event handler for the can execute changed event.
		/// </summary>
		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		/// <summary>
		///     Executes the defined action.
		/// </summary>
		/// <param name="parameter">The parameter for the action.</param>
		public void Execute(object parameter)
		{
			_execute.Invoke(parameter);
		}

		#endregion // ICommand Members
	}
}
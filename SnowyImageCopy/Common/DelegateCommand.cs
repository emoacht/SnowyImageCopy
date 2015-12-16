using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SnowyImageCopy.Common
{
	public class DelegateCommand : ICommand
	{
		private readonly Action _execute;
		private readonly Func<bool> _canExecute;

		#region Constructor

		public DelegateCommand(Action execute)
			: this(execute, () => true)
		{ }

		public DelegateCommand(Action execute, Func<bool> canExecute)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");
			if (canExecute == null)
				throw new ArgumentNullException("canExecute");

			this._execute = execute;
			this._canExecute = canExecute;
		}

		#endregion

		#region Execute

		public void Execute()
		{
			this._execute();
		}

		void ICommand.Execute(object parameter)
		{
			this.Execute();
		}

		#endregion

		#region CanExecute

		public bool CanExecute()
		{
			return this._canExecute();
		}

		bool ICommand.CanExecute(object parameter)
		{
			return this.CanExecute();
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public static void RaiseCanExecuteChanged() // To be considered.
		{
			CommandManager.InvalidateRequerySuggested();
		}

		#endregion
	}
}
using System;
using System.Windows.Input;

namespace DriveFileSearcher.Helpers
{
    public class DelegateCommand : ICommand
    {
        readonly Predicate<object>? canExecute;
        readonly Action<object>? execute;

        public DelegateCommand(Predicate<object> _canexecute, Action<object> _execute)
            : this()
        {
            canExecute = _canexecute;
            execute = _execute;
        }
        public DelegateCommand()
        {

        }
        public bool CanExecute(object? parameter)
        {
            if (canExecute == null)
            {
                return true;
            }

            return canExecute(parameter!);
        }

        public event EventHandler? CanExecuteChanged;

        public void Execute(object? parameter)
        {
            if (execute != null)
                execute(parameter!);
        }
    }
}

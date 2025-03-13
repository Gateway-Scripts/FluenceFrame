using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FluenceFrame.Services
{
    public class ViewModelBase:INotifyPropertyChanged
    {
        // The PropertyChanged event is defined by the INotifyPropertyChanged interface.
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is used to notify when a property value changes.
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // This method sets the value of a property and raises the PropertyChanged event if the value changes.
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            // Check if the new value is different from the old value.
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            // Update the field with the new value.
            field = value;

            // Notify that the property has changed.
            OnPropertyChanged(propertyName);
            return true;
        }
    }
    // RelayCommand class to handle command binding in ViewModel.
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        // Event that gets fired when CanExecute changes.
        public event EventHandler CanExecuteChanged;

        // Constructor for commands without a condition.
        public RelayCommand(Action<object> execute) : this(execute, null) { }

        // Constructor for commands with a condition.
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Determines if the command can be executed.
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        // Executes the command action.
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
        // Raises the CanExecuteChanged event to notify the UI of state changes.
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

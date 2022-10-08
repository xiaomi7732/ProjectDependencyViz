// using System.Windows.Input;

// namespace ArchAnalyzer.ViewModels;

// public sealed class DelegateCommand : ICommand
// {
//     private Action _handler;
//     private bool _isEnabled = true;
//     public event EventHandler CanExecuteChanged;
//     public bool IsEnabled
//     {
//         get { return _isEnabled; }
//     }
//     public DelegateCommand(
//         DelegateEventHandler handler
//         )
//     {
//         _handler = handler;
//     }
//     void ICommand.Execute(object arg)
//     {
//         _handler();
//     }
//     bool ICommand.CanExecute(object arg)
//     {
//         return IsEnabled;
//     }
//     private void OnCanExecuteChanged()
//     {
//         if (CanExecuteChanged != null)
//         {
//             CanExecuteChanged(this, EventArgs.Empty);
//         }
//     }
// }
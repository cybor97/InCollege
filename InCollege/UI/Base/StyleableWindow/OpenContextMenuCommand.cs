using System;
using System.Windows;
using System.Windows.Input;

namespace WpfStyleableWindow.StyleableWindow
{
    public class OpenContextMenuCommand : ICommand
    {

        public bool CanExecute(object parameter)
        {
            return true;
        }

        #pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (parameter is Window window && window.ContextMenu != null)
                window.ContextMenu.IsOpen = true;
        }
    }
}

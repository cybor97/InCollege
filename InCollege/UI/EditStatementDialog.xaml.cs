using System.Windows;
using System.Windows.Controls;

namespace InCollege.UI
{
    public partial class EditStatementDialog : Grid
    {
        public event RoutedEventHandler OnSave;
        public event RoutedEventHandler OnCancel;

        public EditStatementDialog()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            OnSave?.Invoke(sender, e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnCancel?.Invoke(sender, e);
        }
    }
}

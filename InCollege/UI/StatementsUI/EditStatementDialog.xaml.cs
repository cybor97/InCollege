using MaterialDesignThemes.Wpf;
using System.Windows;

namespace InCollege.UI.StatementsUI
{
    public partial class EditStatementDialog : DialogHost
    {
        public event RoutedEventHandler OnSave;
        public event RoutedEventHandler OnCancel;

        public EditStatementDialog()
        {
            InitializeComponent();

            for (int i = 1; i <= 12; i++)
                SemesterCB.Items.Add(i);
            for (int i = 1; i <= 6; i++)
                CourseCB.Items.Add(i);
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

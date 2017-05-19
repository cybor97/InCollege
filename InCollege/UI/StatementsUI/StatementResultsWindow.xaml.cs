using InCollege.Core.Data;
using System.Windows;

namespace InCollege.Client.UI.StatementsUI
{
    public partial class StatementResultsWindow : Window
    {
        Statement Statement { get => (Statement)DataContext; set => DataContext = value; }

        public StatementResultsWindow(Statement statement)
        {
            InitializeComponent();
        }
    }
}

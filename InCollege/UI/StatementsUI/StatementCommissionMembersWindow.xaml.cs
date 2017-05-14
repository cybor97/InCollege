using InCollege.Core.Data;
using System.Threading.Tasks;
using System.Windows;

namespace InCollege.Client.UI.StatementsUI
{
    public partial class StatementCommissionMembersWindow : Window, IUpdatable
    {
        Statement Statement { get; set; }
        public StatementCommissionMembersWindow(Statement statement)
        {
            InitializeComponent();
            Statement = statement;
        }

        public async Task UpdateData()
        {
            //TODO:Implement!
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sorry, still unimplemented.");
        }
    }
}

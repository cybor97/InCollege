using InCollege.Core.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static InCollege.Client.UI.Util.Generators.StatementViewModelGeneratorFactory;

namespace InCollege.Client.UI.StatementsUI
{
    public partial class StatementResultsWindow : Window
    {
        Statement Statement { get => (Statement)DataContext; set => DataContext = value; }

        public StatementResultsWindow(Statement statement)
        {
            InitializeComponent();
        }

        async void StatementResultsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var statementResults = await NetworkUtils.RequestData<StatementResult>(this, (nameof(StatementResult.StatementID), Statement?.ID ?? -1));

            var generator = GetGenerator(GeneratorType.ComplexStatement);
            var columns = generator.GetColumns(statementResults);
            var something = generator.GetResults(generator.GetColumns(statementResults).Select(c => c.name), statementResults);

            var gridView = (GridView)ContentLV.View;
            foreach (var current in columns)
            {
                gridView.Columns.Add(new GridViewColumn { Header = current.uiName });
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MarkTB_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {

        }

        private void MarkTB_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        private void SaveMiddleStatementButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelMiddleStatementButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

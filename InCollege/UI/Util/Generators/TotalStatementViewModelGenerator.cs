using InCollege.Core.Data;
using System;
using System.Collections.Generic;

namespace InCollege.Client.UI.Util.Generators
{
    class TotalStatementResultViewModelGenerator : StatementResultViewModelGenerator
    {
        public override IList<(string name, string uiName)> GetColumns(IEnumerable<StatementResult> data)
        {
            throw new NotImplementedException();
        }

        public override IList<object> GetResults(IEnumerable<string> columns, IEnumerable<StatementResult> results)
        {
            throw new NotImplementedException();
        }
    }
}

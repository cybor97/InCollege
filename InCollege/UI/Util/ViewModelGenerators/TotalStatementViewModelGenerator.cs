using InCollege.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InCollege.Client.UI.Util.Generators
{
    class TotalStatementResultViewModelGenerator : StatementResultViewModelGenerator
    {
        public override IList<(string name, string uiName)> GetColumns(IEnumerable<StatementResult> data)
        {
            var result = new List<(string name, string uiName)>
            {
                ("StudentFullName", "ФИО обучающегося")
            };

            result.AddRange(data.Distinct(new DistinctBySubject()).Select(c => (name: $"subject{c.SubjectID}", uiName: $"{c.SubjectIndex}|{c.StatementResultDate?.ToString("dd.MM.yyyy")}")));

            return result;
        }

        public override IList<StatementResultViewModel> GetResults(IEnumerable<string> columns, IEnumerable<StatementResult> statementResults)
        {
            //   List<string> properties = 
            var type = StatementViewModelTypeBuilder.BuildTypeForFields(columns.Where(c => c != "StudentFullName"));
            var result = new List<StatementResultViewModel>();
            var distinctor = new DistinctByStudent();// :D

            if (statementResults != null)
            {
                foreach (var current in statementResults.Distinct(distinctor))
                {
                    var obj = (StatementResultViewModel)Activator.CreateInstance(type);
                    obj.StudentID = current.StudentID;
                    obj.StudentFullName = current.StudentFullName;
                    result.Add(obj);
                }

                //TODO:Check me on free mind
                foreach (var currentResult in statementResults)
                    foreach (var currentColumn in columns.Where(c => c.StartsWith("subject")))
                        if (currentResult.SubjectID.ToString() == currentColumn.Split(new[] { "subject" }, StringSplitOptions.RemoveEmptyEntries)[0])
                            type.GetProperty(currentColumn)
                                .SetValue(result.First(c => c.StudentID == currentResult.StudentID), currentResult.MarkValueString);
            }
            return result;
        }
    }
}

using InCollege.Core.Data;
using InCollege.Core.Data.Base;
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
            //Make nulls at the end.
            result.AddRange(data.Distinct(new DistinctBySubject())
                                .Select(c =>
                                {
                                    var statementResultDate = c.StatementResultDate ??
                                                              data.FirstOrDefault(currentHelpfulResult => currentHelpfulResult.SubjectID == c.SubjectID &&
                                                                                                          currentHelpfulResult.StatementResultDate != null)
                                                                  .StatementResultDate;
                                    return (name: $"subject{c.SubjectID}", uiName: $"{c.SubjectIndex}({statementResultDate?.ToString("dd.MM.yyyy")})");
                                }));
            result.Add(("average", "Средний балл"));

            return result;
        }

        public override IList<StatementResultViewModel> GetResults(IEnumerable<string> columns, IEnumerable<StatementResult> statementResults)
        {
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
                //Get all results.
                //For each column, what is subject and contains subject ID set it's string value, store it in first 'result', that is for current student.  
                foreach (var currentResult in statementResults)
                    foreach (var currentColumn in columns.Where(c => c.StartsWith("subject")))
                        if (currentResult.SubjectID.ToString() == currentColumn.Split(new[] { "subject" }, StringSplitOptions.RemoveEmptyEntries)[0])
                            type.GetProperty(currentColumn)
                                .SetValue(result.First(c => c.StudentID == currentResult.StudentID), currentResult.MarkValueString);
                //Get all results for this student. 
                //If has any 'Blanks' - result will blank. 
                //If has any absents or unpasseds(does this word exist, btw? :D ) then result will be 'Unpassed'
                //Else result is average of all subject marks.
                //Aaaand we don't count "Passed".
                foreach (var current in result)
                {
                    var currentStudentResults = statementResults.Where(c => c.StudentID == current.StudentID && c.MarkValue != (sbyte)TechnicalMarkValue.Passed);
                    type.GetProperty("average")
                        .SetValue(current, currentStudentResults.Any(c => c.MarkValue == (sbyte)TechnicalMarkValue.Blank) ?
                                           StatementResult.GetMarkValueString((sbyte)TechnicalMarkValue.Blank) :
                                           currentStudentResults.Any(c => c.MarkValue == (sbyte)TechnicalMarkValue.Absent || c.MarkValue == (sbyte)TechnicalMarkValue.Unpassed) ?
                                           StatementResult.GetMarkValueString((sbyte)TechnicalMarkValue.Unpassed) :
                                           StatementResult.GetMarkValueString((sbyte)currentStudentResults.Average(c => c.MarkValue)));
                }
            }
            return result;
        }
    }
}

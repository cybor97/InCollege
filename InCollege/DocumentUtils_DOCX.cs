using InCollege.Client.UI.Util.Generators;
using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using Novacode;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InCollege
{
    static class DocumentUtils_DOCX
    {
        public static void SaveStatementWithTemplate(Statement statement,
                                         List<StatementResult> results,
                                         List<AttestationType> attestationTypes,
                                         List<Account> commissionMembers,
                                         string templateFilename, string saveFilename)
        {
            DocX.Load(templateFilename)

                .Fill("{StatementNumber}", statement.StatementNumber.ToString())

                .Fill("{Subject.Name}", statement.Subject?.SubjectName)
                .Fill("{Subject.Index}", statement.Subject?.SubjectIndex?.ToString())

                .Fill("{Specialty.Name}", statement.Specialty?.SpecialtyName)
                .Fill("{Specialty.Code}", statement.Specialty?.SpecialtyCode)

                .Fill("{Group.Name}", statement.Group?.GroupName)
                .Fill("{Group.Code}", statement.Group?.GroupCode)

                .Fill("{Course}", statement.Course.ToRoman())
                .Fill("{Semester}", statement.Semester.ToString())

                .Fill("{StatementDate.Day}", statement.StatementDate.Day.ToString())
                .Fill("{StatementDate.Month}", CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.MonthGenitiveNames[statement.StatementDate.Month - 1])
                .Fill("{StatementDate.Year}", statement.StatementDate.Year.ToString())

                .Fill("{Note}", statement.Note ?? string.Empty)

                .Fill("{StatementAttestationTypes}", string.Join(", ", attestationTypes?.Select(c => c.TypeName)))
                .Fill("{StatementCommissionMembers}", string.Join(", ", commissionMembers?.Select(c => c.FullName)))
                .Fill("{StatementCommissionMembers_Signs}", string.Join("\n", commissionMembers?.Select(c => $"_________ {c.FullNameInitials}")))

                .FillStatementResults(statement, results)

                .SaveAs(saveFilename);
        }

        static DocX FillStatementResults(this DocX document, Statement statement, List<StatementResult> results)
        {
            switch (statement.StatementType)
            {
                case StatementType.Middle:
                case StatementType.Exam:
                    return document.FillSimpleStatementResults(statement, results);
                case StatementType.Total:
                    return document.FillTotalStatementResults(statement, results);
                default: return document.FillComplexStatementResults(statement, results);
            }
        }

        static DocX FillTotalStatementResults(this DocX document, Statement statement, List<StatementResult> results)
        {
            var table = document.Tables.FirstOrDefault(c => c.Rows.Count >= 3);
            if (results != null && table != null)
            {
                var titleRow = table.Rows[0];

                var bottomRow = table.Rows[table.RowCount - 1];

                var subjectsMarkerCell = titleRow.Cells.FirstOrDefault(c => c.Paragraphs.Count > 0 && c.Paragraphs[0].Text == "{Subjects}");

                var templateRow = table.Rows.FirstOrDefault(row => row.Cells.Any(cell => cell.Paragraphs.Count > 0 && cell.Paragraphs[0].Text == "{IndexNumber}"));

                var generator = StatementViewModelGeneratorFactory.GetGenerator(StatementViewModelGeneratorFactory.GeneratorType.TotalStatement);
                var columns = generator.GetColumns(results)
                                       .Where(c => (c.name?.StartsWith("subject") ?? false) ||
                                                   (c.name?.Equals("average") ?? false))
                                       .Reverse()
                                       .ToList();
                #region Columns generating
                //DO NOT TRY TO OPTIMIZE! DO NOT F*KIN' TRY! THE FAIR BUG IS WAITING HERE!
                for (int column_i = 0; column_i < columns.Count; column_i++)
                    if (column_i != 0)
                        table.InsertColumn(table.ColumnCount - 2);

                for (int column_i = 0; column_i < columns.Count; column_i++)
                {
                    var subjectCell = titleRow.Cells[table.ColumnCount - column_i - 2];
                    subjectCell.TextDirection = TextDirection.btLr;
                    subjectCell.VerticalAlignment = VerticalAlignment.Center;

                    var paragraph = subjectCell.Paragraphs[0];

                    paragraph.Alignment = Alignment.center;
                    if (!string.IsNullOrWhiteSpace(paragraph.Text))
                        paragraph.RemoveText(0);
                    paragraph.InsertText(columns[column_i].uiName?.Split('(')[0] ?? " ");
                    if (columns[column_i].name == "average")
                        paragraph.Bold();




                    var bottomCell = bottomRow.Cells[table.ColumnCount - column_i - 2];
                    bottomCell.TextDirection = TextDirection.btLr;
                    bottomCell.VerticalAlignment = VerticalAlignment.Center;

                    paragraph = bottomCell.Paragraphs[0];

                    paragraph.Alignment = Alignment.center;
                    if (!string.IsNullOrWhiteSpace(paragraph.Text))
                        paragraph.RemoveText(0);
                    if (Regex.IsMatch(columns[column_i].uiName, "^.*\\(.*\\)$"))
                        paragraph.InsertText(columns[column_i].uiName?.Split('(')[1].Split(')')?[0] ?? " ");
                    if (columns[column_i].name == "average")
                        paragraph.Bold();




                    var contentCell = templateRow.Cells[table.ColumnCount - column_i - 2];
                    paragraph = contentCell.Paragraphs[0];
                    if (!string.IsNullOrWhiteSpace(paragraph.Text))
                        paragraph.RemoveText(0);
                    paragraph.InsertText($"{{{columns[column_i].name}}}");
                }
                #endregion
                #region Data filling
                var resultModels = generator.GetResults(columns.Select(c => c.name), results);

                for (int result_i = 0; result_i < resultModels.Count; result_i++)
                {
                    var currentRow = table.InsertRow(templateRow, table.RowCount - 1);

                    for (int cell_i = 0; cell_i < templateRow.Cells.Count - 3; cell_i++)
                    {
                        var cell = currentRow.Cells[cell_i + 2];
                        if (cell.Paragraphs[0].Text != string.Empty)
                        {
                            if (Regex.IsMatch(cell.Paragraphs[0].Text, "^{.*}$"))
                                cell.Fill(cell.Paragraphs[0].Text, resultModels[result_i].GetType()
                                                                                         .GetProperty(cell.Paragraphs[0].Text.Split('{')[1].Split('}')[0])
                                                                                         .GetValue(resultModels[result_i])
                                                                                         ?.ToString() ?? string.Empty);

                            currentRow.Cells[cell_i]
                                      .Fill("{IndexNumber}", (result_i + 1).ToString())
                                      .Fill("{StatementResult.Student.FullName}", resultModels[result_i].StudentFullName);
                        }
                    }
                }

                templateRow.Remove();
                #endregion
            }

            return document;
        }

        static DocX FillComplexStatementResults(this DocX document, Statement statement, List<StatementResult> results)
        {
            var table = document.Tables.FirstOrDefault(c => c.Rows.Count >= 3);
            if (results != null && table != null)
            {
                var titleRow = table.Rows[0];

                var subjectsRow = table.Rows.FirstOrDefault(row => row.Cells.Any(cell => cell.Paragraphs.Count > 0 && cell.Paragraphs[0].Text == "{Subjects}"));
                var templateRow = table.Rows.FirstOrDefault(row => row.Cells.Any(cell => cell.Paragraphs.Count > 0 && cell.Paragraphs[0].Text == "{IndexNumber}"));

                var subjectsMarkerCell = subjectsRow.Cells.FirstOrDefault(c => c.Paragraphs.Count > 0 && c.Paragraphs[0].Text == "{Subjects}");

                var generator = StatementViewModelGeneratorFactory.GetGenerator(StatementViewModelGeneratorFactory.GeneratorType.ComplexStatement);
                var columns = generator.GetColumns(results)
                                       .Where(c => c.name?.StartsWith("subject") ?? false)
                                       .Reverse()
                                       .ToList();
                if (columns.Count > 0)
                {
                    #region Columns generating
                    //DO NOT TRY TO OPTIMIZE! DO NOT F*KIN' TRY! THE FAIR BUG IS WAITING HERE!
                    for (int column_i = 0; column_i < columns.Count; column_i++)
                        if (column_i != 0)
                            table.InsertColumn(table.ColumnCount - 2);

                    for (int column_i = 0; column_i < columns.Count; column_i++)
                    {
                        var subjectCell = subjectsRow.Cells[table.ColumnCount - column_i - 2];
                        subjectCell.TextDirection = TextDirection.btLr;
                        subjectCell.VerticalAlignment = VerticalAlignment.Center;

                        var paragraph = subjectCell.Paragraphs[0];

                        paragraph.Alignment = Alignment.center;
                        if (!string.IsNullOrWhiteSpace(paragraph.Text))
                            paragraph.RemoveText(0);
                        paragraph.InsertText(columns[column_i].uiName ?? " ");

                        var contentCell = templateRow.Cells[table.ColumnCount - column_i - 2];
                        paragraph = contentCell.Paragraphs[0];
                        if (!string.IsNullOrWhiteSpace(paragraph.Text))
                            paragraph.RemoveText(0);
                        paragraph.InsertText($"{{{columns[column_i].name}}}");
                    }
                    #endregion
                    #region Data filling
                    var resultModels = generator.GetResults(columns.Select(c => c.name), results);

                    for (int result_i = 0; result_i < resultModels.Count; result_i++)
                    {
                        var currentRow = table.InsertRow(templateRow);

                        for (int cell_i = 0; cell_i < templateRow.Cells.Count - 3; cell_i++)
                        {
                            var cell = currentRow.Cells[cell_i + 2];
                            if (cell.Paragraphs[0].Text != string.Empty)
                            {
                                if (Regex.IsMatch(cell.Paragraphs[0].Text, "^{.*}$"))
                                    cell.Fill(cell.Paragraphs[0].Text, resultModels[result_i].GetType()
                                                                                             .GetProperty(cell.Paragraphs[0].Text.Split('{')[1].Split('}')[0])
                                                                                             .GetValue(resultModels[result_i])
                                                                                             .ToString());

                                currentRow.Cells[cell_i]
                                          .Fill("{IndexNumber}", (result_i + 1).ToString())
                                          .Fill("{StatementResult.Student.FullName}", resultModels[result_i].StudentFullName);
                            }
                        }
                    }
                    #endregion
                }
                else subjectsMarkerCell.Paragraphs.FirstOrDefault()?.RemoveText(0);

                templateRow.Remove();

                if (columns.Count > 0)
                    titleRow.MergeCells(table.ColumnCount - columns.Count() - 1, table.ColumnCount - 2);
            }
            return document;
        }

        static DocX FillSimpleStatementResults(this DocX document, Statement statement, List<StatementResult> results)
        {
            var table = document.Tables.FirstOrDefault(c => c.Rows.Count >= 1);
            if (results != null && table != null)
            {
                var templateRow = table.Rows.LastOrDefault();

                for (int result_i = 0; result_i < results.Count; result_i++)
                {
                    var currentRow = table.InsertRow(templateRow);

                    for (int cell_i = 0; cell_i < templateRow.Cells.Count; cell_i++)
                    {
                        int ticketNumber = results[result_i].TicketNumber;
                        currentRow.Cells[cell_i]
                                  .Fill("{IndexNumber}", (result_i + 1).ToString())
                                  .Fill("{StatementResult.Student.FullName}", results[result_i].StudentFullName)
                                  .Fill("{StatementResult.MarkValue}", results[result_i].MarkValueString)
                                  .Fill("{StatementResult.TicketNumber}", ticketNumber == -1 ? "" : ticketNumber.ToString());
                    }
                }

                templateRow.Remove();
            }
            return document;
        }

        static T Fill<T>(this T docx, string placeHolder, string content) where T : Container
        {
            docx.ReplaceText(placeHolder, content ?? " ");
            return docx;
        }

        static string ToRoman(this int number)
        {
            StringBuilder result = new StringBuilder();
            int[] digitsValues = { 1, 4, 5, 9, 10, 40, 50, 90, 100, 400, 500, 900, 1000 };
            string[] romanDigits = { "I", "IV", "V", "IX", "X", "XL", "L", "XC", "C", "CD", "D", "CM", "M" };
            while (number > 0)
                for (int i = digitsValues.Count() - 1; i >= 0; i--)
                    if (number / digitsValues[i] >= 1)
                    {
                        number -= digitsValues[i];
                        result.Append(romanDigits[i]);
                        break;
                    }
            return result.ToString();
        }
    }
}
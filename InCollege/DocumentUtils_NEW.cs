using InCollege.Core.Data;
using System.Collections.Generic;
using NetOffice.WordApi;
using System.Linq;
using System.Text;
using System.Globalization;

namespace InCollege
{
    public static class DocumentUtils_NEW
    {
        public static void SaveStatementWithTemplate(Statement statement,
                                                     List<StatementResult> results,
                                                     List<AttestationType> attestationTypes,
                                                     List<Account> commissionMembers,
                                                     string templateFilename, string saveFilename)
        {
            var app = new Application();
            var document = app.Documents.Open(templateFilename);

            app.ReplaceText("{StatementNumber}", statement.StatementNumber.ToString())

               .ReplaceText("{Subject.Name}", statement.Subject?.SubjectName)
               .ReplaceText("{Subject.Index}", statement.Subject?.SubjectIndex?.ToString())

               .ReplaceText("{Specialty.Name}", statement.Specialty?.SpecialtyName)
               .ReplaceText("{Specialty.Code}", statement.Specialty?.SpecialtyCode)

               .ReplaceText("{Group.Name}", statement.Group?.GroupName)
               .ReplaceText("{Group.Code}", statement.Group?.GroupCode)

               .ReplaceText("{Course}", statement.Course.ToRoman())
               .ReplaceText("{Semester}", statement.Semester.ToString())

               .ReplaceText("{StatementDate.Day}", statement.StatementDate.Day.ToString())
               .ReplaceText("{StatementDate.Month}", CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.MonthGenitiveNames[statement.StatementDate.Month - 1])
               .ReplaceText("{StatementDate.Year}", statement.StatementDate.Year.ToString())

               .ReplaceText("{Note}", statement.Note)

               .ReplaceText("{StatementAttestationTypes}", string.Join(", ", attestationTypes?.Select(c => c.TypeName)))
               .ReplaceText("{StatementCommissionMembers}", string.Join(", ", commissionMembers?.Select(c => c.FullName)))
               .ReplaceText("{StatementCommissionMembers_Signs}", string.Join("\n", commissionMembers?.Select(c => $"_________ {c.FullNameInitials}")));

            // document.Paragraphs[0].
            FillSimpleStatementResults(statement, results, document, app);

            document.SaveAs(saveFilename);
            app.Quit();
        }

        static void FillSimpleStatementResults(Statement statement, List<StatementResult> results, Document document, Application app)
        {
            var table = document.Tables.FirstOrDefault();
            if (table != null && table.Rows.Count >= 1)
                if (results != null)
                {
                    var templateRow = table.Rows.LastOrDefault();

                    for (int result_i = 0; result_i < results.Count; result_i++)
                    {
                        var currentRow = table.Rows.Add();
                        for (int row_i = 0; row_i < templateRow.Cells.Count; row_i++)
                        {
                            //table.Rows[row_i].Range.
                            app.Selection.Text.Replace("{IndexNumber}", (result_i + 1).ToString())
                                              .Replace("{StatementResult.Student.FullName}", results[result_i].StudentFullName)
                                              .Replace("{StatementResult.MarkValue}", results[result_i].MarkValueString)
                                              .Replace("{StatementResult.TicketNumber}", results[result_i].TicketNumber.ToString());
                            app.Selection.Collapse();
                        }
                    }
                    templateRow.Delete();
                }
                else table.Delete();
        }

        static Application ReplaceText(this Application app, string placeHolder, string content)
        {
            Find findObject = app.Selection.Find;
            findObject.ClearFormatting();
            findObject.Text = placeHolder;
            findObject.Replacement.ClearFormatting();
            findObject.Replacement.Text = content ?? string.Empty;
            findObject.Execute();
            app.Selection.Collapse();
            return app;
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

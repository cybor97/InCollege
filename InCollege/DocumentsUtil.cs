using InCollege.Core.Data;
using NPOI.XWPF.UserModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InCollege
{
    public static class DocumentsUtil
    {
        public static void SaveStatementWithTemplate(Statement statement,
            List<StatementResult> results,
            List<AttestationType> attestationTypes,
            List<Account> commissionMembers,
            string templateFilename, string saveFilename)
        {
            if (statement != null)
            {
                using (var templateFile = new FileStream(templateFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var document = new XWPFDocument(templateFile);
                    document.ReplaceText("{StatementNumber}", statement.StatementNumber.ToString())

                            .ReplaceText("{Subject.Name}", statement.Subject?.SubjectName)
                            .ReplaceText("{Subject.Index}", statement.Subject?.SubjectIndex?.ToString())

                            .ReplaceText("{Specialty.Name}", statement.Specialty?.SpecialtyName)
                            .ReplaceText("{Specialty.Code}", statement.Specialty?.SpecialtyCode)

                            .ReplaceText("{Group.Name}", statement.Group?.GroupName)
                            .ReplaceText("{Group.Code}", statement.Group?.GroupCode)

                            .ReplaceText("{Course}", statement.Course.ToRoman())
                            .ReplaceText("{Semester}", statement.Semester.ToString())

                            .ReplaceText("{StatementDate.Day}", statement.StatementDate.Day.ToString())
                            .ReplaceText("{StatementDate.Month}", statement.StatementDate.ToString("MMMM"))
                            .ReplaceText("{StatementDate.Year}", statement.StatementDate.Year.ToString())

                            .ReplaceText("{StatementAttestationTypes}", string.Join(", ", attestationTypes?.Select(c => c.TypeName)))
                            .ReplaceText("{StatementCommissionMembers}", string.Join(", ", commissionMembers?.Select(c => c.FullName)))
                            .ReplaceText("{StatementCommissionMembers_Signs}", string.Join("\n", commissionMembers?.Select(c => $"_________ {c.FullName}")));


                    templateFile.Close();
                    using (var resultFileStream = new FileStream(saveFilename, FileMode.OpenOrCreate))
                    {
                        document.Write(resultFileStream);
                        resultFileStream.Flush();
                    }
                }


                //TODO:Table detection
                //TODO:OrderNumber
                //TODO:StatementResult.Student.FullName
                //TODO:StatementResult.MarkValue
            }
        }

        public static XWPFDocument ReplaceText(this XWPFDocument document, string token, string textToReplace)
        {
            ReplaceInParagraphs(document.Paragraphs, token, textToReplace);
            return document;
        }

        static void ReplaceInParagraphs(IList<XWPFParagraph> paragraphs, string placeHolder, string replaceText)
        {
            foreach (XWPFParagraph xwpfParagraph in paragraphs)
            {
                IList<XWPFRun> runs = xwpfParagraph.Runs;
                foreach (XWPFRun run in runs)
                {
                    string runText = run.Text;

                    if (!string.IsNullOrWhiteSpace(placeHolder))
                    {
                        if (runText != null && placeHolder == runText)
                        {
                            run.ReplaceText(runText, replaceText);
                        }
                    }
                    run.SetText(runText, 0);
                }
            }
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

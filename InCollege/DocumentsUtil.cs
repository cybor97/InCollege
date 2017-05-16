using InCollege.Core.Data;
using NPOI.XWPF.UserModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InCollege
{
    public static class DocumentsUtil
    {
        public static void SaveStatementWithTemplate(Statement statement, List<object> results, string templateFilename, string saveFilename)
        {
            if (statement != null)
            {
                var document = new XWPFDocument();
                document.ReplaceText("{StatementNumber}", statement.StatementNumber.ToString());

                document.ReplaceText("{Subject.Name}", statement.Subject?.SubjectName);
                document.ReplaceText("{Subject.Index}", statement.Subject?.SubjectIndex?.ToString());

                document.ReplaceText("{Specialty.Name}", statement.Specialty?.SpecialtyName);
                document.ReplaceText("{Specialty.Code}", statement.Specialty?.SpecialtyCode);

                document.ReplaceText("{Group.Name}", statement.Group?.GroupName);
                document.ReplaceText("{Group.Code}", statement.Group?.GroupCode);

                document.ReplaceText("{Course}", statement.Course.ToRoman());
                document.ReplaceText("{Semester}", statement.Semester.ToString());

                document.ReplaceText("{StatementDate.Day}", statement.StatementDate.Day.ToString());
                document.ReplaceText("{StatementDate.Month}", statement.StatementDate.ToString("MMMM"));
                document.ReplaceText("{StatementDate.Year}", statement.StatementDate.Year.ToString());
            }
        }

        public static void ReplaceText(this XWPFDocument document, string template, string value)
        {
            document.Paragraphs.FirstOrDefault(c => c.Text.Contains(template))?.ReplaceText(template, value);
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

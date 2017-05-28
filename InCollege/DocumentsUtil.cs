using InCollege.Client.UI.Util.Generators;
using InCollege.Core.Data;
using InCollege.Core.Data.Base;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    var document = new XWPFDocument(templateFile)
                                        .ReplaceText("{StatementNumber}", statement.StatementNumber.ToString())

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

                            .ReplaceText("{Note}", statement.Note ?? string.Empty)

                            .ReplaceText("{StatementAttestationTypes}", string.Join(", ", attestationTypes?.Select(c => c.TypeName)))
                            .ReplaceText("{StatementCommissionMembers}", string.Join(", ", commissionMembers?.Select(c => c.FullName)))
                            .ReplaceText("{StatementCommissionMembers_Signs}", string.Join("\n", commissionMembers?.Select(c => $"_________ {c.FullNameInitials}")));

                    if (statement.StatementType == StatementType.Middle || statement.StatementType == StatementType.Exam)
                        FillSimpleStatementResults(statement, results, document);
                    else FillComplexStatementResults(statement, results, document);

                    templateFile.Close();
                    using (var resultFileStream = new FileStream(saveFilename, FileMode.OpenOrCreate))
                    {
                        document.Write(resultFileStream);
                        resultFileStream.Flush();
                        resultFileStream.Close();
                    }
                }

            }
        }

        static void FillComplexStatementResults(Statement statement, List<StatementResult> results, XWPFDocument document)
        {
            var table = document.Tables.FirstOrDefault();
            if (table != null && table.Rows.Count >= 2)
                if (results != null)
                {
                    var placeHolderHeaderRow = table.Rows.FirstOrDefault(row => row.GetTableCells().Any(cell => cell.GetText() == "{TopColumn}"));
                    var placeHolderHeaderCell = table.Rows
                                                     .Select(row => row.GetTableCells().FirstOrDefault(cell => cell.GetText() == "{TopColumn}"))
                                                     .FirstOrDefault(cell => cell != null && cell.GetText() == "{TopColumn}");

                    if (placeHolderHeaderCell != null)
                    {

                        var resultRow = table.Rows.LastOrDefault();
                        var columns = StatementViewModelGeneratorFactory.GetGenerator(StatementViewModelGeneratorFactory.GeneratorType.ComplexStatement)
                                                          .GetColumns(results)
                                                          .Where(c => c.name.StartsWith("subject"))
                                                          .ToList();

                        foreach (var current in columns)
                        {
                            if (columns.IndexOf(current) == 0)
                            {
                                placeHolderHeaderCell.Paragraphs[0].Alignment = ParagraphAlignment.CENTER;
                                placeHolderHeaderCell.SetVerticalAlignment(XWPFTableCell.XWPFVertAlign.CENTER);

                                placeHolderHeaderCell.Paragraphs[0].ReplaceText("{TopColumn}", current.uiName ?? current.name);
                                placeHolderHeaderCell.GetCTTc().AddNewTcPr().textDirection = new CT_TextDirection { val = ST_TextDirection.btLr };
                            }
                            else
                            {
                                table.Rows[0].CreateCell();
                                resultRow.CreateCell();

                                var cell = placeHolderHeaderCell.GetTableRow().CreateCell();
                                cell.SetText(current.uiName ?? current.name);
                                cell.Paragraphs[0].Alignment = ParagraphAlignment.CENTER;
                                cell.SetVerticalAlignment(XWPFTableCell.XWPFVertAlign.CENTER);
                                cell.GetCTTc().AddNewTcPr().textDirection = new CT_TextDirection { val = ST_TextDirection.btLr };
                            }
                        }

                        int placeHolderHeaderCellIndex = placeHolderHeaderRow.GetTableCells().IndexOf(placeHolderHeaderCell);
                        table.Rows[0].MergeCells(placeHolderHeaderCellIndex, placeHolderHeaderCellIndex + columns.Count - 1);
                        table.Rows[0].GetTableCells()[placeHolderHeaderCellIndex].GetCTTc().tcPr.tcW = new CT_TblWidth { type = ST_TblWidth.dxa, typeSpecified = true, w = "1200" };


                        var lastHeaderCell = table.Rows[0].CreateCell();
                        lastHeaderCell.SetText("Итог экзамена квалификационного «вид профессиональной деятельности освоен / не освоен»");
                        var gridSpan = lastHeaderCell.GetCTTc().AddNewTcPr().AddNewGridspan();
                        gridSpan.val = "500";

                        for (int i = 1; i < table.Rows.Count; i++)
                            table.Rows[i].CreateCell().GetCTTc().AddNewTcPr().gridSpan = gridSpan;

                     //   table.MergeCellsVertically(table.Rows[0].GetTableCells().Count - 1, 0, 1);
                    }
                }
                else document.Tables.Remove(table);
        }

        private static void MergeCellsVertically(this XWPFTable table, int col, int fromRow, int toRow)
        {

            for (int rowIndex = fromRow; rowIndex <= toRow; rowIndex++)
            {

                XWPFTableCell cell = table.GetRow(rowIndex).GetCell(col);

                if (rowIndex == fromRow)
                {
                    // The first merged cell is set with RESTART merge value
                    cell.GetCTTc().AddNewTcPr().AddNewVMerge().val = ST_Merge.restart;
                }
                else
                {
                    // Cells which join (merge) the first one, are set with CONTINUE
                    cell.GetCTTc().AddNewTcPr().AddNewVMerge().val = ST_Merge.@continue;
                }
            }
        }


        static void FillSimpleStatementResults(Statement statement, List<StatementResult> results, XWPFDocument document)
        {
            var table = document.Tables.FirstOrDefault();
            if (table != null && table.Rows.Count >= 1)
                if (results != null)
                {
                    var templateRow = table.Rows.LastOrDefault();
                    table.RemoveRow(table.Rows.IndexOf(templateRow));

                    for (int i = 0; i < results.Count; i++)
                    {
                        var currentRow = table.CreateRow();

                        foreach (var currentTemplateCell in templateRow.GetTableCells())
                        {
                            currentRow.GetCell(templateRow.GetTableCells().IndexOf(currentTemplateCell))
                                .SetText(currentTemplateCell.GetText()
                                                            .Replace("{IndexNumber}", (i + 1).ToString())
                                                            .Replace("{StatementResult.Student.FullName}", results[i].StudentFullName)
                                                            .Replace("{StatementResult.MarkValue}", results[i].MarkValueString)
                                                            .Replace("{StatementResult.TicketNumber}", results[i].TicketNumber.ToString()));
                        }
                    }
                }
                else document.Tables.Remove(table);
        }

        public static XWPFTableCell ReplaceText(this XWPFTableCell cell, string placeHolder, string replaceText)
        {
            var cellText = cell.GetText();
            if (cellText == placeHolder)
                cell.SetText(replaceText);
            return cell;
        }

        public static XWPFDocument ReplaceText(this XWPFDocument document, string token, string textToReplace)
        {
            ReplaceInParagraphs(document.Paragraphs, token, textToReplace);
            return document;
        }

        static void ReplaceInParagraphs(IList<XWPFParagraph> paragraphs, string placeHolder, string replaceText)
        {
            List<Task> pendingOperations = new List<Task>();
            foreach (XWPFParagraph paragraph in paragraphs)
            {
                IList<XWPFRun> runs = paragraph.Runs;
                foreach (XWPFRun run in runs)
                {
                    string runText = run.Text;

                    if (!string.IsNullOrWhiteSpace(placeHolder))
                    {
                        if (runText != null && placeHolder == runText)
                        {
                            if (!replaceText.Contains("\n"))
                                run.ReplaceText(runText, replaceText);
                            else
                                pendingOperations.Add(new Task(() =>
                                {
                                    paragraph.RemoveRun(paragraph.Runs.IndexOf(run));

                                    foreach (var current in replaceText.Split('\n'))
                                    {
                                        var newRun = paragraph.CreateRun();
                                        newRun.SetText(current);
                                        paragraph.AddRun(newRun);
                                        newRun.AddBreak(BreakType.TEXTWRAPPING);
                                    }
                                }));
                        }
                    }
                }
            }
            foreach (var current in pendingOperations)
                current.RunSynchronously();
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

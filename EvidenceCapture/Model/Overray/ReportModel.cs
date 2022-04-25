using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using EvidenceCapture.Model.Base;
using EvidenceCapture.Model.ProcessResult;
using EvidenceCapture.Properties;
using EvidenceCapture.ViewModel.Overray;
using Microsoft.Office.Interop.Excel;

namespace EvidenceCapture.Model.Overray
{
    class ReportModel : ModelBase
    {
        public ReportViewModel.FormatType SelectFormat { get; internal set; }
        public List<SnapTreeItem> TargetList { get; internal set; }



        internal async void CreateReport(System.Action<object> callBack, System.Action<Exception> failedBack)
        {
            var outResult = new ReportOut();
            Exception exp = null;

            var result = await Task.Run<bool>(() =>
            {
                try
                {
                    switch (SelectFormat)
                    {
                        case ReportViewModel.FormatType.Excel:
                            outResult = ExcelOut();
                            break;
                        case ReportViewModel.FormatType.PDF:
                            outResult = PDFOut();
                            break;
                        case ReportViewModel.FormatType.HTML:
                            outResult = HTMLOut();
                            break;
                    }
                }
                catch (Exception e)
                {
                    exp = e;
                    logger.Debug(LogMessage.DParams, nameof(SelectFormat), SelectFormat.ToString());
                    logger.Error(e);
                    return false;
                }


                return true;
            });

            if (result)
                callBack(outResult);
            else
                failedBack(exp);

        }

        private ReportOut HTMLOut()
        {
            var rtn = new ReportOut();
            rtn.ReportType = ReportOut.ReportEnum.HTML;

            var destPath = Path.Combine(ApplicationSettings.Instance.OutputDir,
                "Report.html");

            rtn.OutputPath = destPath;

            var html = GetHTMLStr();
            using (var writer = new StreamWriter(destPath, false))
            {
                writer.WriteLine(html);
            }
            logger.Info(LogMessage.ISuccess, nameof(HTMLOut));

            return rtn;
        }

        private string GetHTMLStr()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(@"<html>");
            foreach (var pn in TargetList)
            {
                sb.Append(@"<div style='margin:2em;page-break-after:always;'>");
                sb.Append($"<p>{pn.Name}</p>");
                foreach (var cn in pn.Children)
                {
                    sb.Append(@"<div style='margin:2em;'>");
                    sb.Append($"<img src='{ApplicationSettings.Instance.OutputDir}/{pn.Name}/{cn.Name}' style='width:80%;border:inset 1px;'>");
                    sb.Append(@"</div>");

                }
                sb.Append(@"</div>");

            }

            sb.Append(@"</html>");

            return sb.ToString();

        }

        private ReportOut PDFOut()
        {
            var rtn = new ReportOut();
            rtn.ReportType = ReportOut.ReportEnum.PDF;


            var destPath = Path.Combine(ApplicationSettings.Instance.OutputDir,
                "Report.pdf");

            rtn.OutputPath = destPath;

            var html = GetHTMLStr();


            using (MemoryStream ms = new MemoryStream())
            {
                var pdf = TheArtOfDev.HtmlRenderer.PdfSharp.PdfGenerator.GeneratePdf(html, PdfSharp.PageSize.A4);
                pdf.Save(ms);
                var result = ms.ToArray();

                using (BinaryWriter w = new BinaryWriter(File.OpenWrite(destPath)))
                {
                    w.Write(result);
                }

            }
            logger.Info(LogMessage.ISuccess, nameof(PDFOut));

            return rtn;


        }

        private ReportOut ExcelOut()
        {
            var rtn = new ReportOut();
            rtn.ReportType = ReportOut.ReportEnum.Excel;

            var destPath = Path.Combine(ApplicationSettings.Instance.OutputDir,
                "Report.xlsx");

            rtn.OutputPath = destPath;

            dynamic excelApp = null;
            dynamic workBooks = null;
            dynamic workBook = null;
            dynamic workSheets = null;
            dynamic workSheet = null;
            dynamic range = null;
            dynamic shape = null;

            try
            {
                Type excelApplication = Type.GetTypeFromProgID("Excel.Application");
                excelApp = Activator.CreateInstance(excelApplication);

                // 確認メッセージ非表示
                excelApp.DisplayAlerts = false;

                // ワークブック、シート定義
                workBooks = excelApp.WorkBooks;
                workBook = workBooks.Add();
                workSheets = workBook.Sheets;


                foreach (var dirNode in TargetList)
                {
                    if (dirNode.Children.Count > 0)
                    {
                        var currentSheet = workBook.workSheets[workBook.workSheets.Count];
                        currentSheet.Name = dirNode.Name;

                        // セルA2選択
                        range = currentSheet.Range["A1"];
                        double NextTop = range.Top;
                        double Left = range.Width;
                        double Width = 0;
                        double Height = 0;

                        foreach (var fileNode in dirNode.Children)
                        {

                            var sourceImagePath = Path.Combine(
                                ApplicationSettings.Instance.OutputDir,
                                    dirNode.Name,
                                    fileNode.Name);

                            shape = currentSheet.Shapes.AddPicture(sourceImagePath,
                                Microsoft.Office.Core.MsoTriState.msoTrue,
                                Microsoft.Office.Core.MsoTriState.msoTrue,
                                Left, NextTop, Width, Height);

                            shape.ScaleHeight(1.0, true);
                            shape.ScaleWidth(1.0, true);

                            NextTop += shape.Height + range.Height;

                        }
                        workSheets.Add(After: currentSheet);
                    }
                }

                if (workBook.workSheets.Count > 0)
                {
                    var lastSeet = workBook.workSheets[workBook.workSheets.Count];
                    lastSeet.Delete();
                    workBook.SaveAs(destPath);
                }
                excelApp.Quit();
            }
            finally
            {
                if (range != null)
                    Marshal.ReleaseComObject(range);
                if (workSheets != null)
                    Marshal.ReleaseComObject(workSheets);
                if (workSheet != null)
                    Marshal.ReleaseComObject(workSheet);
                if (workBooks != null)
                    Marshal.ReleaseComObject(workBooks);
                if (excelApp != null)
                    Marshal.ReleaseComObject(excelApp);
                if (shape != null)
                    Marshal.ReleaseComObject(shape);

            }

            logger.Info(LogMessage.ISuccess, nameof(ExcelOut));

            return rtn;
        }
    }
}

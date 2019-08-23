using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EvidenceCapture.ViewModel.Overray;
using Microsoft.Office.Interop.Excel;

namespace EvidenceCapture.Model
{
    class ReportModel
    {
        public ReportViewModel.FormatType SelectFormat { get; internal set; }
        public List<SnapTreeItem> TargetList { get; internal set; }

        internal async void CreateReport(System.Action callBack)
        {
            var result = await Task.Run<bool>(() =>
            {
                switch (SelectFormat)
                {
                    case ReportViewModel.FormatType.Excel:
                        ExcelOut();
                        break;

                }

                return true;
            });
            callBack();
        }

        private void ExcelOut()
        {

            var destPath = Path.Combine(ApplicationSettings.Instance.OutputDir,
                "Report.xlsx");


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
                                Microsoft.Office.Core.MsoTriState.msoFalse,
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

        }
    }
}

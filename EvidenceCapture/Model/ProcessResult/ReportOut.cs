using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvidenceCapture.Model.ProcessResult
{
    class ReportOut
    {
        internal enum ReportEnum
        {
            Excel,
            PDF,
            HTML
        };

        #region Properties
        public ReportEnum ReportType { get; internal set; }
        public string OutputPath { get; internal set; }
        #endregion



    }
}

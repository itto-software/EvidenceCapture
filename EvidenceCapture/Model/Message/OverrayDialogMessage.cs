using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EvidenceCapture.Model.Message
{
    internal class OverrayDialogMessage
    {
        internal enum OperateType
        {
            Open,
            Close,
            Waiting
        }

        public Action CallBack { get; internal set; }
        internal OperateType Operate { get; set; }
        internal UserControl NewDialog { get; set; }


    }
}

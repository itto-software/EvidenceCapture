using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvidenceCapture.Model.ProcessResult
{
    class RenameResult
    {

        public string NewGroupName { get; internal set; }
        public SnapTreeItem OldNode { get; internal set; }
    }
}

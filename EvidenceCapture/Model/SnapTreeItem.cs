using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvidenceCapture.Model
{
    public class SnapTreeItem : GalaSoft.MvvmLight.ViewModelBase
    {
        public enum FileType {
            File,
            Folder
        }
        private string _name;
        private bool _isExpanded;
        private ObservableCollection<SnapTreeItem> _children;

        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                _isExpanded = value;
                RaisePropertyChanged(nameof(IsExpanded));
            }

        }

        public SnapTreeItem Parent { get; set; }

        public FileType NodeFileType { get; set; }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }
        public ObservableCollection<SnapTreeItem> Children
        {
            get
            {
                return _children;
            }
            set
            {
                if (value != _children)
                {
                    _children = value;
                    RaisePropertyChanged(nameof(Children));
                }
            }
        }

    }
}

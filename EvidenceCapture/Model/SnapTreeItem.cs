using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvidenceCapture.Model
{
    public class SnapTreeItem : GalaSoft.MvvmLight.ViewModelBase, IComparable
    {

        public enum FileType
        {
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
                _name = value;
                RaisePropertyChanged(nameof(Name));
                RaisePropertyChanged(nameof(ID));

            }
        }

        public string ID
        {
            get
            {
                if(this.Parent != null)
                {
                    return $"{this.Parent.ID}/{this.Name}";
                }
                return this.Name;
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

        public int CompareTo(object obj)
        {
            SnapTreeItem i = obj as SnapTreeItem;

            var a = CommonUtility.GetLevelsByStr(this.Name);
            var b = CommonUtility.GetLevelsByStr(i.Name);

            a.Reverse();
            b.Reverse();

            foreach (var index in Enumerable.Range(0, a.Count))
            {
                if (a[index] == b[index])
                    continue;

                if (a[index] > b[index])
                    return 1;
                return -1;

            }
            return 1;
        }
    }
}

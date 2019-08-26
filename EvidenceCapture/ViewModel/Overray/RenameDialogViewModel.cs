using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using EvidenceCapture.Model;
using EvidenceCapture.Model.ProcessResult;
using GalaSoft.MvvmLight.CommandWpf;

namespace EvidenceCapture.ViewModel.Overray
{
    class RenameDialogViewModel : OverrayBase
    {
        private ObservableCollection<LevelItem> _levelArray;
        private SnapTreeItem _targetNode;

        #region Proepties

        public ObservableCollection<SnapTreeItem> TreeItem { get; internal set; }

        public ICommand OkCommand { private set; get; }
        public ICommand LevelChangeCommand { private set; get; }

        public bool IsUnderReset
        {
            get
            {
                return ApplicationSettings.Instance.IsUnderReset;
            }
            set
            {
                if (ApplicationSettings.Instance.IsUnderReset != value)
                {
                    ApplicationSettings.Instance.IsUnderReset = value;
                    RaisePropertyChanged(nameof(IsUnderReset));
                }
            }
        }

        public SnapTreeItem TargetNode
        {
            get
            {
                return _targetNode;
            }
            internal set
            {
                _targetNode = value;
                var levels = CommonUtility.GetLevelsByStr(TargetNode.Name);
                levels.Reverse();

                foreach (var index in Enumerable.Range(0, levels.Count))
                    _levelArray[index].SelectedIndex = levels[index] - 1;

                RaisePropertyChanged(nameof(LevelArray));
            }
        }
        #endregion


        public ObservableCollection<LevelItem> LevelArray
        {
            get
            {
                return _levelArray;
            }
            set
            {
                if (_levelArray != value)
                {
                    _levelArray = value;
                    RaisePropertyChanged(nameof(LevelArray));
                }
            }
        }

        public RenameDialogViewModel()
        {
            _levelArray = new ObservableCollection<LevelItem>();

            var matche = Regex.Matches(ApplicationSettings.Instance.GroupPattern,
               "\\[n\\]");

            var range = Enumerable.Range(1, 99);
            foreach (var i in Enumerable.Range(0, matche.Count))
            {

                var newL = new LevelItem() { LevelValues = new ObservableCollection<int>() };
                _levelArray.Add(newL);
                foreach (var v in range)
                    newL.LevelValues.Add(v);

                newL.SelectedIndex = 0;
            }
            RaisePropertyChanged(nameof(LevelArray));

            // コマンドの初期化
            LevelChangeCommand = new RelayCommand<object>(LevelChangeImpl);
            OkCommand = new RelayCommand(OkImpl, CanOk);

        }

        private void OkImpl()
        {
            try
            {
                var result = new RenameResult();

                List<int> levels = new List<int>();
                foreach (var l in LevelArray)
                {
                    levels.Add(l.LevelValues[l.SelectedIndex]);
                }
                levels.Reverse();
                var gn = CommonUtility.GetGroupNameByLevels(levels);

                result.OldNode = this.TargetNode;
                result.NewGroupName = gn;

                CallBak(result);
            }
            finally
            {
                CloseDialog();
            }
        }

        private bool CanOk()
        {
            if(TreeItem != null && LevelArray != null)
            {
                List<int> levels = new List<int>();
                foreach ( var l in LevelArray)
                {
                    levels.Add(l.LevelValues[l.SelectedIndex]);
                }
                levels.Reverse();
                var gn = CommonUtility.GetGroupNameByLevels(levels);

                return TreeItem.ToList().Count(x => x.Name == gn) == 0;
            }
            return false;
        }

        private void LevelChangeImpl(object changedObj)
        {
            if (IsUnderReset)
            {
                bool startReset = false;
                foreach (var item in LevelArray)
                {
                    if (startReset)
                    {
                        item.SelectedIndex = 0;
                    }

                    if (item == changedObj)
                    {
                        startReset = true;
                    }

                }
            }
        }
    }
}

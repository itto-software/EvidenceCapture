using EvidenceCapture.Model;
using EvidenceCapture.Model.ProcessResult;
using EvidenceCapture.View.Overray;
using EvidenceCapture.ViewModel.Base;
using EvidenceCapture.ViewModel.Overray;
using GalaSoft.MvvmLight.CommandWpf;
using HongliangSoft.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Input;

namespace EvidenceCapture.ViewModel.MainContents
{
    /// <summary>操作パネルのViewModel</summary>
    class OperateControlViewModel : BaseVM, IMainContents
    {
        #region Fields

        private KeyboardHook keyHook;
        private OperateControlModel model;
        private string _selectedOutputPath;
        private KeyboardUpDown _before;
        private SnapTreeItem _selectedNode;
        private ImageProcessingViewModel _imageProcessingVM;

        #endregion

        #region Properties

        /// <summary>出力先の設定</summary>
        public ICommand SetOutputPathCommand { get; private set; }

        /// <summary>グループ追加</summary>
        public ICommand AddGroupCommand { get; private set; }

        /// <summary>ノード削除コマンド</summary>
        public ICommand RemoveNodeCommand { get; private set; }

        /// <summary>レポートダイアログ出力起動コマンド</summary>
        public ICommand LaunchReportDialogCommand { get; private set; }

        /// <summary>リネームダイアログ起動コマンド</summary>
        public ICommand LaunchRenameDialogCommand { get; private set; }

        private bool isKeyBind;

        public string SelectedOutputPath
        {
            get
            {
                return _selectedOutputPath;
            }
            set
            {
                if (value != _selectedOutputPath)
                {
                    _selectedOutputPath = value;
                    RaisePropertyChanged(nameof(SelectedOutputPath));
                }
            }

        }

        /// <summary>ツリーリスト</summary>
        public ObservableCollection<SnapTreeItem> SnapList
        {
            get
            {
                return model.SnapShotTreeSource;
            }
            set
            {
                if (model.SnapShotTreeSource != value)
                {
                    model.SnapShotTreeSource = value;
                    RaisePropertyChanged(nameof(SnapList));
                }
            }
        }

        /// <summary>ツリーの選択中ノード</summary>
        public SnapTreeItem SelectedNode
        {
            get
            {
                return _selectedNode;
            }
            set
            {
                _selectedNode = value;
                RaisePropertyChanged(nameof(SelectedNode));

                if (_selectedNode != null)
                {

                    if (_selectedNode.NodeFileType == SnapTreeItem.FileType.File)
                    {
                        model.CurrentGroup = value.Parent.Name;
                    }
                    if (_selectedNode.NodeFileType == SnapTreeItem.FileType.Folder)
                    {
                        model.CurrentGroup = value.Name;
                    }

                    // ファイルノードならプレビューを更新する
                    if (_selectedNode.NodeFileType == SnapTreeItem.FileType.File)
                    {
                        var targetPath = Path.Combine(
                            model.OutputRoot,
                            _selectedNode.Parent.Name,
                            _selectedNode.Name);

                        ImageProcessingVM.TargetPath = targetPath;
                    }
                    else
                    {
                        ImageProcessingVM.TargetPath = null;
                    }
                }
            }
        }


        public bool CheckedAutoResize
        {
            get
            {
                return model.AutoResize;
            }
            set
            {
                model.AutoResize = value;
                RaisePropertyChanged(nameof(CheckedAutoResize));
            }

        }
        public ImageProcessingViewModel ImageProcessingVM
        {
            get
            {
                return _imageProcessingVM;
            }
            set
            {
                _imageProcessingVM = value;
                RaisePropertyChanged(nameof(ImageProcessingVM));
            }
        }

        #endregion

        public OperateControlViewModel()
        {
            ImageProcessingVM = new ImageProcessingViewModel();
            keyHook = new KeyboardHook();
            keyHook.KeyboardHooked += new KeyboardHookedEventHandler(OnGlobalKeyAction);
            model = new OperateControlModel();
            SelectedOutputPath = model.OutputRoot;

            // コマンドの初期化
            SetOutputPathCommand = new RelayCommand(SetOutputPathImpl);
            RemoveNodeCommand = new RelayCommand(RemoveNodeImpl);
            AddGroupCommand = new RelayCommand(AddGroupImpl);
            LaunchReportDialogCommand = new RelayCommand(LanchReportDialogImpl, CanLaunchReport);
            LaunchRenameDialogCommand = new RelayCommand(LaunchRenameDialogmpl);

            isKeyBind = true;
        }

        /// <summary>リネームダイアログ起動の実装</summary>
        /// <param name="param"></param>
        private void LaunchRenameDialogmpl()
        {
            if (SelectedNode != null && SelectedNode.NodeFileType == SnapTreeItem.FileType.Folder)
            {
                var view = new RenameDialog();
                var vm = view.DataContext as RenameDialogViewModel;
                vm.TargetNode = SelectedNode;
                vm.TreeItem = model.SnapShotTreeSource;
                LaunchDialog(view, RenameCallBack);

            }
        }

        private void RenameCallBack(object obj)
        {
            if (obj is RenameResult)
            {
                var result = obj as RenameResult;
                model.Rename(result.OldNode,
                    result.NewGroupName);
            }
        }

        /// <summary>レポートダイアログ起動の実装</summary>
        private void LanchReportDialogImpl()
        {
            var view = new ReportDialog();
            (view.DataContext as ReportViewModel).TargetList = new List<SnapTreeItem>(this.SnapList.ToList());
            LaunchDialog(view, ReportCallBack);
        }

        private bool CanLaunchReport()
        {
            bool HasChild = SnapList.Count > 0;
            foreach (var item in SnapList)
            {
                HasChild |= item.Children.Count > 0;
            }


            return (this.SnapList != null && HasChild);
        }

        private void ReportCallBack(object result)
        {
            if (result != null && result is ReportOut)
            {
                var rt = result as ReportOut;
                var view = new ReportFinish();
                (view.DataContext as ReportFinishViewModel).ReportOut = rt;
                LaunchDialog(view, null);

            }
        }

        private void AddGroupImpl()
        {
            model.AddLevel();

            var latestNode = model.SnapShotTreeSource.ToList().Find(x => x.Name == model.CurrentGroup);
            if (latestNode != null)
                this.SelectedNode = latestNode;
        }

        private void RemoveNodeImpl()
        {
            if (SelectedNode != null)
            {
                model.RemoveTree(SelectedNode);
                var tmpNode = SelectedNode;

                SnapList = new ObservableCollection<SnapTreeItem>(SnapList);
                SelectedNode = tmpNode;
            }
        }

        private void SetOutputPathImpl()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = model.OutputRoot;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                model.OutputRoot = dialog.SelectedPath;
                SelectedOutputPath = model.OutputRoot;

            }
        }

        /// <summary>キーアクション</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGlobalKeyAction(object sender, KeyboardHookedEventArgs e)
        {
            if (!isKeyBind)
                return;

            var kc = e.KeyCode;
            // 長押し防止でキーアップを挟む
            _before = e.UpDown;
            if (_before == KeyboardUpDown.Up)
            {
                bool isRefresh = true;
                switch (GetKeyType(e))
                {
                    case ApplicationSettingViewModel.FocusType.KeyShortCutScreenCap:
                        model.AddCapture();
                        break;
                    case ApplicationSettingViewModel.FocusType.KeyShortCutApplicationCap:
                        model.AddCapture(false);
                        break;
                    case ApplicationSettingViewModel.FocusType.KeyShortCutG1:
                        model.AddLevel(0);
                        break;
                    case ApplicationSettingViewModel.FocusType.KeyShortCutG2:
                        model.AddLevel(1);
                        break;
                    case ApplicationSettingViewModel.FocusType.KeyShortCutG3:
                        model.AddLevel(2);
                        break;
                    default:
                        isRefresh = false;
                        break;
                }

                var tmpNode = SelectedNode;
                SnapList = new ObservableCollection<SnapTreeItem>(SnapList);
                SelectedNode = tmpNode;

            }
        }

        private ApplicationSettingViewModel.FocusType GetKeyType(KeyboardHookedEventArgs e)
        {
            var ai = ApplicationSettings.Instance;
            var re = new Regex("[\\w]+");

            bool EqualKey(string pattern)
            {
                var isAlt = false;
                var key = "";
                var result = re.Matches(pattern);

                if (result.Count == 0 || result.Count >= 3)
                    return false;

                if (result.Count == 1)
                {
                    key = result[0].ToString();
                }
                if (result.Count == 2)
                {
                    if ("Alt" == result[0].ToString())
                        isAlt = true;
                    key = result[1].ToString();
                }
                return isAlt == e.AltDown && key == e.KeyCode.ToString();
            };


            if (EqualKey(ai.KeyShortCutApplicationCap)) return ApplicationSettingViewModel.FocusType.KeyShortCutApplicationCap;
            if (EqualKey(ai.KeyShortCutScreenCap)) return ApplicationSettingViewModel.FocusType.KeyShortCutScreenCap;
            if (EqualKey(ai.KeyShortCutG1)) return ApplicationSettingViewModel.FocusType.KeyShortCutG1;
            if (EqualKey(ai.KeyShortCutG2)) return ApplicationSettingViewModel.FocusType.KeyShortCutG2;
            if (EqualKey(ai.KeyShortCutG3)) return ApplicationSettingViewModel.FocusType.KeyShortCutG3;

            return ApplicationSettingViewModel.FocusType.None;


        }

        /// <summary>別コンテンツに移動した場合</summary>
        public void DetachContens()
        {
            isKeyBind = false;
        }

        /// <summary>自コンテンツに移動した場合</summary>
        public void AttacheContens()
        {
            isKeyBind = true;

            ImageProcessingVM.Height = ApplicationSettings.Instance.DefaultHeight.ToString();
            ImageProcessingVM.Width = ApplicationSettings.Instance.DefaultWidth.ToString();

        }
    }
}

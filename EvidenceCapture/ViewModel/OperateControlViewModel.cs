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
using System.Windows.Input;

namespace EvidenceCapture.ViewModel
{
    /// <summary>操作パネルのViewModel</summary>
    class OperateControlViewModel : BaseVM
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

                SnapList = new ObservableCollection<SnapTreeItem>(SnapList);

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
            var kc = e.KeyCode;


            // 長押し防止でキーアップを挟む
            _before = e.UpDown;
            if (_before == KeyboardUpDown.Up)
            {
                switch (e.KeyCode)
                {
                    case System.Windows.Forms.Keys.PrintScreen:
                        if (e.AltDown)
                        {
                            model.AddCapture(false);
                        }
                        else
                        {
                            model.AddCapture();
                        }

                        SnapList = new ObservableCollection<SnapTreeItem>(SnapList);


                        break;
                }
            }
        }
    }
}

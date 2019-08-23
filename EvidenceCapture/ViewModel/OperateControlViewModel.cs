using EvidenceCapture.Model;
using EvidenceCapture.ViewModel.Base;
using EvidenceCapture.ViewModel.Overray;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using HongliangSoft.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace EvidenceCapture.ViewModel
{
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

        /// <summary>レポート出力起動コマンド</summary>
        public ICommand LaunchReportDialogCommand { get; private set; }

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
                if (value != _selectedNode)
                {
                    _selectedNode = value;
                    RaisePropertyChanged(nameof(SelectedNode));

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
            LaunchReportDialogCommand = new RelayCommand(
                () =>
                {
                    var view = new View.Overray.ReportDialog();
                    (view.DataContext as ReportViewModel).TargetList = new List<SnapTreeItem>(this.SnapList.ToList());
                    LaunchDialog(view, ReportCallBack);
                }, CanLaunchReport
                );

        }

        private bool CanLaunchReport()
        {
            bool HasChild = SnapList.Count > 0;
            foreach (var item in SnapList)
            {
                HasChild |= item.Children.Count > 0;
            }
            

            return (this.SnapList != null && HasChild );
        }

        private void ReportCallBack()
        {
            // OK後のふるまい
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
                        break;
                }
            }
        }
    }
}

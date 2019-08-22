using EvidenceCapture.Model;
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
    class OperateControlViewModel : ViewModelBase
    {
        #region Fields

        private KeyboardHook keyHook;
        private OperateControlModel model;
        private string _selectedOutputPath;
        private KeyboardUpDown _before;
        private SnapTreeItem _selectedNode;
        private ImageSource _preview;

        #endregion

        #region Properties

        /// <summary>出力先の設定</summary>
        public ICommand SetOutputPathCommand { get; private set; }

        /// <summary>グループ追加</summary>
        public ICommand AddGroupCommand { get; private set; }

        /// <summary>ノード削除コマンド</summary>
        public ICommand RemoveNodeCommand { get; private set; }

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
//                if (value != _selectedNode)
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

                        if (File.Exists(targetPath))
                        {
                            Preview = ImageHelper.GetImageSource(targetPath);
                        }
                    }
                    else
                    {
                        Preview = null;
                    }
                }
            }
        }

        public System.Windows.Media.ImageSource Preview
        {
            get
            {
                return _preview;
            }
            set
            {
                _preview = value;
                RaisePropertyChanged(nameof(Preview));
            }
        }

        #endregion

        public OperateControlViewModel()
        {
            keyHook = new KeyboardHook();
            keyHook.KeyboardHooked += new KeyboardHookedEventHandler(OnGlobalKeyAction);
            model = new OperateControlModel();


            SelectedOutputPath = model.OutputRoot;

            // コマンドの初期化
            SetOutputPathCommand = new RelayCommand(SetOutputPathImpl);
            RemoveNodeCommand = new RelayCommand(RemoveNodeImpl);

            AddGroupCommand = new RelayCommand(AddGroupImpl);

        }

        private void AddGroupImpl()
        {
            model.AddLevel();
        }

        private void RemoveNodeImpl()
        {
            if (SelectedNode != null)
            {
                SnapTreeItem nextFocusNode = null;
                if (SelectedNode.NodeFileType == SnapTreeItem.FileType.File)
                {
                    var nextNodeIndex = (SelectedNode.Parent.Children.ToList().FindIndex(x => x.Name == SelectedNode.Name)) - 1;
                    if (nextNodeIndex > 0)
                    {
                        nextFocusNode = SelectedNode.Parent.Children[nextNodeIndex];
                    }
                }
                else
                {
                    var nextNodeIndex = (model.SnapShotTreeSource.ToList().FindIndex(x => x.Name == SelectedNode.Name)) - 1;
                    if (nextNodeIndex > 0)
                    {
                        nextFocusNode = model.SnapShotTreeSource[nextNodeIndex];
                    }

                }



                if (SelectedNode.NodeFileType == SnapTreeItem.FileType.File)
                {

                    var removeTarget = Path.Combine(SelectedOutputPath, SelectedNode.Parent.Name, SelectedNode.Name);
                    SelectedNode.Parent.Children.Remove(SelectedNode);
                    if (File.Exists(removeTarget))
                        File.Delete(removeTarget);
                }
                else
                {
                    var removeTarget = Path.Combine(SelectedOutputPath, SelectedNode.Name);
                    model.SnapShotTreeSource.Remove(SelectedNode);
                    if (Directory.Exists(removeTarget))
                        Directory.Delete(removeTarget, true);
                }

                if (nextFocusNode != null)
                    SelectedNode = nextFocusNode;

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

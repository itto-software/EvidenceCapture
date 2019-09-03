using EvidenceCapture.Model.Base;
using EvidenceCapture.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EvidenceCapture.Model
{
    class OperateControlModel : ModelBase
    {
        internal enum CaptureKind
        {
            Desktop,
            ActiveWindow,
            WebCamera
        }


        #region Fields

        private List<int> levels = new List<int>();
        private WebCamManager webcam;

        #endregion


        #region Properties

        public ObservableCollection<SnapTreeItem> SnapShotTreeSource { get; set; }

        /// <summary>現在のグループ名</summary>
        public string CurrentGroup { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string OutputRoot
        {
            get
            {
                return ApplicationSettings.Instance.OutputDir;
            }
            set
            {
                ApplicationSettings.Instance.OutputDir = value;
                RefreshTree();
            }
        }


        public bool AutoResize { get; internal set; }


        #endregion

        public OperateControlModel()
        {
            SnapShotTreeSource = new ObservableCollection<SnapTreeItem>();
            LevelInit();

            RefreshTree();
        }


        internal SnapTreeItem AddCapture(OperateControlModel.CaptureKind kind)
        {
            SnapTreeItem newNode = null;

            Bitmap bmp = null;
            switch (kind)
            {
                case CaptureKind.ActiveWindow:
                    bmp = SnapHelper.GetAppCapture();
                    break;
                case CaptureKind.Desktop:
                    bmp = SnapHelper.GetDisplayCapture();
                    break;
                case CaptureKind.WebCamera:
                    bmp = GetCameraCapture();
                    break;
            }



            var outDir = Path.Combine(
                ApplicationSettings.Instance.OutputDir, CurrentGroup);

            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            if (SnapShotTreeSource.ToList().Count(x => x.Name == CurrentGroup) == 0)
            {
                SnapShotTreeSource.Add(new SnapTreeItem()
                {
                    NodeFileType = SnapTreeItem.FileType.Folder,
                    Name = CurrentGroup,
                    IsExpanded = false,
                    Children = new ObservableCollection<SnapTreeItem>()
                });
            }

            var parentNode = SnapShotTreeSource.ToList().Find(x => x.Name == CurrentGroup);

            var lastNo = 1;

            if (parentNode.Children.Count > 0)
            {
                var lastFileName = Path.GetFileNameWithoutExtension(
                    parentNode.Children.Last().Name);
                lastNo = int.Parse(lastFileName) + 1;
            }

            var newName = string.Format("{0:D3}.{1}", lastNo, ApplicationSettings.Instance.ImageFormat);

            parentNode.IsExpanded = true;

            newNode =
                new SnapTreeItem()
                {
                    NodeFileType = SnapTreeItem.FileType.File,
                    Name = newName,
                    IsExpanded = false,
                    Parent = parentNode
                };

            parentNode.Children.Add(newNode);

            var outputpath = Path.Combine(outDir,
                newName);


            if (AutoResize)
            {
                var ins = ApplicationSettings.Instance;
                bmp = ImageHelper.Resize(bmp, ins.DefaultWidth, ins.DefaultHeight);
            }
            bmp.Save(outputpath);
            bmp.Dispose();

            logger.Info(LogMessage.ISuccess, nameof(AddCapture));

            return newNode;
        }

        private Bitmap GetCameraCapture()
        {
            if (webcam != null && webcam.IsRunning)
                return webcam.GetCapture();

            throw new Exception(LogMessage.ECameraNotFound);

        }

        /// <summary>指定したノードを一覧から削除する</summary>
        /// <param name="selectedNode">対象ノード</param>
        /// <returns>削除対象の前、または親ノード</returns>
        internal SnapTreeItem RemoveTree(SnapTreeItem selectedNode)
        {
            SnapTreeItem nextNode = null;
            if (selectedNode.NodeFileType == SnapTreeItem.FileType.File)
            {
                var pnode = selectedNode.Parent;
                int findex = pnode.Children.IndexOf(selectedNode) - 1;

                var removeTarget = Path.Combine(OutputRoot, selectedNode.Parent.Name, selectedNode.Name);
                selectedNode.Parent.Children.Remove(selectedNode);
                if (File.Exists(removeTarget))
                    File.Delete(removeTarget);

                // 移動先ノード取得
                if (findex >= 0)
                    nextNode = pnode.Children[findex];
                else
                    nextNode = pnode;

            }
            else
            {
                int pindex = SnapShotTreeSource.IndexOf(selectedNode) - 1;

                var removeTarget = Path.Combine(OutputRoot, selectedNode.Name);
                SnapShotTreeSource.Remove(selectedNode);
                if (Directory.Exists(removeTarget))
                    Directory.Delete(removeTarget, true);
                UpdateLatestLevel();

                // 移動先ノード取得
                if (pindex >= 0)
                    nextNode = SnapShotTreeSource[pindex];

            }
            return nextNode;
        }

        private void UpdateLatestLevel()
        {
            if (SnapShotTreeSource.Count > 0)
            {
                var lastnode = SnapShotTreeSource.Last();

                var matche = Regex.Matches(lastnode.Name,
                    "[0-9+]");
                levels.Clear();


                foreach (var m in matche)
                {
                    int newV = int.Parse(m.ToString());
                    levels.Add(newV);
                }
                levels.Reverse();
                LevelToGroupName();
            }
            else
            {
                LevelInit();
            }
        }

        private void LevelInit()
        {
            var matche = Regex.Matches(ApplicationSettings.Instance.GroupPattern,
                "\\[n\\]");

            levels.Clear();
            foreach (var i in Enumerable.Range(0, matche.Count))
            {
                levels.Add(1);
            }
            LevelToGroupName();

            SnapShotTreeSource.Add(new SnapTreeItem()
            {
                NodeFileType = SnapTreeItem.FileType.Folder,
                Name = CurrentGroup,
                IsExpanded = true,
                Children = new ObservableCollection<SnapTreeItem>()
            });
        }

        private void LevelToGroupName()
        {
            CurrentGroup = CommonUtility.GetGroupNameByLevels(levels);

        }


        /// <summary>ツリーにグループノードを追加する</summary>
        /// <param name="level">対象レベル（0から）</param>
        /// <returns>追加グループのノード</returns>
        internal SnapTreeItem AddGroupNode(int level = 0)
        {
            levels = CommonUtility.GetLevelsByStr(CurrentGroup);

            if (level > 0)
            {
                // 下位レベルをリセット
                foreach (int index in Enumerable.Range(0, level))
                    levels[index] = 1;
            }

            while (true)
            {
                levels[level]++;
                LevelToGroupName();
                if (SnapShotTreeSource.ToList().Count(x => x.Name == CurrentGroup) == 0)
                    break;


            }
            var newItem = new SnapTreeItem()
            {
                NodeFileType = SnapTreeItem.FileType.Folder,
                Name = CurrentGroup,
                IsExpanded = true,
                Children = new ObservableCollection<SnapTreeItem>()
            };

            SnapShotTreeSource.Add(newItem);

            var sorted = SnapShotTreeSource.ToList();
            sorted.Sort();
            SnapShotTreeSource.Clear();
            sorted.ForEach(x => SnapShotTreeSource.Add(x));

            return newItem;

        }

        internal void Rename(SnapTreeItem oldNode, string newGroupName)
        {
            var sourcePath = Path.Combine(OutputRoot, oldNode.Name);
            var destPath = Path.Combine(OutputRoot, newGroupName);

            if (Directory.Exists(sourcePath))
            {
                Directory.Move(sourcePath, destPath);
            }
            oldNode.Name = newGroupName;

            var sorted = SnapShotTreeSource.ToList();
            sorted.Sort();
            SnapShotTreeSource.Clear();

            sorted.ForEach(x => SnapShotTreeSource.Add(x));


        }

        private void RefreshTree()
        {
            SnapShotTreeSource.Clear();
            if (Directory.Exists(OutputRoot))
            {
                string matchestr = ApplicationSettings.Instance.GroupPattern;

                matchestr = matchestr.Replace("[n]", "[0-9]+");
                var re = new Regex(matchestr);
                var fre = new Regex("[0-9]{3}.[png|jpg|bmp]");

                var searchTargetDirs = Directory.EnumerateDirectories(OutputRoot);
                foreach (var parentDir in searchTargetDirs)
                {
                    if (re.IsMatch(parentDir))
                    {
                        var parentName = Path.GetFileName(parentDir);

                        var parentNode = new SnapTreeItem()
                        {
                            NodeFileType = SnapTreeItem.FileType.Folder,
                            Name = parentName,
                            IsExpanded = false,
                            Children = new ObservableCollection<SnapTreeItem>()
                        };
                        SnapShotTreeSource.Add(parentNode);

                        var searchTargetFiles = Directory.EnumerateFiles(parentDir);
                        foreach (var file in searchTargetFiles)
                        {
                            var fileName = Path.GetFileName(file);
                            if (fre.IsMatch(file))
                            {
                                parentNode.Children.Add(
                                new SnapTreeItem()
                                {
                                    NodeFileType = SnapTreeItem.FileType.File,
                                    Name = fileName,
                                    IsExpanded = false,
                                    Parent = parentNode
                                });

                            }
                        }
                    }
                }


                var sorted = SnapShotTreeSource.ToList();
                sorted.Sort();
                SnapShotTreeSource.Clear();

                sorted.ForEach(x => SnapShotTreeSource.Add(x));



            }
        }

        internal void CreateCamera()
        {
            webcam = new WebCamManager();

            var targetDevice = ApplicationSettings.Instance.DefaultCamDevice;
            var dcs = webcam.GetDevices();
            var deviceName = dcs.ToList().Find(x => x == targetDevice);
            if (deviceName != null)
            {
                webcam.SetDevice(deviceName);
                webcam.Start();
                
            }
            else
            {
                throw new Exception(LogMessage.ECameraNotFound);
            }

        }

        internal void DisposeCamera()
        {
            if (webcam != null)
            {
                webcam.Dispose();
            }

        }
    }
}

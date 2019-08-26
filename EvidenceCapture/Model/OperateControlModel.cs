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
    class OperateControlModel
    {
        #region Fields

        private List<int> levels = new List<int>();

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


        internal void AddCapture(bool isDisplay = true)
        {
            var bmp = (isDisplay) ? SnapHelper.GetDisplayCapture() :
                SnapHelper.GetAppCapture();

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
            parentNode.Children.Add(
                new SnapTreeItem()
                {
                    NodeFileType = SnapTreeItem.FileType.File,
                    Name = newName,
                    IsExpanded = false,
                    Parent = parentNode
                });


            var outputpath = Path.Combine(outDir,
                newName);


            if (AutoResize)
            {
                var ins = ApplicationSettings.Instance;
                bmp = ImageHelper.Resize(bmp, ins.DefaultWidth, ins.DefaultHeight);
            }
            bmp.Save(outputpath);
            bmp.Dispose();

        }

        internal void RemoveTree(SnapTreeItem selectedNode)
        {
            if (selectedNode.NodeFileType == SnapTreeItem.FileType.File)
            {
                var removeTarget = Path.Combine(OutputRoot, selectedNode.Parent.Name, selectedNode.Name);
                selectedNode.Parent.Children.Remove(selectedNode);
                if (File.Exists(removeTarget))
                    File.Delete(removeTarget);
            }
            else
            {
                var removeTarget = Path.Combine(OutputRoot, selectedNode.Name);
                SnapShotTreeSource.Remove(selectedNode);
                if (Directory.Exists(removeTarget))
                    Directory.Delete(removeTarget, true);
                UpdateLatestLevel();
            }
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


        internal void AddLevel(int level = 0)
        {
            levels = CommonUtility.GetLevelsByStr(CurrentGroup);

            while (true)
            {
                levels[level]++;
                LevelToGroupName();
                if (SnapShotTreeSource.ToList().Count(x => x.Name == CurrentGroup) == 0)
                    break;

            }

            SnapShotTreeSource.Add(new SnapTreeItem()
            {
                NodeFileType = SnapTreeItem.FileType.Folder,
                Name = CurrentGroup,
                IsExpanded = true,
                Children = new ObservableCollection<SnapTreeItem>()
            });

            var sorted = SnapShotTreeSource.ToList();
            sorted.Sort();
            SnapShotTreeSource.Clear();
            sorted.ForEach(x => SnapShotTreeSource.Add(x));

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

                matchestr = matchestr.Replace("n", "1-9");
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
            }
        }

    }
}

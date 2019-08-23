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
            }
        }

        public bool AutoResize { get; internal set; }


        #endregion

        public OperateControlModel()
        {
            SnapShotTreeSource = new ObservableCollection<SnapTreeItem>();
            LevelInit();
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
                    IsExpanded = true,
                    Children = new ObservableCollection<SnapTreeItem>()
                });
            }

            var parentNode = SnapShotTreeSource.ToList().Find(x => x.Name == CurrentGroup);

            var lastNo = parentNode.Children.Count + 1;
            var newName = string.Format("{0:D3}.{1}", lastNo, ApplicationSettings.Instance.ImageFormat);

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
            var sourceStr = ApplicationSettings.Instance.GroupPattern;

            var re = new Regex("\\[n\\]");

            levels.Reverse();
            levels.ForEach(
                level =>
                {
                    sourceStr = re.Replace(sourceStr, level.ToString(), 1);

                });
            levels.Reverse();
            CurrentGroup = sourceStr;
        }


        internal void AddLevel(int level = 0)
        {
            levels[level]++;
            LevelToGroupName();

            SnapShotTreeSource.Add(new SnapTreeItem()
            {
                NodeFileType = SnapTreeItem.FileType.Folder,
                Name = CurrentGroup,
                IsExpanded = true,
                Children = new ObservableCollection<SnapTreeItem>()
            });
        }

    }
}

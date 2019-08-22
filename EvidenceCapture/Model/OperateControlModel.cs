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


        #endregion

        public OperateControlModel()
        {
            SnapShotTreeSource = new ObservableCollection<SnapTreeItem>();
            LevelInit();
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


        #region External DLL API
        private const int SRCCOPY = 13369376;
        private const int CAPTUREBLT = 1073741824;


        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        private static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern int BitBlt(IntPtr hDestDC,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hSrcDC,
            int xSrc,
            int ySrc,
            int dwRop);


        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hwnd,
            ref RECT lpRect);

        #endregion

        internal void AddCapture(bool isDisplay = true)
        {
            var bmp = (isDisplay) ? GetDisplayCapture() :
                GetAppCapture();

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
            bmp.Save(outputpath);


        }

        private Bitmap GetAppCapture()
        {
            //アクティブなウィンドウのデバイスコンテキストを取得
            IntPtr hWnd = GetForegroundWindow();
            IntPtr winDC = GetWindowDC(hWnd);
            //ウィンドウの大きさを取得
            RECT winRect = new RECT();
            GetWindowRect(hWnd, ref winRect);
            //Bitmapの作成
            Bitmap bmp = new Bitmap(winRect.right - winRect.left,
                winRect.bottom - winRect.top);
            //Graphicsの作成
            Graphics g = Graphics.FromImage(bmp);
            //Graphicsのデバイスコンテキストを取得
            IntPtr hDC = g.GetHdc();
            //Bitmapに画像をコピーする
            BitBlt(hDC, 0, 0, bmp.Width, bmp.Height,
                winDC, 0, 0, SRCCOPY);
            //解放
            g.ReleaseHdc(hDC);
            g.Dispose();
            ReleaseDC(hWnd, winDC);

            return bmp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Bitmap GetDisplayCapture()
        {
            var all_width = 0;
            foreach (var scr in Screen.AllScreens)
            {
                all_width += scr.Bounds.Width;
            }


            //プライマリモニタのデバイスコンテキストを取得
            IntPtr disDC = GetDC(IntPtr.Zero);
            //Bitmapの作成
            Bitmap bmp = new Bitmap(all_width,
                Screen.PrimaryScreen.Bounds.Height);
            //Graphicsの作成
            Graphics g = Graphics.FromImage(bmp);
            //Graphicsのデバイスコンテキストを取得
            IntPtr hDC = g.GetHdc();
            //Bitmapに画像をコピーする
            BitBlt(hDC, 0, 0, all_width, bmp.Height,
                disDC, 0, 0, SRCCOPY);
            //解放
            g.ReleaseHdc(hDC);
            g.Dispose();
            ReleaseDC(IntPtr.Zero, disDC);

            return bmp;
        }

    }
}

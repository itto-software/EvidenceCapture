using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EvidenceCapture.Model
{
    static class SnapHelper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

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

        [DllImport("user32.dll")]
        private static extern int GetClientRect(IntPtr hwnd,
            ref RECT lpRect);


        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, out RECT pvAttribute, int cbAttribute);


        [Flags]
        public enum DwmWindowAttribute : uint
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_LAST
        }


        static internal Bitmap GetAppCapture()
        {
            //アクティブなウィンドウのデバイスコンテキストを取得
            IntPtr hWnd = GetForegroundWindow();
            IntPtr winDC = IntPtr.Zero;
            RECT winRect = new RECT();

            var offsetX = 0;
            var offsetY = 0;
            Bitmap bmp = null;

            // アプリケーションからエアロフレームを考慮したサイズを取得
            if (ApplicationSettings.Instance.IsWindowCapture)
            {
                DwmGetWindowAttribute(
                    hWnd,
                    DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS,
                    out var bounds,
                    Marshal.SizeOf(typeof(RECT)));
                GetWindowRect(hWnd, ref winRect);
                offsetX = bounds.left;
                offsetY = bounds.top;
                bmp = new Bitmap(bounds.right - bounds.left,
                bounds.bottom - bounds.top);
            }
            else
            {
                // アプリケーションのクライアント領域を取得
                GetClientRect(hWnd, ref winRect);
                bmp = new Bitmap(winRect.right - winRect.left,
                    winRect.bottom - winRect.top);

            }

            Graphics g = Graphics.FromImage(bmp);
            IntPtr hDC = g.GetHdc();

            if (ApplicationSettings.Instance.IsWindowCapture)
            {
                // デスクトップ全体から、エアロフレーム領域分をコピー
                winDC = GetWindowDC(hWnd);
                IntPtr disDC = GetDC(IntPtr.Zero);
                BitBlt(hDC, 0, 0, bmp.Width, bmp.Height,
                    disDC, offsetX, offsetY, SRCCOPY);
            }
            else
            {
                // クライアント領域をコピー
                winDC = GetDC(hWnd);
                BitBlt(hDC, 0, 0, bmp.Width, bmp.Height,
                    winDC, 0, 0, SRCCOPY);

            }
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
        static internal Bitmap GetDisplayCapture()
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

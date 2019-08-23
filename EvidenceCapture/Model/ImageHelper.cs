using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvidenceCapture.Model
{
    static class ImageHelper
    {
        internal enum ResizeMode
        {
            Width,
            Height,
            MinSize
        }

        public static System.Windows.Media.ImageSource GetImageSource(string FilePath)
        {
            try
            {
                if (string.IsNullOrEmpty(FilePath))
                {
                    return null;
                }
                // ファイルを占有しない
                MemoryStream data = new MemoryStream(File.ReadAllBytes(FilePath));
                System.Windows.Media.Imaging.WriteableBitmap bmp = new System.Windows.Media.Imaging.WriteableBitmap(System.Windows.Media.Imaging.BitmapFrame.Create(data));
                bmp.Freeze();
                data.Close();
                return bmp;
            }
            catch (Exception exp)
            {
                //                logger.Error(exp, NLogHelper.GetMessageWithID("LE0003"));
                return null;
            }
        }

        internal static Bitmap Resize(Bitmap sourceBmp, int targetWidth, int targetHeight, ResizeMode mode = ResizeMode.MinSize)
        {
            var oldWidth = sourceBmp.Width;
            var oldHeight = sourceBmp.Height;


            var scale = (double)targetWidth / (double)oldWidth;
            var scale2 = (double)targetHeight / (double)oldHeight;

            // モード判定
            switch (mode)
            {
                case ResizeMode.Width:  // 幅指定優先
                    break;
                case ResizeMode.Height: // 高さ指定優先
                    scale = scale2;
                    break;

                case ResizeMode.MinSize: // より小さい幅優先
                    if (scale2 < scale) scale = scale2;
                    break;
            }

            var newWidth = (int)(oldWidth * scale);
            var newHeight = (int)(oldHeight * scale);
            var newBmp = new Bitmap(newWidth, newHeight);

            using (var g = Graphics.FromImage(newBmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(sourceBmp, 0, 0, newWidth, newHeight);
            }

            return newBmp;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvidenceCapture.Model
{
    static class ImageHelper
    {

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
    }
}

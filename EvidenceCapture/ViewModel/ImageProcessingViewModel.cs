using System;
using System.Drawing;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using EvidenceCapture.Model;
using EvidenceCapture.ViewModel.Base;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace EvidenceCapture.ViewModel
{
    /// <summary>画像編集パネルのViewModel</summary>
    public class ImageProcessingViewModel : BaseVM
    {
        #region Fields

        private ImageSource _preview;
        private string _targetPath;
        private int _newWidth;
        private int _newHeight;
        private string _height;
        private string _width;

        bool isEnableWidth = true;
        bool isEnableHeight = true;

        #endregion


        #region Properties

        public ICommand ResizeWidthCommand { get; private set; }

        public ICommand ResizeHeightCommand { get; private set; }

        public bool ControlEnable
        {
            get
            {
                return (_targetPath != null);
            }
        }

        public string TargetPath
        {
            get
            {
                return _targetPath;
            }
            set
            {
                if (value != _targetPath)
                {
                    _targetPath = value;
                    if (File.Exists(_targetPath))
                    {
                        Preview = ImageHelper.GetImageSource(_targetPath);
                    }
                    else
                    {
                        Preview = null;
                    }
                    RefreshSizeInfo(_targetPath);
                }
                RaisePropertyChanged(nameof(ControlEnable));
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

        public string NewWidth
        {
            get
            {
                return _newWidth.ToString();
            }
            set
            {
                isEnableWidth = int.TryParse(value, out _newWidth);
                RaisePropertyChanged(nameof(NewWidth));
            }
        }

        public string NewHeight
        {
            get
            {
                return _newHeight.ToString();
            }
            set
            {
                isEnableHeight = int.TryParse(value, out _newHeight);
                RaisePropertyChanged(nameof(NewHeight));


            }
        }


        public string Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
                RaisePropertyChanged(nameof(Width));
            }
        }

        public string Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
                RaisePropertyChanged(nameof(Height));
            }
        }


        #endregion


        private void RefreshSizeInfo(string targetPath = null)
        {
            if (targetPath != null && File.Exists(targetPath))
            {
                using (var bmp = new Bitmap(targetPath))
                {
                    Width = bmp.Width.ToString();
                    Height = bmp.Height.ToString();
                }

            }
            else
            {
                Width = "-";
                Height = "-";
            }
        }

        public ImageProcessingViewModel()
        {
            ResizeWidthCommand = new RelayCommand(ResizeWidthImpl, CanResizeWidth);
            ResizeHeightCommand = new RelayCommand(ResizeHeightImpl, CanResizeHeight);

            NewWidth = ApplicationSettings.Instance.DefaultWidth.ToString();
            NewHeight = ApplicationSettings.Instance.DefaultHeight.ToString();

            RefreshSizeInfo();
        }

        private bool CanResizeHeight()
        {
            return isEnableHeight;
        }

        private bool CanResizeWidth()
        {
            return isEnableWidth;
        }

        private void ResizeHeightImpl()
        {
            ImageResize(false);
        }

        private void ResizeWidthImpl()
        {
            ImageResize(true);
        }

        private void ImageResize(bool isWidth)
        {
            if (File.Exists(TargetPath))
            {
                var bmp = new Bitmap(TargetPath);
                var newBmp = ImageHelper.Resize(bmp, _newWidth, _newHeight,
                    (isWidth) ? ImageHelper.ResizeMode.Width : ImageHelper.ResizeMode.Height);

                bmp.Dispose();
                newBmp.Save(TargetPath);
                newBmp.Dispose();

                Preview = ImageHelper.GetImageSource(TargetPath);
                RefreshSizeInfo(TargetPath);
            }
            else
            {
                // todo : 要エラーハンドリング
            }
        }
    }
}
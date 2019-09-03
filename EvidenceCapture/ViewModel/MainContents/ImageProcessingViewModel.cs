using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using EvidenceCapture.Model;
using EvidenceCapture.Properties;
using EvidenceCapture.ViewModel.Base;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace EvidenceCapture.ViewModel.MainContents
{
    /// <summary>画像編集パネルのViewModel</summary>
    public class ImageProcessingViewModel : BaseVM
    {
        enum RetouchType
        {
            Trim,
            Mask,
            HighLight
        }


        #region Fields

        private ImageSource _preview;
        private string _targetPath;
        private int _newWidth;
        private int _newHeight;
        private string _height;
        private string _width;

        bool isEnableWidth = true;
        bool isEnableHeight = true;
        private bool _isDragble;
        private System.Windows.Point _dragedPoint;
        private System.Windows.Size _dragedSize;

        #endregion


        #region Properties

        public ICommand ResizeWidthCommand { get; private set; }
        public ICommand ResizeHeightCommand { get; private set; }
        public ICommand ImageTrimCommand { get; private set; }
        public ICommand ImageMaskCommand { get; private set; }
        public ICommand ImageHighLightCommand { get; private set; }

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
                try
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
                catch (Exception e)
                {
                    logger.Error(e);
                    MessageDialog(e);
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

        public bool IsDragble
        {
            get
            {
                return _isDragble;
            }
            set
            {
                _isDragble = value;
                RaisePropertyChanged(nameof(IsDragble));
            }
        }

        public System.Windows.Size DragedSize
        {
            get
            {
                return _dragedSize;
            }
            set
            {
                _dragedSize = value;
                RaisePropertyChanged(nameof(DragedSize));
                RaisePropertyChanged(nameof(SizeStr));
            }
        }
        public System.Windows.Point DragedPoint
        {
            get
            {
                return _dragedPoint;
            }
            set
            {
                _dragedPoint = value;
                RaisePropertyChanged(nameof(DragedPoint));
                RaisePropertyChanged(nameof(PointStr));
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


        public string PointStr
        {
            get
            {
                return $"({(int)DragedPoint.X,4}, {(int)DragedPoint.Y,4})";
            }
        }



        public string SizeStr
        {
            get
            {
                return $"{(int)DragedSize.Width,4} * {(int)DragedSize.Height,4}";
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
            ImageTrimCommand = new RelayCommand(() => { ImageRetouchImpl(RetouchType.Trim); }, CanImageRetouch);
            ImageMaskCommand = new RelayCommand(() => { ImageRetouchImpl(RetouchType.Mask); }, CanImageRetouch);
            ImageHighLightCommand = new RelayCommand(() => { ImageRetouchImpl(RetouchType.HighLight); }, CanImageRetouch);


            NewWidth = ApplicationSettings.Instance.DefaultWidth.ToString();
            NewHeight = ApplicationSettings.Instance.DefaultHeight.ToString();

            RefreshSizeInfo();

            IsDragble = true;
        }

        private void ImageRetouchImpl(RetouchType type)
        {
            if (File.Exists(TargetPath))
            {
                try
                {
                    var bmp = new Bitmap(TargetPath);
                    Bitmap newBmp = null;
                    switch (type)
                    {
                        case RetouchType.Trim:
                            newBmp = ImageHelper.Trim(bmp, DragedPoint, DragedSize);
                            break;
                        case RetouchType.Mask:
                            newBmp = ImageHelper.Mask(bmp, DragedPoint, DragedSize);
                            break;
                        case RetouchType.HighLight:
                            newBmp = ImageHelper.HighLight(bmp, DragedPoint, DragedSize);
                            break;
                    }

                    bmp.Dispose();
                    newBmp.Save(TargetPath);
                    newBmp.Dispose();

                    Preview = ImageHelper.GetImageSource(TargetPath);
                    RefreshSizeInfo(TargetPath);

                    logger.Info(LogMessage.ISuccess, $"{nameof(ImageRetouchImpl)} with {type.ToString()} param");
                }
                catch (Exception e)
                {
                    logger.Debug(LogMessage.DParams, nameof(RetouchType), type.ToString());
                    logger.Debug(LogMessage.DParams, nameof(TargetPath), TargetPath);
                    logger.Error(e);
                    MessageDialog(MessageType.Error, e.Message);

                }
            }
            else
            {
                var msg = string.Format(LogMessage.EFileNotFound, TargetPath);
                logger.Error(msg);
                MessageDialog(MessageType.Error, msg);
            }
        }

        private bool CanImageRetouch()
        {
            return !DragedSize.IsEmpty && (int)(DragedSize.Width) > 0 && (int)(DragedSize.Height) > 0;
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
            try
            {
                ImageResize(false);
            }
            catch (Exception e)
            {
                logger.Error(e);
                MessageDialog(MessageType.Error, e.Message);
            }
        }

        private void ResizeWidthImpl()
        {
            try
            {
                ImageResize(true);
            }
            catch (Exception e)
            {
                logger.Error(e);
                MessageDialog(MessageType.Error, e.Message);
            }
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
                var msg = string.Format(LogMessage.EFileNotFound, TargetPath);
                logger.Error(msg);
                throw new FileNotFoundException(msg);
            }
        }
    }
}
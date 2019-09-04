using EvidenceCapture.Model;
using EvidenceCapture.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace EvidenceCapture.ViewModel.MainContents
{
    class WebCamSettingViewModel : BaseVM, IMainContents, IDisposable
    {
        #region Fields

        private WebCamManager webcam = null;
        private ObservableCollection<string> _cameraDevices = new ObservableCollection<string>();
        private string _targetDevice;
        private ImageSource _cameraPreview;

        #endregion


        public string TargetDevice
        {
            get
            {
                return _targetDevice;
            }
            set
            {
                _targetDevice = value;
                ApplicationSettings.Instance.DefaultCamDevice = _targetDevice;
                RaisePropertyChanged(nameof(TargetDevice));

                webcam.SetDevice(_targetDevice);
                webcam.AttachCallBack(OnCameraNewFrame);
                webcam.Start();
            }

        }

        public System.Windows.Media.ImageSource CameraPreview
        {
            get
            {
                return _cameraPreview;
            }
            set
            {
                _cameraPreview = value;
                RaisePropertyChanged(nameof(CameraPreview));
            }
        }

        private void OnCameraNewFrame(Bitmap snap)
        {
            if (snap != null)
                CameraPreview = ImageHelper.GetImageSource(snap);
        }

        public ObservableCollection<string> CameraDevices
        {
            get
            {
                return _cameraDevices;
            }
            set
            {
                _cameraDevices = value;
                RaisePropertyChanged(nameof(CameraDevices));
            }
        }

        public void AttacheContens()
        {
            webcam = new Model.WebCamManager();
            var newdaray = new ObservableCollection<string>();
            foreach (var dn in webcam.GetDevices())
                newdaray.Add(dn);

            CameraDevices = newdaray;

            RaisePropertyChanged(nameof(CameraDevices));

            var dd = ApplicationSettings.Instance.DefaultCamDevice;
            if (newdaray.ToList().Count(x => x == dd) > 0)
                TargetDevice = dd;
            else if (CameraDevices.Count > 0)
                TargetDevice = CameraDevices[0];
            else
                MessageDialog(MessageType.Error, "");
        }

        public void DetachContens()
        {
            if (webcam != null)
                webcam.Dispose();
        }

        public void Dispose()
        {
            DetachContens();
        }
    }
}

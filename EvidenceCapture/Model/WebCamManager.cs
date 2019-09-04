using AForge.Video;
using AForge.Video.DirectShow;
using EvidenceCapture.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvidenceCapture.Model
{
    class WebCamManager : Base.ModelBase, IDisposable
    {
        private VideoCaptureDevice device;
        private Bitmap latestimg;
        private Action<Bitmap> _callBack;
        private readonly object semaphore = new object();

        public bool IsRunning
        {
            get
            {
                if (device == null)
                    return false;
                return device.IsRunning;
            }
        }

        public void SetDevice(string dn)
        {

            if (IsRunning)
                Stop();

            if (device != null)
                device.NewFrame -= NewFrameEvent;

            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo filterinfo in videoDevices)
            {
                if (filterinfo.Name.Equals(dn))
                {
                    device = new VideoCaptureDevice(filterinfo.MonikerString);
                    device.NewFrame += NewFrameEvent;
                    return;
                }
            }
            throw new ArgumentException("is not found.", dn);
        }

        public void AttachCallBack(Action<Bitmap> callbak)
        {
            _callBack = callbak;
        }


        public Bitmap GetCapture()
        {
            lock (semaphore)
            {
                return latestimg;
            }
        }


        public void Start()
        {
            if (device == null)
                throw new ArgumentException("is null", nameof(device));

            if (device.IsRunning)
                return;

            device.Start();
        }

        private void NewFrameEvent(object sender, NewFrameEventArgs eventArgs)
        {
            lock (semaphore)
            {
                if (latestimg != null)
                    latestimg.Dispose();
                latestimg = (Bitmap)eventArgs.Frame.Clone();
                _callBack?.Invoke(latestimg);
            }
        }

        public void Stop()
        {
            if (device == null)
                throw new ArgumentException("is null", nameof(device));

            device.Stop();
        }

        public IEnumerable<String> GetDevices()
        {

            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo filterinfo in videoDevices)
                yield return filterinfo.Name;

        }

        public void Dispose()
        {
            if (device == null)
                return;
            Stop();
            device.NewFrame -= NewFrameEvent;
            device = null;
        }
    }
}

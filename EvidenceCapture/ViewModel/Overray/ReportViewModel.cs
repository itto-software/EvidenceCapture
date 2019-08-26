using EvidenceCapture.Model;
using EvidenceCapture.Model.Overray;
using EvidenceCapture.Properties;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EvidenceCapture.ViewModel.Overray
{
    class ReportViewModel : OverrayBase
    {
        private ReportModel model = null;

        public enum FormatType
        {
            Excel,
            PDF,
            HTML
        }

        public List<SnapTreeItem> TargetList
        {
            get
            {
                return model.TargetList;
            }
            set
            {
                model.TargetList = value;
            }
        }

        public FormatType SelectFormat
        {
            get
            {
                return model.SelectFormat;
            }
            set
            {
                model.SelectFormat = value;
                RaisePropertyChanged(nameof(SelectFormat));
            }
        }

        public ICommand OkCommand { get; set; }

        public ReportViewModel() : base()
        {
            model = new ReportModel();
            OkCommand = new RelayCommand(
                OkCommandImpl, CanOkCommand);

            SelectFormat = CommonUtility.GetEnum<FormatType>(ApplicationSettings.Instance.OutputFormat);
        }

        private bool CanOkCommand()
        {
            return Directory.Exists(ApplicationSettings.Instance.OutputDir);
        }

        private void OkCommandImpl()
        {
            ApplicationSettings.Instance.OutputFormat = (int)SelectFormat;
            ToWaiting();
            model.CreateReport(CompleteCallBack, failedCallBack);

        }

        private void failedCallBack(Exception e)
        {


            MessageDialog(MessageType.Error,
                string.Format(Resources.ErrUnexpected,
                    e.Message));
        }

        private void CompleteCallBack(object result)
        {
            CloseDialog();
            CallBak(result);

        }
    }
}

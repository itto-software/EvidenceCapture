using EvidenceCapture.Model;
using EvidenceCapture.Model.ProcessResult;
using EvidenceCapture.Properties;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EvidenceCapture.ViewModel.Overray
{

    class ReportFinishViewModel : OverrayBase
    {
        #region Fields

        private string _dialogCaption;
        private ReportOut _reportOut;

        #endregion

        #region Proeprties

        /// <summary>ファイル表示コマンド</summary>
        public ICommand ShowFileCommand { get; private set; }

        /// <summary>ファイル表示コマンド</summary>
        public ICommand ShowDirCommand { get; private set; }

        /// <summary>レポート結果オブジェクト</summary>
        public ReportOut ReportOut
        {
            get
            {
                return _reportOut;
            }
            set
            {
                if (_reportOut != value)
                {
                    _reportOut = value;
                    DialogCaption = string.Format(Resources.ReportFinishCaption, _reportOut.OutputPath);
                    RaisePropertyChanged(nameof(ReportOut));
                }
            }
        }

        /// <summary>ダイアログの表示メッセージ（可変）</summary>
        public String DialogCaption
        {
            get
            {
                return _dialogCaption;
            }
            set
            {
                if (_dialogCaption != value)
                {
                    _dialogCaption = value;
                    RaisePropertyChanged(nameof(DialogCaption));
                }
            }
        }
        #endregion

        public ReportFinishViewModel()
        {
            DialogCaption = string.Format(Resources.ReportFinishCaption, "xxxx");

            ShowFileCommand = new RelayCommand(ShowFileImpl);
            ShowDirCommand = new RelayCommand(ShowDirImpl);
        }

        private void ShowDirImpl()
        {
            var path = Path.GetDirectoryName(ReportOut.OutputPath);
            CommonUtility.Explorer(path);
        }

        private void ShowFileImpl()
        {
            var path = ReportOut.OutputPath;
            CommonUtility.Explorer(path);
        }
    }
}

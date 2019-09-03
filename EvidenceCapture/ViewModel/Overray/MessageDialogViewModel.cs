using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvidenceCapture.ViewModel.Base;

namespace EvidenceCapture.ViewModel.Overray
{
    class MessageDialogViewModel : OverrayBase
    {

        private string _title;
        private string _message;
        private MessageType _mtype;

        public MessageType MType
        {
            get
            {
                return _mtype;
            }
            set
            {
                _mtype = value;
                RaisePropertyChanged(nameof(MType));
            }

        }

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                RaisePropertyChanged(nameof(Title));
            }
        }
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                RaisePropertyChanged(nameof(Message));
            }
        }


        public MessageDialogViewModel()
        {
            Title = "Title";
            Message = "Message";
            MType = MessageType.Info;
        }

    }
}

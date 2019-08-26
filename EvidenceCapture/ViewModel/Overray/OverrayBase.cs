using EvidenceCapture.Model.Message;
using EvidenceCapture.ViewModel.Base;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EvidenceCapture.ViewModel.Overray
{
    public class OverrayBase : BaseVM
    {
        public virtual Action<object> CallBak { set; get; }


        public virtual ICommand CancelCommand { set; get; }

        public OverrayBase() : base()
        {
            CancelCommand = new RelayCommand(
                () =>
                {
                    MessengerInstance.Send(new OverrayDialogMessage() { Operate = OverrayDialogMessage.OperateType.Close });
                });
        }
    }
}

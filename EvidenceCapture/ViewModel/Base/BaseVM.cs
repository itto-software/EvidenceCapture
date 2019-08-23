using EvidenceCapture.Model.Message;
using EvidenceCapture.ViewModel.Overray;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EvidenceCapture.ViewModel.Base
{
    public class BaseVM : GalaSoft.MvvmLight.ViewModelBase
    {
        protected void LaunchDialog(UserControl newDialog, Action callback)
        {
            if (newDialog.DataContext is OverrayBase)
            {
                var dc = newDialog.DataContext as OverrayBase;
                dc.CallBak = callback;

                MessengerInstance.Send(new OverrayDialogMessage()
                {
                    Operate = OverrayDialogMessage.OperateType.Open,
                    NewDialog = newDialog,
                    CallBack = callback
                });
            }
            else
            {
                // todo エラーハンドリング
            }
        }

        protected void ToWaiting()
        {
            MessengerInstance.Send(new OverrayDialogMessage()
            {
                Operate = OverrayDialogMessage.OperateType.Waiting
            });
        }

        protected void CloseDialog()
        {
            MessengerInstance.Send(new OverrayDialogMessage()
            {
                Operate = OverrayDialogMessage.OperateType.Close
            });
        }

    }
}

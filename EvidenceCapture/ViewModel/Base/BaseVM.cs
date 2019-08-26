using EvidenceCapture.Model.Message;
using EvidenceCapture.View.Overray;
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
        public enum MessageType
        {
            Info,
            Error,
            Warn
        }

        protected void MessageDialog(MessageType type, string message)
        {
            var view = new MessageDialog();
            var dc = view.DataContext as MessageDialogViewModel;

            dc.Title = type.ToString();
            dc.Message = message;

            CloseDialog();

            MessengerInstance.Send(new OverrayDialogMessage()
            {
                Operate = OverrayDialogMessage.OperateType.Open,
                NewDialog = view,
                CallBack = null
            });

        }

        protected void LaunchDialog(UserControl newDialog, Action<object> callback)
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

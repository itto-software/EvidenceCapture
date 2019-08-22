
using EvidenceCapture.Model.Message;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Windows.Input;

namespace EvidenceCapture.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Properties
        /// <summary>
        /// 最小化コマンド
        /// </summary>
        public ICommand ToMinimunCommand { get; private set; }

        /// <summary>
        /// 最大化または通常化コマンド
        /// </summary>
        public ICommand ToNormalOrMaximamCommand { get; private set; }

        /// <summary>
        /// アプリケーション終了コマンド
        /// </summary>
        public ICommand ExitApplicationCommand { get; private set; }

        #endregion

        public MainViewModel()
        {
            // コマンドの初期化
            ToMinimunCommand = new RelayCommand(()=> {
                MessengerInstance.Send(new WindowOperate() { Operate = WindowOperate.OperateEnum.ToMinimunCommand });
            });
            ToNormalOrMaximamCommand = new RelayCommand(() => {
                MessengerInstance.Send(new WindowOperate() { Operate = WindowOperate.OperateEnum.ToNormalOrMaximam });

            });
            ExitApplicationCommand = new RelayCommand(() => {
                MessengerInstance.Send(new WindowOperate() { Operate = WindowOperate.OperateEnum.ExitApplicationCommand });
            });
        }

        
    }
}
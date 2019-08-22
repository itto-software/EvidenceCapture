
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
        /// �ŏ����R�}���h
        /// </summary>
        public ICommand ToMinimunCommand { get; private set; }

        /// <summary>
        /// �ő剻�܂��͒ʏ퉻�R�}���h
        /// </summary>
        public ICommand ToNormalOrMaximamCommand { get; private set; }

        /// <summary>
        /// �A�v���P�[�V�����I���R�}���h
        /// </summary>
        public ICommand ExitApplicationCommand { get; private set; }

        #endregion

        public MainViewModel()
        {
            // �R�}���h�̏�����
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
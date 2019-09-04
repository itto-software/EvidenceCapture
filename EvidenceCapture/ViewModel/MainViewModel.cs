
using EvidenceCapture.Model;
using EvidenceCapture.Model.Message;
using EvidenceCapture.ViewModel.Base;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EvidenceCapture.ViewModel
{
    public class MainViewModel : BaseVM, IDisposable
    {
        public enum ContentsType
        {
            OperateControl,
            AppliactionSettingControl,
            WebCamSettingControl
        }


        #region Fields

        private Dictionary<ContentsType, UserControl> mainContentsCache;

        private bool _showOverRay;
        private bool _showWaiting;
        private UserControl _subDialog;
        private bool _showDialog;
        private UserControl _mainContents;

        #endregion

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

        /// <summary>���C���R���e���c�ύX�R�}���h</summary>
        public ICommand ContentsSwitchCommand { get; private set; }

        /// <summary>�A�v���P�[�V�������O�t�H���_�m�F�R�}���h</summary>
        public ICommand LaunchAppLogCommand { get; private set; }

        public UserControl MainContents
        {
            get
            {
                return _mainContents;
            }
            set
            {
                _mainContents = value;
                RaisePropertyChanged(nameof(MainContents));
            }

        }

        /// <summary>���[�f�B���O�̕\���A�\��</summary>
        public bool ShowWaiting
        {
            get
            {
                return _showWaiting;
            }
            set
            {
                _showWaiting = value;
                RaisePropertyChanged(nameof(ShowWaiting));
            }
        }

        /// <summary>�I�[�o�[���C�̕\���A�\��</summary>
        public bool ShowOverRay
        {
            get
            {
                return _showOverRay;
            }
            set
            {
                _showOverRay = value;
                RaisePropertyChanged(nameof(ShowOverRay));
            }
        }


        /// <summary>�T�u�_�C�A���O�̕\���A��\��</summary>
        public bool ShowDialog
        {
            get
            {
                return _showDialog;
            }
            set
            {
                if (value != _showDialog)
                {
                    _showDialog = value;
                    RaisePropertyChanged(nameof(ShowDialog));
                }
            }
        }

        public UserControl SubDialog
        {
            get
            {
                return _subDialog;
            }
            set
            {
                if (value != _subDialog)
                {
                    _subDialog = value;
                    RaisePropertyChanged(nameof(SubDialog));
                }
            }

        }

        #endregion

        public MainViewModel()
        {
            // �R�}���h�̏�����
            ToMinimunCommand = new RelayCommand(() =>
            {
                MessengerInstance.Send(new WindowOperateMessage() { Operate = WindowOperateMessage.OperateEnum.ToMinimunCommand });
            });
            ToNormalOrMaximamCommand = new RelayCommand(() =>
            {
                MessengerInstance.Send(new WindowOperateMessage() { Operate = WindowOperateMessage.OperateEnum.ToNormalOrMaximam });

            });
            ExitApplicationCommand = new RelayCommand(() =>
            {
                MessengerInstance.Send(new WindowOperateMessage() { Operate = WindowOperateMessage.OperateEnum.ExitApplicationCommand });
            });

            ContentsSwitchCommand = new RelayCommand<ContentsType>(ContentsSwitchAction);

            LaunchAppLogCommand = new RelayCommand(LaunchAppLogImpl);

            // �I�[�o�[���C�E�C���h�E���V�[�o�[��o�^
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register(this, (Action<OverrayDialogMessage>)OverrayDialogReceiver);


            ShowOverRay = false;
            ShowWaiting = false;

            // ���C���R���e���c�̃L���b�V����������
            mainContentsCache = new Dictionary<ContentsType, UserControl>();
            mainContentsCache.Add(ContentsType.OperateControl, new View.MainContents.OperateControl());
            mainContentsCache.Add(ContentsType.AppliactionSettingControl, new View.MainContents.AppliactionSettingControl());
            mainContentsCache.Add(ContentsType.WebCamSettingControl, new View.MainContents.WebCamSettingControl());

            ContentsSwitchAction(ContentsType.OperateControl);

        }

        private void LaunchAppLogImpl()
        {
            CommonUtility.Explorer(NLogHelper.AppLogdir);
        }

        private void ContentsSwitchAction(ContentsType param)
        {
            try
            {
                if (MainContents != null && MainContents.DataContext is IMainContents)
                {
                    var mcd = MainContents.DataContext as IMainContents;
                    mcd.DetachContens();
                }
                MainContents = mainContentsCache[param];
                (MainContents.DataContext as IMainContents).AttacheContens();
            }
            catch (Exception e)
            {
                logger.Error(e);
                MessageDialog(e);
            }
        }

        private void OverrayDialogReceiver(OverrayDialogMessage odlg)
        {
            switch (odlg.Operate)
            {
                case OverrayDialogMessage.OperateType.Open:
                    ShowOverRay = true;
                    ShowDialog = true;

                    if (odlg.NewDialog != null)
                        SubDialog = odlg.NewDialog;
                    break;
                case OverrayDialogMessage.OperateType.Close:
                    ShowOverRay = false;
                    ShowWaiting = false;
                    break;

                case OverrayDialogMessage.OperateType.Waiting:
                    ShowWaiting = true;
                    ShowOverRay = true;
                    ShowDialog = false;
                    break;

            }
        }

        public void Dispose()
        {
            if (mainContentsCache != null)
            {
                foreach(var item in  mainContentsCache)
                {
                    if( item.Value.DataContext is IDisposable)
                    {
                        var mc = item.Value.DataContext as IDisposable;
                        mc.Dispose();
                    }
                }

            }
        }
    }
}
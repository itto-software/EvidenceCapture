
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

        /// <summary>メインコンテンツ変更コマンド</summary>
        public ICommand ContentsSwitchCommand { get; private set; }

        /// <summary>アプリケーションログフォルダ確認コマンド</summary>
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

        /// <summary>ローディングの表示、表示</summary>
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

        /// <summary>オーバーレイの表示、表示</summary>
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


        /// <summary>サブダイアログの表示、非表示</summary>
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
            // コマンドの初期化
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

            // オーバーレイウインドウレシーバーを登録
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register(this, (Action<OverrayDialogMessage>)OverrayDialogReceiver);


            ShowOverRay = false;
            ShowWaiting = false;

            // メインコンテンツのキャッシュを初期化
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
using EvidenceCapture.Model.Message;
using EvidenceCapture.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EvidenceCapture.View
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel vm;

        public MainWindow()
        {


            InitializeComponent();

            // ウインドステータス更新メッセージレシーバーを登録
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register(this, (Action<WindowOperateMessage>)WindowOperateReceiver);


            this.WindowState = WindowState.Maximized;
            vm = this.DataContext as MainViewModel;
            this.Closing += OnClosing;

            var ds = Properties.DisplaySettings.Default;

            if (ds.DefaultDisplayIsNormal)
            {
                this.WindowState = WindowState.Normal;
                this.normOrMax.Content = 1;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                this.normOrMax.Content = 2;
            }

            this.Width = ds.DefaultDisplayWidth;
            this.Height = ds.DefaultDisplayHeight;
            this.Left = ds.DefaultDisplayX;
            this.Top = ds.DefaultDisplayY;

        }


        private void WindowOperateReceiver(WindowOperateMessage womsg)
        {
            switch (womsg.Operate)
            {
                case WindowOperateMessage.OperateEnum.ExitApplicationCommand:
                    this.Close();
                    break;
                case WindowOperateMessage.OperateEnum.ToMinimunCommand:
                    this.WindowState = WindowState.Minimized;
                    this.ShowInTaskbar = true;
                    break;
                case WindowOperateMessage.OperateEnum.ToNormal:
                    this.WindowState = WindowState.Normal;
                    break;
                case WindowOperateMessage.OperateEnum.ToMaximam:
                    this.WindowState = WindowState.Maximized;
                    break;
                case WindowOperateMessage.OperateEnum.ToNormalOrMaximam:
                    if (this.WindowState == WindowState.Maximized)
                        this.WindowState = WindowState.Normal;
                    else
                        this.WindowState = WindowState.Maximized;

                    break;
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            Properties.DisplaySettings.Default.DefaultDisplayIsNormal
                = this.WindowState == WindowState.Normal;

            Properties.DisplaySettings.Default.Save();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            // ウインドウステータスに応じてタイトルのアイコンを変更する
            if (this.WindowState == WindowState.Maximized)
            {
                this.normOrMax.Content = 2;
            }
            else
            {
                this.normOrMax.Content = 1;
            }
            base.OnStateChanged(e);
        }


        protected override void OnLocationChanged(EventArgs e)
        {
            Properties.DisplaySettings.Default.DefaultDisplayX = (this.Left > 0) ? this.Left : 0;
            Properties.DisplaySettings.Default.DefaultDisplayY = (this.Top > 0) ? this.Top : 0;
            base.OnLocationChanged(e);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            Properties.DisplaySettings.Default.DefaultDisplayWidth = this.Width;
            Properties.DisplaySettings.Default.DefaultDisplayHeight = this.Height;
            base.OnRenderSizeChanged(sizeInfo);
        }

    }
}

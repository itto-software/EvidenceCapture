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

            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register(this, (Action<WindowOperate>)WindowOperateReceiver);
            this.WindowState = WindowState.Maximized;
            vm = this.DataContext as MainViewModel;
            this.Closing += _OnClosing;

            if (Properties.DisplaySettings.Default.DefaultDisplayIsNormal)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;

        }

        private void WindowOperateReceiver(WindowOperate womsg)
        {
            switch (womsg.Operate)
            {
                case WindowOperate.OperateEnum.ExitApplicationCommand:
                    this.Close();
                    break;
                case WindowOperate.OperateEnum.ToMinimunCommand:
                    this.WindowState = WindowState.Minimized;
                    this.ShowInTaskbar = true;
                    break;
                case WindowOperate.OperateEnum.ToNormal:
                    this.WindowState = WindowState.Normal;
                    break;
                case WindowOperate.OperateEnum.ToMaximam:
                    this.WindowState = WindowState.Maximized;
                    break;

                case WindowOperate.OperateEnum.ToNormalOrMaximam:
                    if (this.WindowState == WindowState.Maximized)
                    {
                        this.WindowState = WindowState.Normal;
                    }
                    else
                    {
                        this.WindowState = WindowState.Maximized;
                    }
                    break;
            }
        }

        private void _OnClosing(object sender, CancelEventArgs e)
        {
            Properties.DisplaySettings.Default.DefaultDisplayIsNormal
                = this.WindowState == WindowState.Normal;

            Properties.DisplaySettings.Default.Save();
        }
    }
}

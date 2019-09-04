using EvidenceCapture.Model;
using EvidenceCapture.View;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EvidenceCapture
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 多重起動を防止する為のミューテックス
        /// </summary>
        private static Mutex _mutex;


        public App()
        {
           NLogHelper.NLogInitilalize();
        }


        /// <summary>アプリ起動時イベント</summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            App._mutex = new Mutex(false, "EvidenceCapture");
            if (!App._mutex.WaitOne(0, false))
            {
                App._mutex.Close();
                App._mutex = null;

                this.Shutdown(-1);
                return;
            }

            var mainWindow = new MainWindow();

            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (App._mutex == null) { return; }
            // ミューテックスの解放
            App._mutex.ReleaseMutex();
            App._mutex.Close();
            App._mutex = null;

            base.OnExit(e);
        }
    }
}

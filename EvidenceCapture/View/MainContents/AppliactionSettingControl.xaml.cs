using EvidenceCapture.ViewModel.MainContents;
using System;
using System.Collections.Generic;
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

namespace EvidenceCapture.View.MainContents
{
    /// <summary>
    /// AppliactionSettingControl.xaml の相互作用ロジック
    /// </summary>
    public partial class AppliactionSettingControl : UserControl
    {
        public AppliactionSettingControl()
        {
            InitializeComponent();
            var dc = this.DataContext as ApplicationSettingViewModel;

            if(dc != null)
            {
                dc.View = this;
            }
        }

    }
}

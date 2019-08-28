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

namespace EvidenceCapture.View.CustomControl
{
    /// <summary>
    /// LabelAndValue.xaml の相互作用ロジック
    /// </summary>
    public partial class LabelAndValue : UserControl
    {
        public string LabelText
        {
            get
            {
                return (this.Label.Content).ToString();
            }
            set
            {
                this.Label.Content = value;
            }
        }

        public object ValueContent
        {
            get
            {
                return this.Value.Content;
            }
            set
            {
                this.Value.Content = value;
            }
        }


        public LabelAndValue()
        {
            InitializeComponent();
        }
    }
}

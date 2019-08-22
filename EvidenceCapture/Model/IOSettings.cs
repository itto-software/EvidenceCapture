using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvidenceCapture.Model
{
    /// <summary>IO先を管理するオブジェクト</summary>
    class IOSettings
    {
        #region Fields

        /// <summary>内部管理用インスタンス</summary>
        private static IOSettings _instance = new IOSettings();

        #endregion

        #region Properties

        /// <summary>インスタンス</summary>
        internal static IOSettings Instance => _instance;


        public string OutputDir
        {
            get
            {
                return Properties.Settings.Default.DefaultOutput;
            }
            set
            {
                Properties.Settings.Default.DefaultOutput = value;
                Properties.Settings.Default.Save();
            }
        }


        #endregion

        /// <summary>
        /// privateなコンストラクタ
        /// </summary>
        private IOSettings() { }
    }
}

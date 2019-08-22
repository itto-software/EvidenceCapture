using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvidenceCapture.Model
{
    /// <summary>IO先を管理するオブジェクト</summary>
    class ApplicationSettings
    {
        #region Fields

        /// <summary>内部管理用インスタンス</summary>
        private static ApplicationSettings _instance = new ApplicationSettings();

        #endregion

        #region Properties

        /// <summary>インスタンス</summary>
        internal static ApplicationSettings Instance => _instance;


        public string GroupPattern
        {
            get
            {
                return Properties.Settings.Default.GroupPattern;
            }
            set
            {
                Properties.Settings.Default.GroupPattern = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>出力先フォルダ</summary>
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

        /// <summary>画像拡張子</summary>
        public string ImageFormat
        {
            get
            {
                return Properties.Settings.Default.ImageFormat;
            }
            set
            {
                Properties.Settings.Default.ImageFormat = value;
                Properties.Settings.Default.Save();
            }
        }


        #endregion

        /// <summary>
        /// privateなコンストラクタ
        /// </summary>
        private ApplicationSettings() { }
    }
}

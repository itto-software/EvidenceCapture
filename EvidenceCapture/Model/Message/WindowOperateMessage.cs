using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvidenceCapture.Model.Message
{
    /// <summary>
    /// Windowステータス変更メッセージ
    /// </summary>
    internal class WindowOperateMessage
    {
        /// <summary>
        /// ウィンドウステータス変更イベント名enum
        /// </summary>
        internal enum OperateEnum
        {
            /// <summary>
            /// 最小化
            /// </summary>
            ToMinimunCommand,

            /// <summary>
            /// 通常または最大化
            /// </summary>
            ToNormalOrMaximam,

            /// <summary>
            /// アプリケーションの終了
            /// </summary>
            ExitApplicationCommand,

            /// <summary>
            /// 通常化
            /// </summary>
            ToNormal,

            /// <summary>
            /// 最大化
            /// </summary>
            ToMaximam,

        }

        /// <summary>
        /// ステータス変更通知内容
        /// </summary>
        internal OperateEnum Operate { get; set; }

    }
}

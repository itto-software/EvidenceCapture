using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace HongliangSoft.Utilities
{
    ///<summary>キーボードが操作されたときに実行されるメソッドを表すイベントハンドラ。</summary>
    public delegate void KeyboardHookedEventHandler(object sender, KeyboardHookedEventArgs e);
    ///<summary>KeyboardHookedイベントのデータを提供する。</summary>
    public class KeyboardHookedEventArgs : EventArgs
    {
        ///<summary>新しいインスタンスを作成する。</summary>
        internal KeyboardHookedEventArgs(KeyboardMessage message, ref KeyboardState state)
        {
            this.message = message;
            this.state = state;
        }
        private KeyboardMessage message;
        private KeyboardState state;
        ///<summary>キーボードが押されたか放されたかを表す値を取得する。</summary>
        public KeyboardUpDown UpDown
        {
            get
            {
                return (message == KeyboardMessage.KeyDown || message == KeyboardMessage.SysKeyDown) ?
                    KeyboardUpDown.Down : KeyboardUpDown.Up;
            }
        }
        ///<summary>操作されたキーの仮想キーコードを表す値を取得する。</summary>
        public Keys KeyCode { get { return state.KeyCode; } }
        ///<summary>操作されたキーのスキャンコードを表す値を取得する。</summary>
        public int ScanCode { get { return state.ScanCode; } }
        ///<summary>操作されたキーがテンキーなどの拡張キーかどうかを表す値を取得する。</summary>
        public bool IsExtendedKey { get { return state.Flag.IsExtended; } }
        ///<summary>ALTキーが押されているかどうかを表す値を取得する。</summary>
        public bool AltDown { get { return state.Flag.AltDown; } }
    }
    ///<summary>キーボードが押されているか放されているかを表す。</summary>
    public enum KeyboardUpDown
    {
        ///<summary>キーは押されている。</summary>
        Down,
        ///<summary>キーは放されている。</summary>
        Up,
    }

    ///<summary>メッセージコードを表す。</summary>
    internal enum KeyboardMessage
    {
        ///<summary>キーが押された。</summary>
        KeyDown = 0x100,
        ///<summary>キーが放された。</summary>
        KeyUp = 0x101,
        ///<summary>システムキーが押された。</summary>
        SysKeyDown = 0x104,
        ///<summary>システムキーが放された。</summary>
        SysKeyUp = 0x105,
    }
    ///<summary>キーボードの状態を表す。</summary>
    internal struct KeyboardState
    {
        ///<summary>仮想キーコード。</summary>
        public Keys KeyCode;
        ///<summary>スキャンコード。</summary>
        public int ScanCode;
        ///<summary>各種特殊フラグ。</summary>
        public KeyboardStateFlag Flag;
        ///<summary>このメッセージが送られたときの時間。</summary>
        public int Time;
        ///<summary>メッセージに関連づけられた拡張情報。</summary>
        public IntPtr ExtraInfo;
    }
    ///<summary>キーボードの状態を補足する。</summary>
    internal struct KeyboardStateFlag
    {
        private int flag;
        private bool IsFlagging(int value)
        {
            return (flag & value) != 0;
        }
        private void Flag(bool value, int digit)
        {
            flag = value ? (flag | digit) : (flag & ~digit);
        }
        ///<summary>キーがテンキー上のキーのような拡張キーかどうかを表す。</summary>
        public bool IsExtended { get { return IsFlagging(0x01); } set { Flag(value, 0x01); } }
        ///<summary>イベントがインジェクトされたかどうかを表す。</summary>
        public bool IsInjected { get { return IsFlagging(0x10); } set { Flag(value, 0x10); } }
        ///<summary>ALTキーが押されているかどうかを表す。</summary>
        public bool AltDown { get { return IsFlagging(0x20); } set { Flag(value, 0x20); } }
        ///<summary>キーが放されたどうかを表す。</summary>
        public bool IsUp { get { return IsFlagging(0x80); } set { Flag(value, 0x80); } }
    }
    ///<summary>キーボードの操作をフックし、任意のメソッドを挿入する。</summary>
    public class KeyboardHook : System.ComponentModel.Component
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int hookType, KeyboardHookDelegate hookDelegate, IntPtr hInstance, uint threadId);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int CallNextHookEx(IntPtr hook, int code, KeyboardMessage message, ref KeyboardState state);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);

        private delegate int KeyboardHookDelegate(int code, KeyboardMessage message, ref KeyboardState state);
        private const int KeyboardHookType = 13;
        private KeyboardHookDelegate hookDelegate;
        private IntPtr hook;
        private static readonly object EventKeyboardHooked = new object();
        ///<summary>キーボードが操作されたときに発生する。</summary>
        public event KeyboardHookedEventHandler KeyboardHooked
        {
            add { base.Events.AddHandler(EventKeyboardHooked, value); }
            remove { base.Events.RemoveHandler(EventKeyboardHooked, value); }
        }
        ///<summary>
        ///KeyboardHookedイベントを発生させる。
        ///</summary>
        ///<param name="e">イベントのデータ。</param>
        protected virtual void OnKeyboardHooked(KeyboardHookedEventArgs e)
        {
            KeyboardHookedEventHandler handler = base.Events[EventKeyboardHooked] as KeyboardHookedEventHandler;
            if (handler != null)
                handler(this, e);
        }
        ///<summary>
        ///新しいインスタンスを作成する。
        ///</summary>
        public KeyboardHook()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                throw new PlatformNotSupportedException("Windows 98/Meではサポートされていません。");
            this.hookDelegate = new KeyboardHookDelegate(CallNextHook);
            IntPtr module = Marshal.GetHINSTANCE(typeof(KeyboardHook).Assembly.GetModules()[0]);
            hook = SetWindowsHookEx(KeyboardHookType, hookDelegate, module, 0);
        }
        ///<summary>
        ///キーボードが操作されたときに実行するデリゲートを指定してインスタンスを作成する。
        ///</summary>
        ///<param name="handler">キーボードが操作されたときに実行するメソッドを表すイベントハンドラ。</param>
        public KeyboardHook(KeyboardHookedEventHandler handler) : this()
        {
            this.KeyboardHooked += handler;
        }
        private int CallNextHook(int code, KeyboardMessage message, ref KeyboardState state)
        {
            if (code >= 0)
                OnKeyboardHooked(new KeyboardHookedEventArgs(message, ref state));
            return CallNextHookEx(hook, code, message, ref state);
        }
        ///<summary>
        ///使用されているアンマネージリソースを解放し、オプションでマネージリソースも解放する。
        ///</summary>
        ///<param name="disposing">マネージリソースも解放する場合はtrue。</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                UnhookWindowsHookEx(hook);
                hook = IntPtr.Zero;
                base.Dispose(disposing);
            }
        }
        ///<summary>ファイナライザ。</summary>
        ~KeyboardHook()
        {
            Dispose(false);
        }
        private bool disposed = false;
    }
}

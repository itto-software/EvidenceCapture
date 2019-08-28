using EvidenceCapture.Model;
using EvidenceCapture.Properties;
using EvidenceCapture.View.MainContents;
using EvidenceCapture.ViewModel.Base;
using GalaSoft.MvvmLight.CommandWpf;
using HongliangSoft.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EvidenceCapture.ViewModel.MainContents
{
    public class ApplicationSettingViewModel : BaseVM, IMainContents
    {
        public enum FocusType
        {
            KeyShortCutScreenCap,
            KeyShortCutApplicationCap,
            KeyShortCutG1,
            KeyShortCutG2,
            KeyShortCutG3,
            None
        }

        public enum ImageFormatType
        {
            png,
            bmp,
            jpeg
        }


        #region Fields
        private Stopwatch sw = new Stopwatch();
        private TimeSpan st = new TimeSpan(0, 0, 0, 0, 500);
        private List<LabalAndValue> _imageFormatComboList;
        private Dictionary<string, bool> _validateState;
        private int _snapShotFormatWidth;
        private int _snapShotFormatHeight;
        private LabalAndValue _snapShotFormatOutFormat;
        private string _keyShortCutScreenCap;
        private string _keyShortCutApplicationCap;
        private string _keyShortCutG1;
        private string _keyShortCutG2;
        private string _keyShortCutG3;
        private bool isKeyBind;

        private KeyboardHook keyHook;
        private KeyboardUpDown _before;
        private FocusType _selcKyCol;

        #endregion

        #region Propeties

        public ICommand ApplyCommand { get; private set; }
        public ICommand GotFocusKeyInputColumnCommand { get; private set; }
        public ICommand LostsFocusKeyInputColumnCommand { get; private set; }

        public List<LabalAndValue> ImageFormatComboList
        {
            get
            {
                if (_imageFormatComboList == null)
                {
                    _imageFormatComboList = new List<LabalAndValue>();
                    _imageFormatComboList.Add(new LabalAndValue() { Label = Resources.ImageFormatPng, Value = ImageFormatType.png });
                    _imageFormatComboList.Add(new LabalAndValue() { Label = Resources.ImageFormatBmp, Value = ImageFormatType.bmp });
                    _imageFormatComboList.Add(new LabalAndValue() { Label = Resources.ImageFormatJpg, Value = ImageFormatType.jpeg });

                }
                return _imageFormatComboList;
            }
        }

        public string SnapShotFormatWidth
        {
            get
            {
                return _snapShotFormatWidth.ToString();
            }
            set
            {
                var key = nameof(SnapShotFormatWidth);
                if (!_validateState.ContainsKey(key))
                    _validateState.Add(key, false);


                _validateState[key] = int.TryParse(value, out _snapShotFormatWidth);

                if (_validateState[key])
                    RaisePropertyChanged(nameof(SnapShotFormatWidth));
                else
                    throw new ArgumentException();
            }
        }

        public string SnapShotFormatHeight
        {
            get
            {
                return _snapShotFormatHeight.ToString();
            }
            set
            {
                var key = nameof(SnapShotFormatHeight);
                if (!_validateState.ContainsKey(key))
                    _validateState.Add(key, false);


                _validateState[key] = int.TryParse(value, out _snapShotFormatHeight);

                if (_validateState[key])
                    RaisePropertyChanged(nameof(SnapShotFormatHeight));
                else
                    throw new ArgumentException();

            }
        }


        public LabalAndValue SnapShotFormatOutFormat
        {
            get
            {
                return _snapShotFormatOutFormat;
            }
            set
            {
                _snapShotFormatOutFormat = value;
                RaisePropertyChanged(nameof(SnapShotFormatOutFormat));
            }
        }

        public string KeyShortCutScreenCap
        {
            get
            {
                return _keyShortCutScreenCap;
            }
            set
            {
                var key = nameof(KeyShortCutScreenCap);
                if (!_validateState.ContainsKey(key))
                    _validateState.Add(key, false);

                _validateState[key] = HasSameValue(value, nameof(KeyShortCutScreenCap));

                _keyShortCutScreenCap = value;
                RaisePropertyChanged(nameof(KeyShortCutScreenCap));

            }
        }


        public string KeyShortCutApplicationCap
        {
            get
            {
                return _keyShortCutApplicationCap;
            }
            set
            {
                var key = nameof(KeyShortCutApplicationCap);
                if (!_validateState.ContainsKey(key))
                    _validateState.Add(key, false);

                _validateState[key] = HasSameValue(value, nameof(KeyShortCutApplicationCap));

                _keyShortCutApplicationCap = value;
                RaisePropertyChanged(nameof(KeyShortCutApplicationCap));
            }
        }

        public string KeyShortCutG1
        {
            get
            {
                return _keyShortCutG1;
            }
            set
            {
                var key = nameof(KeyShortCutG1);
                if (!_validateState.ContainsKey(key))
                    _validateState.Add(key, false);

                _validateState[key] = HasSameValue(value, key);

                _keyShortCutG1 = value;
                RaisePropertyChanged(key);

            }
        }

        public string KeyShortCutG2
        {
            get
            {
                return _keyShortCutG2;
            }
            set
            {
                var key = nameof(KeyShortCutG2);
                if (!_validateState.ContainsKey(key))
                    _validateState.Add(key, false);

                _validateState[key] = HasSameValue(value, key);

                _keyShortCutG2 = value;
                RaisePropertyChanged(key);

            }
        }
        public string KeyShortCutG3
        {
            get
            {
                return _keyShortCutG3;
            }
            set
            {
                var key = nameof(KeyShortCutG3);
                if (!_validateState.ContainsKey(key))
                    _validateState.Add(key, false);

                _validateState[key] = HasSameValue(value, key);

                _keyShortCutG3 = value;
                RaisePropertyChanged(key);

            }
        }

        public AppliactionSettingControl View { get; internal set; }

        #endregion


        private bool HasSameValue(string value, string propname)
        {
            if (nameof(KeyShortCutApplicationCap) != propname && KeyShortCutApplicationCap == value)
                return false;
            if (nameof(KeyShortCutScreenCap) != propname && KeyShortCutScreenCap == value)
                return false;
            if (nameof(KeyShortCutG1) != propname && KeyShortCutG1 == value)
                return false;
            if (nameof(KeyShortCutG2) != propname && KeyShortCutG2 == value)
                return false;
            if (nameof(KeyShortCutG3) != propname && KeyShortCutG3 == value)
                return false;


            return true;

        }

        public ApplicationSettingViewModel()
        {
            _validateState = new Dictionary<string, bool>();
            SnapShotFormatHeight = "0";
            SnapShotFormatWidth = "0";

            ApplyCommand = new RelayCommand(ApplyAction, CanApply);

            GotFocusKeyInputColumnCommand = new RelayCommand<FocusType>((selectType) => _selcKyCol = selectType);
            LostsFocusKeyInputColumnCommand = new RelayCommand(() => _selcKyCol = FocusType.None);

            keyHook = new KeyboardHook();
            keyHook.KeyboardHooked += new KeyboardHookedEventHandler(OnGlobalKeyAction);

            _selcKyCol = FocusType.None;
        }


        private void ApplyAction()
        {
            var ai = ApplicationSettings.Instance;
            ai.DefaultHeight = int.Parse(SnapShotFormatHeight);
            ai.DefaultWidth = int.Parse(SnapShotFormatWidth);
            ai.ImageFormat = SnapShotFormatOutFormat.Value.ToString();
            ai.KeyShortCutApplicationCap = KeyShortCutApplicationCap;
            ai.KeyShortCutScreenCap = KeyShortCutScreenCap;
            ai.KeyShortCutG1 = KeyShortCutG1;
            ai.KeyShortCutG2 = KeyShortCutG2;
            ai.KeyShortCutG3 = KeyShortCutG3;
            ai.KeyShortCutG3 = KeyShortCutG3;


        }

        private bool CanApply()
        {
            return _validateState.Count(x => x.Value == false) == 0;
        }



        private void OnGlobalKeyAction(object sender, KeyboardHookedEventArgs e)
        {
            if (!isKeyBind)
                return;

            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.LControlKey:
                case System.Windows.Forms.Keys.RControlKey:
                case System.Windows.Forms.Keys.LShiftKey:
                case System.Windows.Forms.Keys.RShiftKey:
                    return;

            }

            if (e.UpDown == KeyboardUpDown.Up == (!sw.IsRunning || sw.Elapsed > st))
            {
                List<string> keyArray = new List<string>();
                if (e.AltDown) keyArray.Add("Alt");
                keyArray.Add(e.KeyCode.ToString());

                var keyStr = String.Join(" + ", keyArray.ToArray());

                switch (_selcKyCol)
                {
                    case FocusType.KeyShortCutApplicationCap:
                        KeyShortCutApplicationCap = keyStr;
                        break;
                    case FocusType.KeyShortCutScreenCap:
                        KeyShortCutScreenCap = keyStr;
                        break;
                    case FocusType.KeyShortCutG1:
                        KeyShortCutG1 = keyStr;
                        break;
                    case FocusType.KeyShortCutG2:
                        KeyShortCutG2 = keyStr;
                        break;
                    case FocusType.KeyShortCutG3:
                        KeyShortCutG3 = keyStr;
                        break;

                }
                sw.Restart();

            }
        }

        /// <summary>自コンテンツに遷移した場合</summary>
        public void AttacheContens()
        {
            var ai = ApplicationSettings.Instance;
            _validateState = new Dictionary<string, bool>();

            SnapShotFormatHeight = ai.DefaultHeight.ToString();
            SnapShotFormatWidth = ai.DefaultWidth.ToString();
            KeyShortCutScreenCap = ai.KeyShortCutScreenCap;
            KeyShortCutApplicationCap = ai.KeyShortCutApplicationCap;
            KeyShortCutG1 = ai.KeyShortCutG1;
            KeyShortCutG2 = ai.KeyShortCutG2;
            KeyShortCutG3 = ai.KeyShortCutG3;

            var imageFormat = CommonUtility.GetEnum<ImageFormatType>(ApplicationSettings.Instance.ImageFormat);

            var result = ImageFormatComboList.Find(x => x.Value == imageFormat);

            if (result != null)
            {
                SnapShotFormatOutFormat = result;
            }

            isKeyBind = true;
        }

        /// <summary>別コンテンツに遷移する場合</summary>
        public void DetachContens()
        {
            isKeyBind = false;
        }
    }
}

using EvidenceCapture.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using HongliangSoft.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EvidenceCapture.ViewModel
{
    class OperateControlViewModel : ViewModelBase
    {
        #region Fields

        private KeyboardHook keyHook;
        private OperateControlModel model;
        private string _selectedOutputPath;

        #endregion

        #region Properties

        public ICommand SetOutputPathCommand { get; private set; }

        public string SelectedOutputPath
        {
            get
            {
                return _selectedOutputPath;
            }
            set
            {
                if (value != _selectedOutputPath)
                {
                    _selectedOutputPath = value;
                    RaisePropertyChanged(nameof(SelectedOutputPath));
                }
            }

        }

        #endregion

        public OperateControlViewModel()
        {
            keyHook = new KeyboardHook();
            keyHook.KeyboardHooked += new KeyboardHookedEventHandler(OnKeyAction);
            model = new OperateControlModel();

            SetOutputPathCommand = new RelayCommand(SetOutputPathImpl);
            SelectedOutputPath = model.OutputRoot;
        }


        private void SetOutputPathImpl()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = model.OutputRoot;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                model.OutputRoot = dialog.SelectedPath;
                SelectedOutputPath = model.OutputRoot;

            }
        }

        private void OnKeyAction(object sender, KeyboardHookedEventArgs e)
        {
            var kc = e.KeyCode;

            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.PrintScreen:
                    if (e.AltDown)
                    {
                        model.AddCapture(false);
                    }
                    else
                    {
                        model.AddCapture();
                    }
                    break;
            }
        }
    }
}

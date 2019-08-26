using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EvidenceCapture.View.Custom
{
    public class BindableCommandCombobox : ComboBox, ICommandSource
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(BindableCommandCombobox),
             new PropertyMetadata((ICommand)null,
            new PropertyChangedCallback(BindableCommandCombobox.CommandChanged)));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }


        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter",
            typeof(object),
            typeof(BindableCommandCombobox),
            new PropertyMetadata(null));


        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public IInputElement CommandTarget
        {
            get { return this; }
        }
        public BindableCommandCombobox() : base()
        {
        }
        private static void CommandChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            //do nothing
        }
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {//オーバーライド！
            base.OnSelectionChanged(e);
            if (this.Command != null)
            {
                RoutedCommand command = this.Command as RoutedCommand;
                if (command != null)
                {
                    command.Execute(this.CommandParameter, this.CommandTarget);
                }
                else
                {
                    ((ICommand)this.Command).Execute(this.CommandParameter);
                }
            }
        }
    }

}

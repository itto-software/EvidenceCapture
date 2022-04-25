using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace EvidenceCapture.View.CustomControl
{
    public class FileDraggableTextBox : TextBox
    {
        // 依存関係プロパティ
        public static readonly DependencyProperty ExtensionsProperty =
            DependencyProperty.Register(
                "Extensions",
                typeof(string),
                typeof(ListBox));

        // CLR ラッパープロパティ
        public string Extensions
        {
            get => (string)this.GetValue(ExtensionsProperty);
            set => this.SetValue(ExtensionsProperty, value);
        }

        // 依存関係プロパティ
        public static readonly DependencyProperty IsFileProperty =
            DependencyProperty.Register(
                "IsFile",
                typeof(bool),
                typeof(ListBox),
                new FrameworkPropertyMetadata(true));

        // CLR ラッパープロパティ
        public bool IsFile
        {
            get => (bool)this.GetValue(IsFileProperty);
            set => this.SetValue(IsFileProperty, value);
        }

        static FileDraggableTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FileDraggableTextBox), new FrameworkPropertyMetadata(typeof(FileDraggableTextBox)));
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            // マウスポインタを変更する。
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = false;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            var fileorfolders = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (fileorfolders.Count() > 0)
                SetText(fileorfolders[0]);
        }

        private void SetText(string fileorfolder)
        {
            if (!IsFile)
            {
                if (Directory.Exists(fileorfolder)) this.Text = fileorfolder;
            }
            else
            {
                var extensions = Extensions.Split(',');
                var extension = System.IO.Path.GetExtension(fileorfolder);
                extension = extension.Substring(1);


                if ((extensions.Count() > 0 && extensions.Contains(extension)) || extensions.Count() == 0)
                    this.Text = fileorfolder;
            }

        }
    }
}

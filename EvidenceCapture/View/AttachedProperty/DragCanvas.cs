using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace EvidenceCapture.View.AttachedProperty
{
    public class DragCanvas
    {
        private static string previewName = "PreviewPicture";
        private static string selectingName = "SelectingRect";
        private static string selectingNameB = "SelectingRectBack";


        #region Preview property


        public static System.Windows.Media.ImageSource GetPreview(DependencyObject canvas)
        {
            return (System.Windows.Media.ImageSource)canvas.GetValue(PreviewProperty);
        }

        public static void SetPreview(DependencyObject canvas, System.Windows.Media.ImageSource value)
        {
            canvas.SetValue(PreviewProperty, value);
        }

        public static readonly DependencyProperty PreviewProperty =
            DependencyProperty.RegisterAttached("Preview", typeof(System.Windows.Media.ImageSource), typeof(DragCanvas), new PropertyMetadata(OnPreviewChanged));

        public static readonly DependencyProperty StateManageProperty =
            DependencyProperty.RegisterAttached("StateManage", typeof(StateManage), typeof(DragCanvas), new PropertyMetadata(null));


        private static void OnPreviewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Canvas)
            {
                var stateManager = (d.GetValue(StateManageProperty)) as StateManage;
                var canvas = d as Canvas;
                Image image = null;
                foreach (var uie in canvas.Children)
                {
                    if (uie is Image && (uie as Image).Name == previewName)
                    {
                        image = uie as Image;
                        image.Name = previewName;
                        break;
                    }
                }

                if (image == null)
                {
                    image = new Image();
                    image.Name = previewName;
                    image.HorizontalAlignment = HorizontalAlignment.Left;
                    image.VerticalAlignment = VerticalAlignment.Top;
                    canvas.Children.Add(image);
                }

                image.Source = e.NewValue as System.Windows.Media.ImageSource;
                image.Width = canvas.ActualWidth;
                image.Height = canvas.ActualHeight;

                stateManager.ImageSource = image.Source;
                stateManager.SetScale(image);

                stateManager.SetPoint(new Point());
                stateManager.SetSize(new Size());

                List<UIElement> target = new List<UIElement>();
                foreach (var uie in canvas.Children)
                {
                    if (uie is Rectangle)
                        target.Add((UIElement)uie);
                }
                target.ForEach(i => canvas.Children.Remove(i));


                SetDragedPoint(d, new Point(0, 0));
                SetDragedSize(d, new Size(0, 0));

            }
        }


        #endregion

        public static Size GetDragedSize(DependencyObject canvas)
        {
            return (Size)canvas.GetValue(DragedSizeProperty);
        }

        public static void SetDragedSize(DependencyObject canvas, Size value)
        {
            canvas.SetValue(DragedSizeProperty, value);
        }

        public static readonly DependencyProperty DragedSizeProperty =
            DependencyProperty.RegisterAttached("DragedSize", typeof(Size), typeof(DragCanvas), new PropertyMetadata(new Size()));


        public static Point GetDragedPoint(DependencyObject canvas)
        {
            return (Point)canvas.GetValue(DragedPointProperty);
        }

        public static void SetDragedPoint(DependencyObject canvas, Point value)
        {
            canvas.SetValue(DragedPointProperty, value);
        }

        public static readonly DependencyProperty DragedPointProperty =
            DependencyProperty.RegisterAttached("DragedPoint", typeof(Point), typeof(DragCanvas), new PropertyMetadata(new Point()));



        public static bool GetIsDragble(DependencyObject canvas)
        {
            return (bool)canvas.GetValue(IsDragbleProperty);
        }

        public static void SetIsDragble(DependencyObject canvas, bool value)
        {
            canvas.SetValue(IsDragbleProperty, value);
        }

        public static readonly DependencyProperty IsDragbleProperty =
         DependencyProperty.RegisterAttached(
             "IsDragble", typeof(bool), typeof(DragCanvas),
             new UIPropertyMetadata(false, OnIsDragbleChanged));

        private static void OnIsDragbleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var targetObj = d as Canvas;

            if (targetObj == null)
                return;

            if (!(e.NewValue is bool))
            {
                return;
            }

            var isDrag = (bool)e.NewValue;

            if (isDrag)
            {
                targetObj.MouseDown += OnMouseDown;
                targetObj.MouseMove += OnMouseMove;
                targetObj.MouseUp += OnMouseUp;
                targetObj.MouseLeave += OnMouseLeave;
                targetObj.SizeChanged += OnSizeChanged;
                targetObj.SetValue(StateManageProperty, new StateManage());
            }
            else
            {
                targetObj.MouseDown -= OnMouseDown;
                targetObj.MouseMove -= OnMouseMove;
                targetObj.MouseUp -= OnMouseUp;
                targetObj.MouseLeave -= OnMouseLeave;
                targetObj.SizeChanged -= OnSizeChanged;
            }

        }


        private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {

            var canvas = sender as Canvas;
            var stateManager = ((sender as DependencyObject).GetValue(StateManageProperty)) as StateManage;

            Image image = null;
            Rectangle selRect = null;
            Rectangle selRectB = null;


            foreach (var uie in canvas.Children)
            {
                if (uie is Image && (uie as Image).Name == previewName)
                    image = uie as Image;
                else if (uie is Rectangle && (uie as Rectangle).Name == selectingName)
                    selRect = uie as Rectangle;
                else if (uie is Rectangle && (uie as Rectangle).Name == selectingNameB)
                    selRectB = uie as Rectangle;

            }

            if (image != null)
            {
                image.Width = e.NewSize.Width;
                image.Height = e.NewSize.Height;
                stateManager.SetScale(image);

                if (selRect != null && selRectB != null)
                {
                    var point = stateManager.GetViewPoint();
                    var size = stateManager.GetViewSize();

                    selRect.Margin = new Thickness(point.X, point.Y, 0, 0);
                    selRect.Width = size.Width;
                    selRect.Height = size.Height;

                    selRectB.Margin = new Thickness(point.X, point.Y, 0, 0);
                    selRectB.Width = size.Width;
                    selRectB.Height = size.Height;

                }
            }

        }

        private static void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SetRect(sender as DependencyObject, e);
            }
        }

        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SetRect(sender as DependencyObject, e);
            }
        }

        private static void SetRect(DependencyObject dobj, MouseEventArgs e)
        {
            var stateManager = (dobj.GetValue(StateManageProperty)) as StateManage;

            var start = stateManager.GetFirstViewPoint();
            var end = e.GetPosition((IInputElement)dobj);


            var bx = (start.X < end.X) ? start.X : end.X;
            var by = (start.Y < end.Y) ? start.Y : end.Y;
            var ex = (start.X > end.X) ? start.X : end.X;
            var ey = (start.Y > end.Y) ? start.Y : end.Y;

            if (ex > stateManager.MaxWidth)
                ex = stateManager.MaxWidth;
            if (ey > stateManager.MaxHeight)
                ey = stateManager.MaxHeight;


            start = new Point(bx, by);
            end = new Point(ex, ey);

            stateManager.SetPoint(start);
            SetDragedPoint(dobj, stateManager.RealPoint);


            var width = (end.X > start.X) ? end.X - start.X : 0;
            var height = (end.Y > start.Y) ? end.Y - start.Y : 0;
            

            var size = new Size(width, height);

            stateManager.SetSize(size);
            SetDragedSize(dobj, stateManager.RealSize);


            var canvas = dobj as Canvas;

            Rectangle rect = null;
            Rectangle rectB = null;

            foreach (var uie in canvas.Children)
            {
                if (uie is Rectangle && (uie as Rectangle).Name == selectingName)
                    rect = uie as Rectangle;
                else if (uie is Rectangle && (uie as Rectangle).Name == selectingNameB)
                    rectB = uie as Rectangle;
            }
            if (rect == null)
            {
                ResourceDictionary dic = new ResourceDictionary();
                dic.Source = new Uri("/View/Style/CommonStyle.xaml", UriKind.RelativeOrAbsolute);
                var style = dic["SelectingArea"] as Style;
                var styleB = dic["SelectingAreaBack"] as Style;


                rect = new Rectangle();
                rect.Name = selectingName;
                rect.Width = canvas.Width;
                rect.Height = canvas.Height;
                rect.Style = style;
                canvas.Children.Add(rect);


                rectB = new Rectangle();
                rectB.Name = selectingNameB;
                rectB.Width = canvas.Width;
                rectB.Height = canvas.Height;
                rectB.Style = styleB;
                canvas.Children.Add(rectB);

            }
            rect.Margin = new Thickness(start.X, start.Y, 0, 0);
            rect.Width = width;
            rect.Height = height;

            rectB.Margin = new Thickness(start.X, start.Y, 0, 0);
            rectB.Width = width;
            rectB.Height = height;


        }

        private static void OnMouseUp(object sender, MouseEventArgs e)
        {
            var stateManager = ((sender as DependencyObject).GetValue(StateManageProperty)) as StateManage;

            if (e.LeftButton == MouseButtonState.Released)
            {
                ReleaseRect(sender as Canvas);
            }
        }

        private static void ReleaseRect(Canvas canvas)
        {
            // todo 
        }
        

        private static void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var stateManager = ((sender as DependencyObject).GetValue(StateManageProperty)) as StateManage;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition((IInputElement)sender);
                stateManager.SetPoint(point,true);

                SetDragedPoint((DependencyObject)sender, stateManager.RealPoint);

            }

        }


    }
}

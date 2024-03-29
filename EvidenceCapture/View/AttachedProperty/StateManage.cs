﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EvidenceCapture.View.AttachedProperty
{
    internal class StateManage
    {
        public double MaxWidth
        {
            get
            {
                if (ImageSource == null)
                    throw new ArgumentNullException(nameof(ImageSource));

                return (double)(ImageSource as System.Windows.Media.Imaging.BitmapSource).PixelWidth * Scale;
            }
        }

        public double MaxHeight
        {
            get
            {
                if (ImageSource == null)
                    throw new ArgumentNullException(nameof(ImageSource));

                return (double)(ImageSource as System.Windows.Media.Imaging.BitmapSource).PixelHeight * Scale;
            }
        }

        public ImageSource ImageSource { get; internal set; }
        public double Scale { get; private set; }
        public Point FirstRealPoint { get; private set; }
        public Point RealPoint { get; private set; }
        public Size RealSize { get; private set; }

        internal void SetScale(Image image)
        {
            if (ImageSource != null)
            {
                var xScale = image.Width / (ImageSource as System.Windows.Media.Imaging.BitmapSource).PixelWidth;
                var yScale = image.Height / (ImageSource as System.Windows.Media.Imaging.BitmapSource).PixelHeight;
                Scale = (xScale > yScale) ? yScale : xScale;
            }
        }

        /// <summary>拡大率を考慮した座標を設定</summary>
        /// <param name="point"></param>
        internal void SetPoint(Point point, bool isFirst = false)
        {
            RealPoint = new Point(
                point.X / Scale,
                point.Y / Scale);

            if (isFirst)
                FirstRealPoint = new Point(
                point.X / Scale,
                point.Y / Scale);
        }

        internal Point GetFirstViewPoint()
        {
            return new Point(
                FirstRealPoint.X * Scale,
                FirstRealPoint.Y * Scale);
        }

        internal Point GetViewPoint()
        {
            return new Point(
                RealPoint.X * Scale,
                RealPoint.Y * Scale);
        }

        internal void SetSize(Size size)
        {
            RealSize = new Size(
                Math.Round(size.Width / Scale),
                Math.Round(size.Height / Scale));
        }

        internal Size GetViewSize()
        {
            return new Size(
                RealSize.Width * Scale,
                RealSize.Height * Scale);
        }
    }
}
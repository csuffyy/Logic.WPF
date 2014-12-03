﻿using Logic.Util;
using Logic.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Logic.Controls
{
    public class NativeCanvas : Canvas
    {
        public CanvasViewModel Model { get; private set; }

        public NativeCanvas()
        {
            InitializeLayer();
        }

        private void InitializeLayer()
        {
            Model = new CanvasViewModel()
            {
                IsMouseCaptured = () => 
                {
                    return this.IsMouseCaptured;
                },
                CaptureMouse = () =>
                {
                    this.CaptureMouse();
                },
                ReleaseMouseCapture = () => 
                {
                    this.ReleaseMouseCapture();
                },
                InvalidateVisual = () => 
                {
                    this.InvalidateVisual();
                }
            };

            PreviewMouseLeftButtonDown += (s, e) =>
            {
                Model.MouseLeftButtonDown(e.GetPosition(this).ToPoint1());
            };

            PreviewMouseLeftButtonUp += (s, e) =>
            {
                Model.MouseLeftButtonUp(e.GetPosition(this).ToPoint1());
            };

            PreviewMouseMove += (s, e) =>
            {
                Model.MouseMove(e.GetPosition(this).ToPoint1());
            };

            PreviewMouseRightButtonDown += (s, e) =>
            {
                Model.MouseRightButtonDown(e.GetPosition(this).ToPoint1());
            };

            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            Model.OnRender(dc);
        }
    }
}

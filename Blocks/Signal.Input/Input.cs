using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Signal.Input
{
    public class Input : XBlock
    {
        public Input()
        {
            base.Shapes = new List<IShape>();
            base.Pins = new List<XPin>();

            base.Name = "INPUT";

            base.Shapes.Add(
                new XText()
                {
                    X = 0,
                    Y = 0,
                    Width = 30,
                    Height = 30,
                    HAlignment = HAlignment.Center,
                    VAlignment = VAlignment.Center,
                    FontName = "Consolas",
                    FontSize = 14,
                    Text = "IN"
                });
            base.Shapes.Add(new XRectangle() { X = 0, Y = 0, Width = 30, Height = 30, IsFilled = false });
            base.Pins.Add(new XPin() { Name = "O", X = 30, Y = 15, PinType = PinType.Output, Owner = null });
        }
    }
}
// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Blocks
{
    public class Output : XBlock
    {
        public Output()
        {
            base.Database = new List<KeyValuePair<string, IProperty>>();
            base.Shapes = new List<IShape>();
            base.Pins = new List<XPin>();

            base.Name = "OUTPUT";

            IProperty labelProperty = new XProperty("OUT");
            base.Database.Add(new KeyValuePair<string, IProperty>("Label", labelProperty));

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
                    Text = "{0}",
                    Properties = new[] { labelProperty }
                });
            base.Shapes.Add(new XRectangle() { X = 0, Y = 0, Width = 30, Height = 30, IsFilled = false });
            base.Pins.Add(new XPin() { Name = "I", X = 0, Y = 15, PinType = PinType.Input, Owner = null });
        }
    }
}

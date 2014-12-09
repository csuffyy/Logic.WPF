﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public interface IStyle
    {
        string Name { get; set; }
        IColor Fill { get; set; }
        IColor Stroke { get; set; }
        double Thickness { get; set; }
    }
}

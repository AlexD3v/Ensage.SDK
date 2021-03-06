// <copyright file="ID2DBrushContainer.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Ensage.SDK.Renderer.D2D
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    using SharpDX.Direct2D1;

    public interface ID2DBrushContainer : IDictionary<string, SolidColorBrush>, IDisposable
    {
        SolidColorBrush this[Color color] { get; }

        SolidColorBrush Create(string name, Color color);

        SolidColorBrush Create(Color color);
    }
}
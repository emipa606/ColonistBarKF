﻿using UnityEngine;

namespace ColonistBarKF.PSI;

public struct IconEntryPSI
{
    public Icon Icon;

    public Color Color;

    public float Opacity;

    public int Priority;

    public IconEntryPSI(Icon icon, Color color, float opacity, int priority = 10)
    {
        Icon = icon;
        Color = color;
        Opacity = opacity;
        Priority = priority;
    }
}
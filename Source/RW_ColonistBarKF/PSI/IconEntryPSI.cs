using UnityEngine;

namespace ColonistBarKF.PSI;

public struct IconEntryPSI(Icon icon, Color color, float opacity, int priority = 10)
{
    public readonly Icon Icon = icon;

    public Color Color = color;

    public readonly float Opacity = opacity;

    public int Priority = priority;
}
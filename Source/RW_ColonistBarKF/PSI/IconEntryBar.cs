using JetBrains.Annotations;
using UnityEngine;

namespace ColonistBarKF.PSI;

public struct IconEntryBar
{
    public Icon Icon;

    public Color Color;

    public int Priority;

    [CanBeNull] public string Tooltip;

    public IconEntryBar(Icon icon, Color color, [CanBeNull] string tooltip, int priority = 10)
    {
        Icon = icon;
        Color = color;
        Tooltip = tooltip;
        Priority = priority;
    }
}
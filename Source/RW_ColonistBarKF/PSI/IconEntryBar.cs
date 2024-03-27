using JetBrains.Annotations;
using UnityEngine;

namespace ColonistBarKF.PSI;

public struct IconEntryBar(Icon icon, Color color, [CanBeNull] string tooltip, int priority = 10)
{
    public readonly Icon Icon = icon;

    public Color Color = color;

    public int Priority = priority;

    [CanBeNull] public readonly string Tooltip = tooltip;
}
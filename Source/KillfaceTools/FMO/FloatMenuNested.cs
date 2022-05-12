using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace KillfaceTools.FMO;

public class FloatMenuNested : FloatMenu
{
    public FloatMenuNested([NotNull] List<FloatMenuOption> options, [CanBeNull] string label)
        : base(options, label)
    {
        givesColonistOrders = true;
        vanishIfMouseDistant = true;
        closeOnClickedOutside = true;
    }

    public override void DoWindowContents(Rect rect)
    {
        options.ForEach(
            o =>

                // FloatMenuOptionSorting option = o as FloatMenuOptionSorting;
                // option.Label = PathInfo.GetJobReport(option.sortBy);
                o.SetSizeMode(FloatMenuSizeMode.Normal));
        windowRect = new Rect(windowRect.x, windowRect.y, InitialSize.x, InitialSize.y);
        base.DoWindowContents(windowRect);
    }

    public override void PostClose()
    {
        base.PostClose();

        Tools.CloseLabelMenu(false);
    }
}
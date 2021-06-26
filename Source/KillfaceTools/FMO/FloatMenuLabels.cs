using System.Collections.Generic;
using Verse;

namespace KillfaceTools.FMO
{
    public class FloatMenuLabels : FloatMenu
    {
        public FloatMenuLabels(List<FloatMenuOption> options)
            : base(options, null)
        {
            givesColonistOrders = false;
            vanishIfMouseDistant = true;
            closeOnClickedOutside = false;
        }
    }
}
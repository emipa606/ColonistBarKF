using System;
using UnityEngine;
using Verse;

namespace KillfaceTools.FMO
{
    public class FloatMenuOptionNoClose : FloatMenuOption
    {
        public FloatMenuOptionNoClose(string label, Action action, float extraPartWidth,
            Func<Rect, bool> extraPartOnGUI = null)
            : base(label, action, extraPartWidth: extraPartWidth, extraPartOnGUI: extraPartOnGUI)
        {
        }

        public override bool DoGUI(Rect rect, bool colonistOrdering, FloatMenu floatMenu)
        {
            base.DoGUI(rect, colonistOrdering, floatMenu);
            return false; // don't close after an item is selected
        }
    }
}
using System;
using ColonistBarKF.Bar;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace ColonistBarKF
{
    public struct EntryKF
    {
        [CanBeNull]
        public Pawn pawn;

        [CanBeNull]
        public Map map;

        public int @group;

        public Action<int, int> reorderAction;

        public Action<int, Vector2> extraDraggedItemOnGUI;


        public EntryKF([CanBeNull] Pawn pawn, [CanBeNull] Map map, int group, int groupCount)
        {
            this.pawn = pawn;
            this.map = map;
            this.group = group;
            reorderAction = delegate (int from, int to)
            {
                Settings.BarSettings.SortBy =
                    SettingsColonistBar.SortByWhat.vanilla;
                ColonistBar_KF.BarHelperKF.Reorder(from, to, group);
            };
            extraDraggedItemOnGUI = delegate (int index, Vector2 dragStartPos)
            {
                ColonistBar_KF.BarHelperKF.DrawColonistMouseAttachment(index, dragStartPos, group);
            };
        }
    }
}
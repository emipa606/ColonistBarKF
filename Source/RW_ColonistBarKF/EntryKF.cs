﻿using System;
using ColonistBarKF.Bar;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace ColonistBarKF;

public struct EntryKF
{
    [CanBeNull] public readonly Pawn pawn;

    [CanBeNull] public readonly Map map;

    public readonly int group;

    public readonly Action<int, int> reorderAction;

    public readonly Action<int, Vector2> extraDraggedItemOnGUI;


    public EntryKF([CanBeNull] Pawn pawn, [CanBeNull] Map map, int group)
    {
        this.pawn = pawn;
        this.map = map;
        this.group = group;
        reorderAction = delegate(int from, int to)
        {
            Settings.BarSettings.SortBy =
                SettingsColonistBar.SortByWhat.vanilla;
            ColonistBar_KF.BarHelperKF.Reorder(from, to, group);
        };
        extraDraggedItemOnGUI = delegate(int index, Vector2 dragStartPos)
        {
            ColonistBar_KF.BarHelperKF.DrawColonistMouseAttachment(index, dragStartPos, group);
        };
    }
}
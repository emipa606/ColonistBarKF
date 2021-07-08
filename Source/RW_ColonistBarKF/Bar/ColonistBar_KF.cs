using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace ColonistBarKF.Bar
{
    [StaticConstructorOnStartup]
    public static class ColonistBar_KF
    {
        public const float SpacingLabel = 15f;

        public static ColBarHelper_KF BarHelperKF = new ColBarHelper_KF();
        private static readonly List<Pawn> colonistsToHighlight = new List<Pawn>();

        [NotNull] public static ColonistBarColonistDrawer_KF Drawer = new ColonistBarColonistDrawer_KF();

        [NotNull] public static ColonistBarDrawLocsFinder_Kf DrawLocsFinder = new ColonistBarDrawLocsFinder_Kf();

        public static Vector2 BaseSize => new Vector2(
            Settings.BarSettings.BaseIconSize,
            Settings.BarSettings.BaseIconSize);

        public static Vector2 FullSize => new Vector2(
            Settings.BarSettings.BaseIconSize + WidthMoodBarHorizontal
                                              + WidthPSIHorizontal,
            Settings.BarSettings.BaseIconSize + HeightMoodBarVertical
                                              + HeightPSIVertical) * Scale;

        public static float HeightPSIVertical { get; private set; }

        public static float HeightSpacingVertical => Settings.BarSettings.BaseSpacingVertical + HeightMoodBarVertical
            + HeightPSIVertical;

        public static Vector2 PawnSize => new Vector2(
            Settings.BarSettings.BaseIconSize,
            Settings.BarSettings.BaseIconSize) * Scale;

        public static int PSIRowsOnBar => 2;

        // public static readonly Vector2 PawnTextureSize = new Vector2(BaseSize.x - 2f, 75f);
        public static float Scale => BarHelperKF.CachedScale;

        public static bool Visible => UI.screenWidth >= 800 && UI.screenHeight >= 500;

        public static float WidthPSIHorizontal { get; private set; }

        public static float WidthSpacingHorizontal => Settings.BarSettings.BaseSpacingHorizontal
                                                      + WidthMoodBarHorizontal + WidthPSIHorizontal;

        private static float HeightMoodBarVertical { get; set; }


        private static float WidthMoodBarHorizontal { get; set; }
        public static float SpaceBetweenColonistsHorizontal => 24f * Scale;

        [NotNull]
        public static List<Pawn> CaravanMembersInScreenRect(Rect rect)
        {
            BarHelperKF.TmpCaravanPawns.Clear();
            if (!Visible)
            {
                return BarHelperKF.TmpCaravanPawns;
            }

            var list = ColonistsOrCorpsesInScreenRect(rect);
            foreach (var thing in list)
            {
                if (thing is Pawn pawn && pawn.IsCaravanMember())
                {
                    BarHelperKF.TmpCaravanPawns.Add(pawn);
                }
            }

            return BarHelperKF.TmpCaravanPawns;
        }

        public static void Highlight(Pawn pawn)
        {
            if (Visible && !colonistsToHighlight.Contains(pawn))
            {
                colonistsToHighlight.Add(pawn);
            }
        }

        public static bool ColonistBarOnGUI_Prefix()
        {
            if (!Visible)
            {
                return false;
            }

            if (Event.current.type != EventType.Layout)
            {
                var entries = BarHelperKF.Entries;
                var num = -1;
                var showGroupFrames = BarHelperKF.ShowGroupFrames;
                var reorderableGroup = -1;
                for (var i = 0; i < BarHelperKF.DrawLocs.Count; i++)
                {
                    var rect = new Rect(
                        BarHelperKF.DrawLocs[i].x,
                        BarHelperKF.DrawLocs[i].y,
                        FullSize.x,
                        FullSize.y + SpacingLabel);
                    var entry = entries[i];
                    num = entry.group;
                    if (num != entry.group)
                    {
                        reorderableGroup = ReorderableWidget.NewGroup(entry.reorderAction,
                            ReorderableDirection.Horizontal, SpaceBetweenColonistsHorizontal,
                            entry.extraDraggedItemOnGUI);
                    }

                    bool reordering;
                    if (entry.pawn != null)
                    {
                        Drawer.HandleClicks(rect, entry.pawn, reorderableGroup, out reordering);
                    }
                    else
                    {
                        reordering = false;
                    }

                    if (Event.current.type != EventType.Repaint)
                    {
                        continue;
                    }

                    if (num != entry.group && showGroupFrames)
                    {
                        Drawer.DrawGroupFrame(entry.group);
                    }

                    if (entry.pawn != null)
                    {
                        Drawer.DrawColonist(rect, entry.pawn, entry.map, colonistsToHighlight.Contains(entry.pawn),
                            reordering);
                    }
                    else
                    {
                        Drawer.DrawEmptyFrame(rect, entry.map, entry.group);
                    }
                }

                num = -1;
                if (showGroupFrames)
                {
                    for (var j = 0; j < BarHelperKF.DrawLocs.Count; j++)
                    {
                        var entry2 = entries[j];
                        var entry2Group = entry2.group;
                        num = entry2Group;
                        if (num != entry2Group)
                        {
                            Drawer.HandleGroupFrameClicks(entry2Group);
                        }
                    }
                }
            }

            if (Event.current.type == EventType.Repaint)
            {
                colonistsToHighlight.Clear();
            }

            return false;
        }

        [NotNull]
        public static List<Thing> ColonistsOrCorpsesInScreenRect(Rect rect)
        {
            var drawLocs = BarHelperKF.DrawLocs;
            var entries = BarHelperKF.Entries;
            var size = FullSize;
            BarHelperKF.TmpColonistsWithMap.Clear();
            for (var i = 0; i < drawLocs.Count; i++)
            {
                if (!rect.Overlaps(new Rect(drawLocs[i].x, drawLocs[i].y, size.x, size.y)))
                {
                    continue;
                }

                var pawn = entries[i].pawn;
                if (pawn == null)
                {
                    continue;
                }

                Thing first;
                if (pawn.Dead && pawn.Corpse != null && pawn.Corpse.SpawnedOrAnyParentSpawned)
                {
                    first = pawn.Corpse;
                }
                else
                {
                    first = pawn;
                }

                BarHelperKF.TmpColonistsWithMap.Add(new Pair<Thing, Map>(first, entries[i].map));
            }

            var b = true;

            if (WorldRendererUtility.WorldRenderedNow)
            {
                if (BarHelperKF.TmpColonistsWithMap.Any(x => x.Second == null))
                {
                    BarHelperKF.TmpColonistsWithMap.RemoveAll(x => x.Second != null);
                    b = false;
                }
            }

            if (b)
            {
                if (BarHelperKF.TmpColonistsWithMap.Any(x => x.Second == Find.CurrentMap))
                {
                    BarHelperKF.TmpColonistsWithMap.RemoveAll(x => x.Second != Find.CurrentMap);
                }
            }

            BarHelperKF.TmpColonists.Clear();
            foreach (var pair in BarHelperKF.TmpColonistsWithMap)
            {
                BarHelperKF.TmpColonists.Add(pair.First);
            }

            BarHelperKF.TmpColonistsWithMap.Clear();
            return BarHelperKF.TmpColonists;
        }

        public static float GetEntryRectAlpha(Rect rect)
        {
            if (Messages.CollidesWithAnyMessage(rect, out var t))
            {
                return Mathf.Lerp(1f, 0.2f, t);
            }

            return 1f;
        }

        public static void RecalcSizes()
        {
            WidthMoodBarHorizontal = 0f;

            HeightMoodBarVertical = 0f;

            WidthPSIHorizontal = 0f;

            HeightPSIVertical = 0f;

            if (Settings.BarSettings.UseExternalMoodBar)
            {
                switch (Settings.BarSettings.MoodBarPos)
                {
                    case Position.Alignment.Left:
                    case Position.Alignment.Right:
                        WidthMoodBarHorizontal = Settings.BarSettings.BaseIconSize / 4;
                        break;

                    case Position.Alignment.Top:
                    case Position.Alignment.Bottom:
                        HeightMoodBarVertical = Settings.BarSettings.BaseIconSize / 4;
                        break;
                }
            }

            if (!Settings.BarSettings.UsePSI)
            {
                return;
            }

            switch (Settings.BarSettings.ColBarPSIIconPos)
            {
                case Position.Alignment.Left:
                case Position.Alignment.Right:
                    WidthPSIHorizontal = Settings.BarSettings.BaseIconSize
                        / Settings.BarSettings.IconsInColumn * PSIRowsOnBar;
                    break;

                case Position.Alignment.Top:
                case Position.Alignment.Bottom:
                    HeightPSIVertical = Settings.BarSettings.BaseIconSize
                        / Settings.BarSettings.IconsInColumn * PSIRowsOnBar;
                    break;
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace ColonistBarKF.Bar
{
    public class ColonistBarDrawLocsFinder_Kf
    {
        private readonly List<int> _entriesInGroup = new List<int>();

        private readonly List<int> _horizontalSlotsPerGroup = new List<int>();

        private static float MaxColonistBarWidth => UI.screenWidth - Settings.BarSettings.MarginHorizontal;

        public void CalculateDrawLocs([NotNull] List<Vector2> outDrawLocs, out float scale)
        {
            if (ColonistBar_KF.BarHelperKF.Entries.Count == 0)
            {
                outDrawLocs.Clear();
                scale = 1f;
                return;
            }

            CalculateColonistsInGroup();

            scale = FindBestScale(out var onlyOneRow, out var maxPerGlobalRow);

            CalculateDrawLocs(outDrawLocs, scale, onlyOneRow, maxPerGlobalRow);
        }

        // modded
        private static int GetAllowedRowsCountForScale(float scale)
        {
            if (Settings.BarSettings.UseCustomRowCount)
            {
                var maxRowsCustom = Settings.BarSettings.MaxRowsCustom;

                return Mathf.RoundToInt(Mathf.Lerp(maxRowsCustom, 1f, scale));
            }

            if (scale > 0.58f)
            {
                return 1;
            }

            if (scale > 0.42f)
            {
                return 2;
            }

            return 3;
        }

        private void CalculateColonistsInGroup()
        {
            _entriesInGroup.Clear();
            List<EntryKF> entries = ColonistBar_KF.BarHelperKF.Entries;
            var num = CalculateGroupsCount();
            for (var i = 0; i < num; i++)
            {
                _entriesInGroup.Add(0);
            }

            for (var j = 0; j < entries.Count; j++)
            {
                List<int> list;
                List<int> entryList = list = _entriesInGroup;
                int num2;
                var entryGroup = num2 = entries[j].@group;
                num2 = list[num2];
                entryList[entryGroup] = num2 + 1;
            }
        }

        private void CalculateDrawLocs([NotNull] List<Vector2> outDrawLocs, float scale, bool onlyOneRow, int maxPerGlobalRow)
        {
            outDrawLocs.Clear();
            var entriesCount = maxPerGlobalRow;
            if (onlyOneRow)
            {
                for (var i = 0; i < _horizontalSlotsPerGroup.Count; i++)
                {
                    _horizontalSlotsPerGroup[i] =
                        Mathf.Min(_horizontalSlotsPerGroup[i], _entriesInGroup[i]);
                }

                entriesCount = ColonistBar_KF.BarHelperKF.Entries.Count;
            }

            var groupsCount = CalculateGroupsCount();
            List<EntryKF> entries = ColonistBar_KF.BarHelperKF.Entries;
            var index = -1;
            var numInGroup = -1;

            var scaledEntryWidthFloat = (ColonistBar_KF.BaseSize.x + ColonistBar_KF.WidthSpacingHorizontal) * scale;
            var groupWidth = (entriesCount * scaledEntryWidthFloat) + ((groupsCount - 1) * 25f * scale);
            var groupStartX = (UI.screenWidth - groupWidth) / 2f;

            for (var j = 0; j < entries.Count; j++)
            {
                if (index != entries[j].@group)
                {
                    if (index >= 0)
                    {
                        groupStartX += 25f * scale;
                        groupStartX += _horizontalSlotsPerGroup[index] * scale
                                       * (ColonistBar_KF.BaseSize.x + ColonistBar_KF.WidthSpacingHorizontal);
                    }

                    numInGroup = 0;
                    index = entries[j].@group;
                }
                else
                {
                    numInGroup++;
                }

                Vector2 drawLoc = GetDrawLoc(
                    groupStartX,
                    Settings.BarSettings.MarginTop,
                    entries[j].@group,
                    numInGroup,
                    scale);
                outDrawLocs.Add(drawLoc);
            }
        }

        private int CalculateGroupsCount()
        {
            List<EntryKF> entries = ColonistBar_KF.BarHelperKF.Entries;
            var num = -1;
            var num2 = 0;
            for (var i = 0; i < entries.Count; i++)
            {
                if (num != entries[i].@group)
                {
                    num2++;
                    num = entries[i].@group;
                }
            }

            return num2;
        }

        //   private bool horizontal = true;

        private float FindBestScale(out bool onlyOneRow, out int maxPerGlobalRow)
        {
            var bestScale = 1f;
            List<EntryKF> entries = ColonistBar_KF.BarHelperKF.Entries;
            var groupsCount = CalculateGroupsCount();
            while (true)
            {
                // float num3 = (ColonistBar.BaseSize.x + 24f) * num;
                var neededPerEntry = (ColonistBar_KF.BaseSize.x + ColonistBar_KF.WidthSpacingHorizontal) * bestScale;
                var availableScreen = MaxColonistBarWidth - ((groupsCount - 1) * 25f * bestScale);

                maxPerGlobalRow = Mathf.FloorToInt(availableScreen / neededPerEntry);
                onlyOneRow = true;
                if (TryDistributeHorizontalSlotsBetweenGroups(maxPerGlobalRow))
                {
                    var allowedRowsCountForScale = GetAllowedRowsCountForScale(bestScale);
                    var flag = true;
                    var mapNum = -1;
                    for (var i = 0; i < entries.Count; i++)
                    {
                        if (mapNum != entries[i].@group)
                        {
                            mapNum = entries[i].@group;
                            var rows = Mathf.CeilToInt(_entriesInGroup[entries[i].@group]
                                / (float)_horizontalSlotsPerGroup[entries[i].@group]);
                            if (rows > 1)
                            {
                                onlyOneRow = false;
                            }

                            if (rows > allowedRowsCountForScale)
                            {
                                flag = false;
                                break;
                            }
                        }
                    }

                    if (flag)
                    {
                        break;
                    }
                }

                bestScale -= 0.03f;
            }

            return bestScale;
        }

        private Vector2 GetDrawLoc(float groupStartX, float groupStartY, int group, int numInGroup, float scale)
        {
            var x = groupStartX + (numInGroup % _horizontalSlotsPerGroup[group] * scale
                      * (ColonistBar_KF.BaseSize.x + ColonistBar_KF.WidthSpacingHorizontal));
            var y = groupStartY + (numInGroup / _horizontalSlotsPerGroup[group] * scale
                      * (ColonistBar_KF.BaseSize.y + ColonistBar_KF.HeightSpacingVertical));
            y += numInGroup / _horizontalSlotsPerGroup[group] * ColonistBar_KF.SpacingLabel;

            var flag = numInGroup >= _entriesInGroup[group]
                        - (_entriesInGroup[group] % _horizontalSlotsPerGroup[group]);
            if (flag)
            {
                var num2 = _horizontalSlotsPerGroup[group]
                           - (_entriesInGroup[group] % _horizontalSlotsPerGroup[group]);
                x += num2 * scale * (ColonistBar_KF.BaseSize.x + ColonistBar_KF.WidthSpacingHorizontal) * 0.5f;
            }

            return new Vector2(x, y);
        }

        private bool TryDistributeHorizontalSlotsBetweenGroups(int maxPerGlobalRow)
        {
            var groupsCount = CalculateGroupsCount();
            _horizontalSlotsPerGroup.Clear();
            for (var k = 0; k < groupsCount; k++)
            {
                _horizontalSlotsPerGroup.Add(0);
            }

            GenMath.DHondtDistribution(_horizontalSlotsPerGroup,
                i => (float)_entriesInGroup[i],
                maxPerGlobalRow);
            for (var j = 0; j < _horizontalSlotsPerGroup.Count; j++)
            {
                if (_horizontalSlotsPerGroup[j] == 0)
                {
                    var maxSlots = _horizontalSlotsPerGroup.Max();
                    if (maxSlots <= 1)
                    {
                        return false;
                    }

                    var num3 = _horizontalSlotsPerGroup.IndexOf(maxSlots);
                    List<int> list;
                    List<int> listInt = list = _horizontalSlotsPerGroup;
                    int num4;
                    var index = num4 = num3;
                    num4 = list[num4];
                    listInt[index] = num4 - 1;
                    List<int> list2;
                    List<int> slots = list2 = _horizontalSlotsPerGroup;
                    var integerK = num4 = j;
                    num4 = list2[num4];
                    slots[integerK] = num4 + 1;
                }
            }

            return true;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace ColonistBarKF.Bar;

// ReSharper disable once InconsistentNaming
public class ColBarHelper_KF : IExposable
{
    [NotNull] private readonly List<Map> _tmpMaps = new List<Map>();

    [NotNull] private readonly List<EntryKF> cachedEntries = new List<EntryKF>();

    [NotNull] public readonly List<Pair<Thing, Map>> TmpColonistsWithMap = new List<Pair<Thing, Map>>();

    public float CachedScale;

    public int DisplayGroupForBar;

    public bool EntriesDirty = true;
    [NotNull] public List<Pawn> TmpCaravanPawns = new List<Pawn>();

    [NotNull] public List<Caravan> TmpCaravans = new List<Caravan>();

    [NotNull] public List<Thing> TmpColonists = new List<Thing>();

    [NotNull] public List<Pawn> TmpColonistsInOrder = new List<Pawn>();

    [NotNull] public List<Thing> TmpMapColonistsOrCorpsesInScreenRect = new List<Thing>();

    [NotNull] private List<Pawn> tmpPawns = new List<Pawn>();

    [NotNull] public List<Vector2> DrawLocs { get; } = new List<Vector2>();

    [NotNull]
    public List<EntryKF> Entries
    {
        get
        {
            CheckRecacheEntries();
            return cachedEntries;
        }
    }

    public bool ShowGroupFrames
    {
        get
        {
            var entries = Entries;
            var num = -1;
            for (var i = 0; i < entries.Count; i++)
            {
                num = Mathf.Max(num, entries[i].group);
            }

            return num >= 1;
        }
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref DisplayGroupForBar, "displayGroupForBar");
    }

    public bool AnyColonistOrCorpseAt(Vector2 pos)
    {
        if (!TryGetEntryAt(pos, out var entry))
        {
            return false;
        }

        return entry.pawn != null;
    }

    public void DrawColonistMouseAttachment(int index, Vector2 dragStartPos, int entryGroup)
    {
        Pawn pawn = null;
        Vector2 vector = default;
        var num = 0;
        for (var i = 0; i < cachedEntries.Count; i++)
        {
            if (cachedEntries[i].group != entryGroup || cachedEntries[i].pawn == null)
            {
                continue;
            }

            if (num == index)
            {
                pawn = cachedEntries[i].pawn;
                vector = DrawLocs[i];
                break;
            }

            num++;
        }

        if (pawn == null)
        {
            return;
        }

        var iconTex = PortraitsCache.Get(pawn, ColonistBarColonistDrawer.PawnTextureSize, Rot4.South,
            ColonistBarColonistDrawer.PawnTextureCameraOffset, 1.28205f);
        var rect = new Rect(vector.x, vector.y, ColonistBar_KF.FullSize.x, ColonistBar_KF.FullSize.y);
        var pawnTextureRect = ColonistBar_KF.Drawer.GetPawnTextureRect(rect.position);
        pawnTextureRect.position += Event.current.mousePosition - dragStartPos;
        GenUI.DrawMouseAttachment(iconTex, "", 0f, default, pawnTextureRect);
    }


    public void Reorder(int from, int to, int entryGroup)
    {
        var num = 0;
        Pawn pawn = null;
        Pawn pawn2 = null;
        Pawn pawn3 = null;
        for (var i = 0; i < cachedEntries.Count; i++)
        {
            if (cachedEntries[i].group != entryGroup || cachedEntries[i].pawn == null)
            {
                continue;
            }

            if (num == from)
            {
                pawn = cachedEntries[i].pawn;
            }

            if (num == to)
            {
                pawn2 = cachedEntries[i].pawn;
            }

            pawn3 = cachedEntries[i].pawn;
            num++;
        }

        if (pawn == null)
        {
            return;
        }

        var num2 = pawn2?.playerSettings.displayOrder ?? pawn3.playerSettings.displayOrder + 1;
        for (var j = 0; j < cachedEntries.Count; j++)
        {
            var pawn4 = cachedEntries[j].pawn;
            if (pawn4 == null)
            {
                continue;
            }

            if (pawn4.playerSettings.displayOrder == num2)
            {
                if (pawn2 == null || cachedEntries[j].group != entryGroup)
                {
                    continue;
                }

                if (pawn4.thingIDNumber < pawn2.thingIDNumber)
                {
                    pawn4.playerSettings.displayOrder--;
                }
                else
                {
                    pawn4.playerSettings.displayOrder++;
                }
            }
            else if (pawn4.playerSettings.displayOrder > num2)
            {
                pawn4.playerSettings.displayOrder++;
            }
            else
            {
                pawn4.playerSettings.displayOrder--;
            }
        }

        pawn.playerSettings.displayOrder = num2;
        HarmonyPatches.MarkColonistsDirty_Postfix();
        MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
    }

    public bool TryGetEntryAt(Vector2 pos, out EntryKF entry)
    {
        var entries = Entries;
        var size = ColonistBar_KF.FullSize;
        for (var i = 0; i < DrawLocs.Count; i++)
        {
            var rect = new Rect(DrawLocs[i].x, DrawLocs[i].y, size.x, size.y);
            if (!rect.Contains(pos))
            {
                continue;
            }

            entry = entries[i];
            return true;
        }

        entry = default;
        return false;
    }

    private static void SortCachedColonists([NotNull] ref List<Pawn> tmpColonists)
    {
        List<Pawn> sort;
        List<Pawn> others;

        List<Pawn> orderedEnumerable;
        switch (Settings.BarSettings.SortBy)
        {
            case SettingsColonistBar.SortByWhat.byName:
            {
                tmpColonists.SortBy(x => x.LabelCap);
                break;
            }

            case SettingsColonistBar.SortByWhat.sexage:
            {
                orderedEnumerable = tmpColonists.OrderBy(x => x.gender.GetLabel() != null)
                    .ThenBy(x => x.gender.GetLabel()).ThenBy(x => x.ageTracker?.AgeBiologicalYears).ToList();
                tmpColonists = orderedEnumerable;
                break;
            }

            case SettingsColonistBar.SortByWhat.age:
            {
                orderedEnumerable = tmpColonists.OrderBy(x => x.ageTracker?.AgeBiologicalYears).ToList();
                tmpColonists = orderedEnumerable;
                break;
            }

            case SettingsColonistBar.SortByWhat.health:
            {
                tmpColonists.SortBy(x => x.health.summaryHealth.SummaryHealthPercent);
                break;
            }

            case SettingsColonistBar.SortByWhat.bleedRate:
            {
                tmpColonists.SortByDescending(x => x.health.hediffSet.BleedRateTotal);
                break;
            }

            case SettingsColonistBar.SortByWhat.mood:
            {
                tmpColonists.SortBy(x => x.needs?.mood?.CurInstantLevelPercentage ?? 0f);

                // tmpColonists.SortBy(x => x.needs.mood.CurLevelPercentage);
                break;
            }
            //inverted the order, the melee goes to the end of the list
            case SettingsColonistBar.SortByWhat.weapons:
            {
                orderedEnumerable = tmpColonists
                    .OrderByDescending(a => a?.equipment?.Primary?.def?.IsRangedWeapon)
                    .ThenByDescending(b => b?.skills?.GetSkill(SkillDefOf.Shooting).Level)
                    .ThenByDescending(c => c?.equipment?.Primary?.def?.IsMeleeWeapon == true).ToList();
                tmpColonists = orderedEnumerable;
                break;
            }


            case SettingsColonistBar.SortByWhat.medicTendQuality:
            {
                sort = tmpColonists.Where(x => !x?.WorkTypeIsDisabled(WorkTypeDefOf.Doctor) ?? false).ToList();
                others = tmpColonists.Where(x => x?.WorkTypeIsDisabled(WorkTypeDefOf.Doctor) ?? true).ToList();

                sort.SortByDescending(b => b.GetStatValue(StatDefOf.MedicalTendQuality));
                others.SortBy(x => x.LabelCap);

                sort.AddRange(others);

                tmpColonists = sort;
                break;
            }

            case SettingsColonistBar.SortByWhat.medicSurgerySuccess:
            {
                sort = tmpColonists.Where(x => !x?.WorkTypeIsDisabled(WorkTypeDefOf.Doctor) ?? false).ToList();
                others = tmpColonists.Where(x => x?.WorkTypeIsDisabled(WorkTypeDefOf.Doctor) ?? true).ToList();

                sort.SortByDescending(b => b.GetStatValue(StatDefOf.MedicalSurgerySuccessChance));
                others.SortBy(x => x.LabelCap);

                sort.AddRange(others);

                tmpColonists = sort;
                break;
            }

            case SettingsColonistBar.SortByWhat.diplomacy:
            {
                sort = tmpColonists.Where(x => !x?.WorkTagIsDisabled(WorkTags.Social) ?? false).ToList();
                others = tmpColonists.Where(x => x?.WorkTagIsDisabled(WorkTags.Social) ?? true).ToList();

                sort.SortByDescending(b => b.GetStatValue(StatDefOf.NegotiationAbility));
                others.SortBy(x => x.LabelCap);

                sort.AddRange(others);

                tmpColonists = sort;
                break;
            }

            case SettingsColonistBar.SortByWhat.tradePrice:
            {
                sort = tmpColonists.Where(x => !x?.WorkTagIsDisabled(WorkTags.Social) ?? false).ToList();
                others = tmpColonists.Where(x => x?.WorkTagIsDisabled(WorkTags.Social) ?? true).ToList();

                sort.SortByDescending(b => b.GetStatValue(StatDefOf.TradePriceImprovement));
                others.SortBy(x => x.LabelCap);

                sort.AddRange(others);

                tmpColonists = sort;
                break;
            }
            //shooting accuracy
            case SettingsColonistBar.SortByWhat.shootingAccuracy:
            {
                sort = tmpColonists.Where(x => !x?.WorkTypeIsDisabled(WorkTypeDefOf.Hunting) ?? false).ToList();
                others = tmpColonists.Where(x => x?.WorkTypeIsDisabled(WorkTypeDefOf.Hunting) ?? true).ToList();

                sort.SortByDescending(b => b.GetStatValue(StatDefOf.ShootingAccuracyPawn));
                others.SortBy(x => x.LabelCap);

                sort.AddRange(others);

                tmpColonists = sort;
                break;
            }
            //sort by shotting skill
            case SettingsColonistBar.SortByWhat.shootingSkill:
            {
                orderedEnumerable = tmpColonists
                    .OrderByDescending(a => a?.skills?.GetSkill(SkillDefOf.Shooting)?.Level)
                    .ThenBy(b => b?.skills?.GetSkill(SkillDefOf.Shooting)?.TotallyDisabled).ToList();

                tmpColonists = orderedEnumerable;
                break;
            }

            case SettingsColonistBar.SortByWhat.moveSpeed:
            {
                tmpColonists.SortByDescending(x => x.GetStatValue(StatDefOf.MoveSpeed));
                break;
            }
            default:
            {
                tmpColonists.SortBy(x => x.thingIDNumber);
                break;
            }
        }

        if (Settings.BarSettings.ReverseSort)
        {
            tmpColonists.Reverse();
        }

        Settings.SaveBarSettings();
    }

    private void CheckRecacheEntries()
    {
        if (!EntriesDirty)
        {
            return;
        }

        EntriesDirty = false;
        cachedEntries.Clear();
        if (Find.PlaySettings.showColonistBar)
        {
            _tmpMaps.Clear();
            _tmpMaps.AddRange(Find.Maps);
            _tmpMaps.SortBy(x => !x.IsPlayerHome, x => x.uniqueID);
            var groupInt = 0;
            foreach (var tempMap in _tmpMaps)
            {
                tmpPawns.Clear();
                tmpPawns.AddRange(tempMap.mapPawns.FreeColonists);
                var list = tempMap.listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
                foreach (var thing in list)
                {
                    if (thing.IsDessicated())
                    {
                        continue;
                    }

                    var innerPawn = ((Corpse)thing).InnerPawn;
                    if (innerPawn == null)
                    {
                        continue;
                    }

                    if (innerPawn.IsColonist)
                    {
                        tmpPawns.Add(innerPawn);
                    }
                }

                var allPawnsSpawned = tempMap.mapPawns.AllPawnsSpawned;
                foreach (var corpse in allPawnsSpawned
                             .Select(spawnedPawn => spawnedPawn.carryTracker.CarriedThing as Corpse).Where(
                                 corpse => corpse != null && !corpse.IsDessicated() && corpse.InnerPawn.IsColonist))
                {
                    tmpPawns.Add(corpse.InnerPawn);
                }

                // tmpPawns.SortBy((Pawn x) => x.thingIDNumber);
                if (Settings.BarSettings.SortBy == SettingsColonistBar.SortByWhat.vanilla)
                {
                    PlayerPawnsDisplayOrderUtility.Sort(tmpPawns);
                }
                else
                {
                    SortCachedColonists(ref tmpPawns);
                }

                for (var index = 0; index < tmpPawns.Count; index++)
                {
                    var tempPawn = tmpPawns[index];
                    cachedEntries.Add(new EntryKF(tempPawn, tempMap, groupInt));

                    if (!Settings.BarSettings.UseGrouping || groupInt == DisplayGroupForBar)
                    {
                        continue;
                    }

                    if (cachedEntries.FindAll(x => x.group == groupInt).Count <= 2)
                    {
                        continue;
                    }

                    cachedEntries.Add(new EntryKF(null, tempMap, groupInt));
                    break;
                }

                if (!tmpPawns.Any())
                {
                    cachedEntries.Add(new EntryKF(null, tempMap, groupInt));
                }

                groupInt++;
            }

            TmpCaravans.Clear();
            TmpCaravans.AddRange(Find.WorldObjects.Caravans);
            TmpCaravans.SortBy(x => x.ID);
            foreach (var caravan in TmpCaravans.Where(caravan => caravan.IsPlayerControlled))
            {
                tmpPawns.Clear();
                tmpPawns.AddRange(caravan.PawnsListForReading);

                // tmpPawns.SortBy((Pawn x) => x.thingIDNumber);
                if (Settings.BarSettings.SortBy == SettingsColonistBar.SortByWhat.vanilla)
                {
                    PlayerPawnsDisplayOrderUtility.Sort(tmpPawns);
                }
                else
                {
                    SortCachedColonists(ref tmpPawns);
                }

                foreach (var tempPawn in tmpPawns.Where(tempPawn => tempPawn.IsColonist))
                {
                    cachedEntries.Add(
                        new EntryKF(
                            tempPawn,
                            null,
                            groupInt));

                    if (!Settings.BarSettings.UseGrouping || groupInt == DisplayGroupForBar)
                    {
                        continue;
                    }

                    if (cachedEntries.FindAll(x => x.group == groupInt).Count <= 2)
                    {
                        continue;
                    }

                    cachedEntries.Add(
                        new EntryKF(
                            null,
                            null,
                            groupInt));
                    break;
                }

                groupInt++;
            }
        }

        // RecacheDrawLocs();
        ColonistBar_KF.Drawer.Notify_RecachedEntries();
        tmpPawns.Clear();
        _tmpMaps.Clear();
        TmpCaravans.Clear();
        ColonistBar_KF.DrawLocsFinder.CalculateDrawLocs(DrawLocs, out CachedScale);
    }
}
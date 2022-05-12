﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ColonistBarKF.Bar;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace ColonistBarKF;

[StaticConstructorOnStartup]
internal static class HarmonyPatches
{
    static HarmonyPatches()
    {
        var injected = false;
        var patchLog = "Start injecting PSI to pawns ...";
        foreach (var def in DefDatabase<ThingDef>.AllDefs.Where(
                     x => x.race != null && x.race.Humanlike &&
                          x.race.IsFlesh))
        {
            patchLog += "\nPSI check: " + def;
            if (def.comps == null)
            {
                continue;
            }

            def.comps.Add(new CompProperties(typeof(CompPSI)));
            patchLog += " - PSI injected.";
            injected = true;
        }

        patchLog += injected ? string.Empty : "\nNo pawns found for PSI :(";
        Log.Message(patchLog);

        var harmony = new Harmony("com.colonistbarkf.rimworld.mod");

        harmony.Patch(
            AccessTools.Method(typeof(ColonistBar), nameof(ColonistBar.ColonistBarOnGUI)),
            new HarmonyMethod(typeof(ColonistBar_KF), nameof(ColonistBar_KF.ColonistBarOnGUI_Prefix)));

        harmony.Patch(
            AccessTools.Method(typeof(ColonistBar),
                nameof(ColonistBar.MapColonistsOrCorpsesInScreenRect)),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(MapColonistsOrCorpsesInScreenRect_Prefix)));

        harmony.Patch(
            AccessTools.Method(typeof(ColonistBar),
                nameof(ColonistBar.CaravanMembersCaravansInScreenRect)),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(CaravanMembersCaravansInScreenRect_Prefix)));

        harmony.Patch(
            AccessTools.Method(typeof(ColonistBar), nameof(ColonistBar.ColonistOrCorpseAt)),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(ColonistOrCorpseAt_Prefix)));

        harmony.Patch(
            AccessTools.Method(typeof(ColonistBar), nameof(ColonistBar.CaravanMemberCaravanAt)),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(CaravanMemberCaravanAt_Prefix)));

        harmony.Patch(
            AccessTools.Method(typeof(ColonistBar), nameof(ColonistBar.GetColonistsInOrder)),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(GetColonistsInOrder_Prefix)));

        harmony.Patch(
            AccessTools.Method(typeof(ColonistBar), nameof(ColonistBar.MarkColonistsDirty)),
            null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(MarkColonistsDirty_Postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(ColonistBar), nameof(ColonistBar.Highlight)),
            null,
            new HarmonyMethod(typeof(ColonistBar_KF), nameof(ColonistBar_KF.Highlight)));

        harmony.Patch(
            AccessTools.Method(typeof(Caravan), nameof(Caravan.Notify_PawnAdded)),
            null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(EntriesDirty_Postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(Caravan), nameof(Caravan.Notify_PawnRemoved)),
            null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(EntriesDirty_Postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(Caravan), nameof(Caravan.PostAdd)),
            null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(EntriesDirty_Postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(Caravan), nameof(Caravan.PostRemove)),
            null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(EntriesDirty_Postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(Game), nameof(Game.AddMap)),
            null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(EntriesDirty_Postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(Window), nameof(Window.Notify_ResolutionChanged)),
            null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(IsPlayingDirty_Postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(Game), nameof(Game.DeinitAndRemoveMap)),
            null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(IsPlayingDirty_Postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(Pawn), nameof(Pawn.SetFaction)),
            null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(EntriesDirty_Postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(Pawn), nameof(Pawn.SpawnSetup)),
            null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_SpawnSetup_Postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(Pawn), nameof(Pawn.Kill)),
            null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_Kill_Postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.Notify_Resurrected)),
            null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_Resurrected_Postfix)));

        // NOT WORKING, FollowMe immediatly cancels if this is active
        // harmony.Patch(
        // AccessTools.Method(typeof(CameraDriver), nameof(CameraDriver.JumpToCurrentMapLoc), new[] { typeof(Vector3) }),
        // new HarmonyMethod(typeof(HarmonyPatches), nameof(StopFollow_Prefix)),
        // null);
        harmony.Patch(
            AccessTools.Method(
                typeof(WorldCameraDriver),
                nameof(WorldCameraDriver.JumpTo),
                new[] { typeof(Vector3) }),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(StopFollow_Prefix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(ThingSelectionUtility),
                nameof(ThingSelectionUtility.SelectNextColonist)),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(StartFollowSelectedColonist1)),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(StartFollowSelectedColonist2)));

        harmony.Patch(
            AccessTools.Method(
                typeof(ThingSelectionUtility),
                nameof(ThingSelectionUtility.SelectPreviousColonist)),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(StartFollowSelectedColonist1)),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(StartFollowSelectedColonist2)));

        harmony.Patch(
            AccessTools.Method(
                typeof(CameraDriver),
                nameof(CameraDriver.JumpToCurrentMapLoc),
                new[] { typeof(Vector3) }),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(StopFollow_Prefix_Vector3)));

        harmony.Patch(
            AccessTools.Method(typeof(Pawn), nameof(Pawn.PostApplyDamage)),
            null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_PostApplyDamage_Postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(Corpse), "NotifyColonistBar"),
            null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(NotifyColonistBar_Postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(MapPawns), "DoListChangedNotifications"),
            null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(IsColonistBarNull_Postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(ThingOwner), "NotifyColonistBarIfColonistCorpse"),
            null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(NotifyColonistBarIfColonistCorpse_Postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(Thing), nameof(Thing.DeSpawn)),
            null,
            new HarmonyMethod(typeof(HarmonyPatches), nameof(DeSpawn_Postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls)),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(PlaySettingsDirty_Prefix)),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(PlaySettingsDirty_Postfix)));

        Log.Message(
            "Colonistbar KF successfully completed " + harmony.GetPatchedMethods().Count()
                                                     + " patches with harmony.");
    }

    public static void MarkColonistsDirty_Postfix()
    {
        ColonistBar_KF.RecalcSizes();
        ColonistBar_KF.BarHelperKF.EntriesDirty = true;

        // Log.Message("Colonists marked dirty.01");
    }

    private static bool CaravanMemberCaravanAt_Prefix([CanBeNull] ref Caravan __result, Vector2 at)
    {
        if (!ColonistBar_KF.Visible)
        {
            __result = null;
            return false;
        }

        Thing thing = null;
        ColonistOrCorpseAt_Prefix(ref thing, at);

        if (thing is Pawn pawn && pawn.IsCaravanMember())
        {
            __result = pawn.GetCaravan();
            return false;
        }

        __result = null;
        return false;
    }

    private static bool CaravanMembersCaravansInScreenRect_Prefix([NotNull] ref List<Caravan> __result, Rect rect)
    {
        ColonistBar_KF.BarHelperKF.TmpCaravans.Clear();
        if (!ColonistBar_KF.Visible)
        {
            __result = ColonistBar_KF.BarHelperKF.TmpCaravans;
            return false;
        }

        var list = ColonistBar_KF.CaravanMembersInScreenRect(rect);
        foreach (var pawn in list)
        {
            ColonistBar_KF.BarHelperKF.TmpCaravans.Add(pawn.GetCaravan());
        }

        __result = ColonistBar_KF.BarHelperKF.TmpCaravans;
        return false;
    }

    private static bool ColonistOrCorpseAt_Prefix([CanBeNull] ref Thing __result, Vector2 pos)
    {
        if (!ColonistBar_KF.Visible)
        {
            __result = null;
            return false;
        }

        if (!ColonistBar_KF.BarHelperKF.TryGetEntryAt(pos, out var entry))
        {
            __result = null;
            return false;
        }

        var pawn = entry.pawn;
        Thing result;
        if (pawn != null && pawn.Dead && pawn.Corpse != null && pawn.Corpse.SpawnedOrAnyParentSpawned)
        {
            result = pawn.Corpse;
        }
        else
        {
            result = pawn;
        }

        __result = result;
        return false;
    }

    private static void DeSpawn_Postfix([NotNull] Thing __instance)
    {
        if (!(__instance is Pawn pawn))
        {
            return;
        }

        if (pawn.Faction != Faction.OfPlayer)
        {
            return;
        }

        if (!pawn.RaceProps.Humanlike)
        {
            return;
        }

        EntriesDirty_Postfix();
    }

    private static void EntriesDirty_Postfix()
    {
        ColonistBar_KF.BarHelperKF.EntriesDirty = true;
    }

    private static bool GetColonistsInOrder_Prefix([NotNull] ref List<Pawn> __result)
    {
        var entries = ColonistBar_KF.BarHelperKF.Entries;
        ColonistBar_KF.BarHelperKF.TmpColonistsInOrder.Clear();
        for (var i = 0; i < entries.Count; i++)
        {
            if (entries[i].pawn != null)
            {
                ColonistBar_KF.BarHelperKF.TmpColonistsInOrder.Add(entries[i].pawn);
            }
        }

        __result = ColonistBar_KF.BarHelperKF.TmpColonistsInOrder;
        return false;
    }

    private static void IsColonistBarNull_Postfix()
    {
        if (Find.ColonistBar != null)
        {
            EntriesDirty_Postfix();
        }
    }

    private static void IsPlayingDirty_Postfix()
    {
        if (Current.ProgramState == ProgramState.Playing)
        {
            EntriesDirty_Postfix();
        }
    }

    private static bool MapColonistsOrCorpsesInScreenRect_Prefix(ref List<Thing> __result, Rect rect)
    {
        ColonistBar_KF.BarHelperKF.TmpMapColonistsOrCorpsesInScreenRect.Clear();
        if (!ColonistBar_KF.Visible)
        {
            __result = ColonistBar_KF.BarHelperKF.TmpMapColonistsOrCorpsesInScreenRect;
            return false;
        }

        var list = ColonistBar_KF.ColonistsOrCorpsesInScreenRect(rect);
        foreach (var thing in list)
        {
            if (thing.Spawned)
            {
                ColonistBar_KF.BarHelperKF.TmpMapColonistsOrCorpsesInScreenRect.Add(thing);
            }
        }

        __result = ColonistBar_KF.BarHelperKF.TmpMapColonistsOrCorpsesInScreenRect;
        return false;
    }

    private static void NotifyColonistBar_Postfix([NotNull] Corpse __instance)
    {
        var innerPawn = __instance.InnerPawn;

        if (innerPawn == null)
        {
            return;
        }

        if (innerPawn.Faction == Faction.OfPlayer && Current.ProgramState == ProgramState.Playing)
        {
            EntriesDirty_Postfix();

            // Log.Message("Colonists marked dirty.x07");
        }
    }

    private static void NotifyColonistBarIfColonistCorpse_Postfix([NotNull] Thing __instance)
    {
        if (Current.ProgramState != ProgramState.Playing)
        {
            return;
        }


        if (__instance is not Corpse corpse)
        {
            return;
        }

        if (corpse.Bugged)
        {
            return;
        }

        if (corpse.InnerPawn != null && corpse.InnerPawn.Faction?.IsPlayer == true)
        {
            EntriesDirty_Postfix();
        }
    }

    // ReSharper disable once InconsistentNaming
    private static void Pawn_Kill_Postfix([NotNull] Pawn __instance)
    {
        if (__instance.Faction?.IsPlayer != true || Current.ProgramState != ProgramState.Playing)
        {
            return;
        }

        EntriesDirty_Postfix();
        var compPSI = __instance.GetComp<CompPSI>();
        if (compPSI == null)
        {
            return;
        }

        compPSI.BgColor = Color.gray;
        compPSI.ThisColCount = 0;
    }

    // ReSharper disable once InconsistentNaming
    private static void Pawn_Resurrected_Postfix([NotNull] Pawn_HealthTracker __instance)
    {
        var pawnFieldInfo =
            typeof(Pawn_HealthTracker).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
        var pawn = (Pawn)pawnFieldInfo?.GetValue(__instance);

        if (pawn?.Faction == null || !pawn.Faction.IsPlayer || Current.ProgramState != ProgramState.Playing)
        {
            return;
        }

        EntriesDirty_Postfix();
        var compPSI = pawn.GetComp<CompPSI>();
        compPSI?.CheckTraits();
    }

    private static void Pawn_PostApplyDamage_Postfix([NotNull] Pawn __instance)
    {
        var compPSI = __instance.GetComp<CompPSI>();

        compPSI?.SetEntriesDirty();
    }

    private static void Pawn_SpawnSetup_Postfix(Pawn __instance)
    {
        if (__instance == null)
        {
            return;
        }

        if (!__instance.RaceProps.Humanlike || !__instance.RaceProps.IsFlesh)
        {
            return;
        }

        if (__instance is IThingHolder && Find.ColonistBar != null)
        {
            EntriesDirty_Postfix();
        }
    }

    private static void PlaySettingsDirty_Postfix(bool __state)
    {
        if (__state != Find.PlaySettings.showColonistBar)
        {
            EntriesDirty_Postfix();
        }
    }

    // ReSharper disable once RedundantAssignment
    private static void PlaySettingsDirty_Prefix(ref bool __state)
    {
        __state = Find.PlaySettings.showColonistBar;
    }

    private static void StopFollow_Prefix()
    {
        if (FollowMe.CurrentlyFollowing)
        {
            FollowMe.StopFollow("Harmony");
        }
    }

    private static void StartFollowSelectedColonist1(ref bool __state)
    {
        __state = FollowMe.CurrentlyFollowing;
    }

    private static void StartFollowSelectedColonist2(bool __state)
    {
        if (__state)
        {
            FollowMe.TryStartFollow(Find.Selector.SingleSelectedThing);
        }
    }

    private static void StopFollow_Prefix_Vector3([NotNull] Vector3 loc)
    {
        if (!FollowMe.CurrentlyFollowing)
        {
            return;
        }

        if (FollowMe.FollowedThing.TrueCenter() != loc)
        {
            FollowMe.StopFollow("Harmony");
        }
    }
}
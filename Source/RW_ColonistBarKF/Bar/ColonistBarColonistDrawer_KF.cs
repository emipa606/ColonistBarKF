﻿namespace ColonistBarKF.Bar
{
    using ColonistBarKF.PSI;
    using JetBrains.Annotations;
    using KillfaceTools.FMO;
    using RimWorld;
    using RimWorld.Planet;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;
    using Verse.AI;
    using Verse.AI.Group;
    using Verse.Sound;

    [StaticConstructorOnStartup]
    public class ColonistBarColonistDrawer_KF
    {
        [NotNull]
        private static readonly Vector2[] bracketLocs = new Vector2[4];

        private static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private static Vector3 pawnTextureCameraOffset;

        [NotNull]
        private readonly Dictionary<string, string> pawnLabelsCache = new Dictionary<string, string>();

        private static Vector3 PawnTextureCameraOffset
        {
            get
            {
                float pawnTextureCameraOffsetNew = Settings.barSettings.PawnTextureCameraZoom / 1.28205f;
                float posx = Settings.barSettings.PawnTextureCameraHorizontalOffset / pawnTextureCameraOffsetNew;
                float posz = Settings.barSettings.PawnTextureCameraVerticalOffset / pawnTextureCameraOffsetNew;
                pawnTextureCameraOffset = new Vector3(posx, 0f, posz);
                return pawnTextureCameraOffset;
            }
        }

        private static Vector2 PawnTextureSize => new Vector2(
            Settings.barSettings.BaseIconSize - 2f,
            Settings.barSettings.BaseIconSize * 1.5f);

        [CanBeNull]
        private static Pawn SelPawn => Find.Selector.SingleSelectedThing as Pawn;

        public void DrawColonist(Rect outerRect, [NotNull] Pawn colonist, [CanBeNull] Map pawnMap)
        {
            CompPSI psiComp = colonist.GetComp<CompPSI>();
            Rect pawnRect = new Rect(outerRect.x, outerRect.y, ColonistBar_KF.PawnSize.x, ColonistBar_KF.PawnSize.y);

            // if (pawnStats.IconCount == 0)
            // outerRect.width
            float entryRectAlpha = ColonistBar_KF.GetEntryRectAlpha(outerRect);
            this.ApplyEntryInAnotherMapAlphaFactor(pawnMap, outerRect, ref entryRectAlpha);

            bool colonistAlive = !colonist.Dead
                                     ? Find.Selector.SelectedObjects.Contains(colonist)
                                     : Find.Selector.SelectedObjects.Contains(colonist.Corpse);

            Color color = new Color(1f, 1f, 1f, entryRectAlpha);
            GUI.color = color;

            // testing
            // Widgets.DrawBox(outerRect);
            if (psiComp != null)
            {
                BuildRects(
                    psiComp.thisColCount,
                    ref outerRect,
                    ref pawnRect,
                    out Rect moodBorderRect,
                    out Rect psiRect);

                // Widgets.DrawBoxSolid(outerRect, new Color(0.5f, 1f, 0.5f, 0.5f));
                Color background = color;
                Texture2D tex2 = Textures.BgTexVanilla;
                if (Settings.barSettings.UseGender)
                {
                    background = psiComp.BGColor;
                    tex2 = Textures.BgTexGrey;
                    background.a = entryRectAlpha;
                    GUI.color = background;
                }

                GUI.DrawTexture(pawnRect, tex2);

                GUI.color = color;

                if (!colonist.Dead)
                {
                    if (psiComp.Mood?.thoughts != null)
                    {
                        if (Settings.barSettings.UseExternalMoodBar || Settings.barSettings.UseNewMood)
                        {
                            if (psiComp.Mood != null && psiComp.Mb != null)
                            {
                                // string tooltip = colonist.needs.mood.GetTipString();
                                DrawNewMoodRect(moodBorderRect, psiComp.Mood, psiComp.Mb);
                            }
                        }
                        else
                        {
                            Rect position = pawnRect.ContractedBy(2f);
                            float num = position.height * colonist.needs.mood.CurLevelPercentage;
                            position.yMin = position.yMax - num;
                            position.height = num;
                            GUI.DrawTexture(position, Textures.VanillaMoodBgTex);
                        }
                    }

                    // PSI
                    if (Settings.barSettings.UsePsi)
                    {
                        colonist.DrawColonistIconsBar(psiRect, entryRectAlpha);
                    }
                }
            }
            else
            {
                GUI.color = color;
                GUI.DrawTexture(pawnRect, Textures.BgTexVanilla);
            }

            GUI.color = color;

            // Rect rect2 = outerRect.ContractedBy(-2f * ColonistBar_KF.Scale);
            Rect rect2 = outerRect.ContractedBy(-2f);

            if (colonistAlive && !WorldRendererUtility.WorldRenderedNow)
            {
                if (FollowMe.CurrentlyFollowing)
                {
                    Color col = Textures.ColBlueishGreen;

                    Pawn follow = FollowMe._followedThing as Pawn;
                    if (follow != null)
                    {
                        if (follow == colonist)
                        {
                            col = Textures.ColSkyBlue;
                        }
                    }

                    col.a = color.a;
                    GUI.color = col;
                }

                this.DrawSelectionOverlayOnGUI(colonist, rect2);
            }
            else if (WorldRendererUtility.WorldRenderedNow && colonist.IsCaravanMember()
                     && Find.WorldSelector.IsSelected(colonist.GetCaravan()))
            {
                this.DrawCaravanSelectionOverlayOnGUI(colonist.GetCaravan(), rect2);
            }

            GUI.color = color;

            GUI.DrawTexture(
                this.GetPawnTextureRect(pawnRect.x, pawnRect.y),
                PortraitsCache.Get(
                    colonist,
                    PawnTextureSize,
                    PawnTextureCameraOffset,
                    Settings.barSettings.PawnTextureCameraZoom));
            if (colonist.CurJob != null)
            {
                DrawCurrentJobTooltip(colonist, pawnRect);
            }

            if (Settings.barSettings.UseWeaponIcons)
            {
                this.DrawWeaponIcon(pawnRect, entryRectAlpha, colonist);
            }

            GUI.color = new Color(1f, 1f, 1f, entryRectAlpha * 0.8f);
            this.DrawIcons(pawnRect, colonist);
            GUI.color = color;
            if (colonist.Dead)
            {
                GUI.DrawTexture(pawnRect, Textures.DeadColonistTex);
            }

            // float num = 4f * Scale;
            Vector2 pos = new Vector2(pawnRect.center.x, pawnRect.yMax + 1f * ColonistBar_KF.Scale);
            GenMapUI.DrawPawnLabel(colonist, pos, entryRectAlpha, pawnRect.width, this.pawnLabelsCache);

            GUI.color = Color.white;
        }

        public void DrawEmptyFrame(Rect outerRect, [CanBeNull] Map pawnMap, int groupCount)
        {
            Rect pawnRect = new Rect(outerRect.x, outerRect.y, ColonistBar_KF.PawnSize.x, ColonistBar_KF.PawnSize.y);
            pawnRect.x += (outerRect.width - pawnRect.width) / 2;

            // if (pawnStats.IconCount == 0)
            // outerRect.width
            float entryRectAlpha = ColonistBar_KF.GetEntryRectAlpha(outerRect);
            this.ApplyEntryInAnotherMapAlphaFactor(pawnMap, outerRect, ref entryRectAlpha);

            Color color = new Color(1f, 1f, 1f, entryRectAlpha);
            GUI.color = color;

            // testing
            // Widgets.DrawBox(outerRect);

            // Widgets.DrawBoxSolid(outerRect, new Color(0.5f, 1f, 0.5f, 0.5f));
            GUI.DrawTexture(pawnRect, Textures.BgTexGrey);
            GUI.color = color;

            GUI.color = color;
            GUI.Label(pawnRect, groupCount + " in group");
            outerRect = pawnRect;

            GUI.color = Color.white;
        }

        public void DrawGroupFrame(int group)
        {
            Rect position = this.GroupFrameRect(group);
            List<EntryKF> entries = ColonistBar_KF.BarHelperKf.Entries;
            Map map = entries.Find(x => x.group == group).map;
            float num;
            Color color = new Color(0.5f, 0.5f, 0.5f, 0.4f);
            //  Color color = new Color(0.23f, 0.23f, 0.23f, 0.4f);

            bool flag = Mouse.IsOver(position);

            // Caravan on world map
            if (map == null)
            {
                if (WorldRendererUtility.WorldRenderedNow)
                {
                    num = 1f;
                }
                else
                {
                    num = 0.75f;
                }

                if (Settings.barSettings.UseGroupColors)
                {
                    color = new Color(0.2f, 0.5f, 0.47f, 0.4f);
                }
            }
            else
            {
                // other pawns, on map
                if (map != Find.VisibleMap || WorldRendererUtility.WorldRenderedNow)
                {
                    num = 0.75f;
                }
                else
                {
                    num = 1f;
                }

                if (Settings.barSettings.UseGroupColors && !map.IsPlayerHome)
                {
                    color = new Color(0.2f, 0.25f, 0.5f, 0.4f);
                }
            }

            if (flag && num < 1f)
            {
                num = 1f;
            }

            color.a *= num;
            Widgets.DrawRectFast(position, color);
            return;

            // Stupid stuff
            if (Mouse.IsOver(position) && Event.current.type == EventType.MouseDown)
            {
                List<Pawn> tmpColonists = new List<Pawn>();
                for (int i = 0; i < entries.Count; i++)
                {
                    Pawn pawn = entries[i].pawn;
                    if (pawn == null)
                    {
                        continue;
                    }
                    tmpColonists.Add(pawn);
                }
                if (tmpColonists.Count != 0)
                {
                    bool worldRenderedNow = WorldRendererUtility.WorldRenderedNow;
                    int num3 = -1;
                    int num2 = tmpColonists.Count - 1;
                    while (num2 >= 0)
                    {
                        if (!worldRenderedNow && Find.Selector.IsSelected(tmpColonists[num2]))
                        {
                            goto IL_00c4;
                        }
                        if (worldRenderedNow && tmpColonists[num2].IsCaravanMember()
                            && Find.WorldSelector.IsSelected(tmpColonists[num2].GetCaravan()))
                        {
                            goto IL_00c4;
                        }
                        num2--;
                        continue;

                        IL_00c4:
                        num3 = num2;
                        break;
                    }
                    if (num3 == -1)
                    {
                        CameraJumper.TryJumpAndSelect(tmpColonists[0]);
                    }
                    else
                    {
                        CameraJumper.TryJumpAndSelect(tmpColonists[(num3 + 1) % tmpColonists.Count]);
                    }
                }
            }
        }

        public void HandleClicks(Rect rect, [CanBeNull] Pawn colonist, int showThisMap)
        {
            if (Mouse.IsOver(rect) && Event.current.type == EventType.MouseDown)
            {
                switch (Event.current.button)
                {
                    // Left Mouse Button
                    case 0:
                        {
                            if (Event.current.clickCount == 1)
                            {
                                // Single click on "more colonists"
                                if (colonist == null)
                                {
                                    // use event so it doesn't bubble through
                                    Event.current.Use();
                                    ColonistBar_KF.BarHelperKf.displayGroupForBar = showThisMap;
                                    HarmonyPatches.MarkColonistsDirty_Postfix();
                                }
                            }

                            if (Event.current.clickCount == 2)
                            {
                                // Double click
                                // use event so it doesn't bubble through
                                Event.current.Use();
                                bool flag = false;
                                if (colonist == null)
                                {
                                }
                                else
                                {
                                    if (FollowMe.CurrentlyFollowing)
                                    {
                                        FollowMe.StopFollow("Selected another colonist on bar");
                                        if (colonist?.Map != null)
                                        {
                                            FollowMe.TryStartFollow(colonist);
                                        }
                                        else
                                        {
                                            flag = true;
                                        }
                                    }
                                    else
                                    {
                                        flag = true;
                                    }

                                    if (flag)
                                    {
                                        CameraJumper.TryJump(colonist);
                                    }
                                }

                                // clickedColonist = null;
                            }

                            // if (Event.current.clickCount == 1)
                            // {
                            // clickedColonist = colonist;
                            // }
                            break;
                        }

                    // Right Mouse Button
                    case 1:
                        List<FloatMenuOption> choicesList = new List<FloatMenuOption>();
                        List<FloatMenuOption> fluffyStart = new List<FloatMenuOption>();
                        List<FloatMenuOption> fluffyStop = new List<FloatMenuOption>();

                        if (colonist != null && SelPawn != null && SelPawn != colonist && SelPawn.Map != null
                            && colonist.Map == SelPawn.Map && SelPawn.IsColonistPlayerControlled)
                        {
                            List<FloatMenuOption> fmoptions = FloatMenuMakerMap.ChoicesAtFor(colonist.TrueCenter(), SelPawn);
                            for (int i = 0; i < fmoptions.Count; i++)
                            {
                                FloatMenuOption choice = fmoptions[i];
                                choicesList.Add(choice);

                                // floatOptionList.Add(choice);
                            }
                        }

                        if (colonist?.Map != null)
                        {
                            FloatMenuOption fluffyStopAction;

                            FloatMenuOption fluffyFollowAction = new FloatMenuOption(
                                "FollowMe.StartFollow".Translate() + " - " + colonist.LabelShort,
                                delegate { FollowMe.TryStartFollow(colonist); });

                            bool flag = !FollowMe.CurrentlyFollowing
                                        || FollowMe.CurrentlyFollowing && FollowMe._followedThing != colonist;
                            if (flag)
                            {
                                fluffyStart.Add(fluffyFollowAction);

                                // foreach (var pawn in colonist.Map.mapPawns.FreeColonistsSpawned.OrderBy(pawn => pawn.LabelShort))
                                // {
                                // if (pawn != colonist)
                                // {
                                // FloatMenuOption fluffyFollowAlsoAction = new FloatMenuOption(
                                // "FollowMe.StartFollow".Translate() + " - " + pawn.LabelShort,
                                // delegate { FollowMe.TryStartFollow(pawn); });
                                // fluffyStart.Add(fluffyFollowAlsoAction);
                                // }
                                // }
                            }

                            if (FollowMe.CurrentlyFollowing)
                            {
                                fluffyStopAction = new FloatMenuOption(
                                    "FollowMe.StopFollow".Translate() + " - " + FollowMe._followedThing.LabelShort,
                                    delegate { FollowMe.StopFollow("Canceled in dropdown"); });

                                fluffyStop.Add(fluffyStopAction);
                            }
                        }

                        this.GetSortList(out List<FloatMenuOption> sortList);
                        sortList.Reverse();

                        // this.GetSortExtraList(out List<FloatMenuOption> extraSortList);
                        Dictionary<string, List<FloatMenuOption>> labeledSortingActions =
                            new Dictionary<string, List<FloatMenuOption>>();

                        FloatMenuOption options = new FloatMenuOption(
                            "CBKF.Settings.SettingsColonistBar".Translate(),
                            delegate { Find.WindowStack.Add(new ColonistBarKfSettings()); });

                        List<FloatMenuOption> floatOptionList = new List<FloatMenuOption> { options };

                        if (!fluffyStart.NullOrEmpty())
                        {
                            labeledSortingActions.Add(fluffyStart[0].Label, fluffyStart);
                        }

                        if (!fluffyStop.NullOrEmpty())
                        {
                            labeledSortingActions.Add(fluffyStop[0].Label, fluffyStop);
                        }

                        if (!choicesList.NullOrEmpty())
                        {
                            labeledSortingActions.Add(
                                "CBKF.Settings.ChoicesForPawn".Translate(SelPawn, colonist) + Tools.NestedString,
                                choicesList);
                        }

                        labeledSortingActions.Add("CBKF.Settings.OrderingOptions".Translate() + Tools.NestedString, sortList);

                        labeledSortingActions.Add("CBKF.Settings.SettingsColonistBar".Translate(), floatOptionList);

                        List<FloatMenuOption> items = labeledSortingActions.Keys.Select(
                            groupContent =>
                                {
                                    List<FloatMenuOption> fmo = labeledSortingActions[groupContent];

                                    return Tools.MakeMenuItemForLabel(groupContent, fmo);
                                }).ToList();

                        Tools.LabelMenu = new FloatMenuLabels(items);
                        Find.WindowStack.Add(Tools.LabelMenu);

                        // use event so it doesn't bubble through
                        Event.current.Use();
                        break;
                }

                // Middle Mouse Button
                if (Event.current.type == EventType.mouseUp && Event.current.button == 2)
                {
                    // start following
                    if (FollowMe.CurrentlyFollowing)
                    {
                        FollowMe.StopFollow("Canceled by user");
                    }
                    else
                    {
                        FollowMe.TryStartFollow(colonist);
                    }

                    // use event so it doesn't bubble through
                    Event.current.Use();
                }
            }
        }

        // RimWorld.ColonistBarColonistDrawer
        public void HandleGroupFrameClicks(int group)
        {
            Rect rect = this.GroupFrameRect(group);

            // Using Mouse Down instead of Up to not interfere with HandleClicks
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Mouse.IsOver(rect)
                && Event.current.clickCount == 1)
            {
                bool worldRenderedNow = WorldRendererUtility.WorldRenderedNow;
                EntryKF entry = ColonistBar_KF.BarHelperKf.Entries.Find(x => x.group == group);
                Map map = entry.map;

                if (!ColonistBar_KF.BarHelperKf.AnyBarEntryAt(UI.MousePositionOnUIInverted))
                {
                    if (!worldRenderedNow && !Find.Selector.dragBox.IsValidAndActive
                        || worldRenderedNow && !Find.WorldSelector.dragBox.IsValidAndActive)
                    {
                        Find.Selector.dragBox.active = false;
                        Find.WorldSelector.dragBox.active = false;
                        if (map == null)
                        {
                            if (worldRenderedNow)
                            {
                                CameraJumper.TrySelect(entry.pawn);
                            }
                            else
                            {
                                CameraJumper.TryJumpAndSelect(entry.pawn);
                            }
                        }
                        else
                        {
                            if (!CameraJumper.TryHideWorld() && Current.Game.VisibleMap != map)
                            {
                                SoundDefOf.MapSelected.PlayOneShotOnCamera();
                            }

                            Current.Game.VisibleMap = map;
                        }
                    }
                }
            }

            // RMB vanilla - not wanted

            // if (Event.current.button == 1 && Widgets.ButtonInvisible(rect, false))
            // {
            // ColonistBar.Entry entry2 = ColonistBar_KF.BarHelperKf.Entries.Find((ColonistBar.Entry x) => x.group == group);
            // if (entry2.map != null)
            // {
            // CameraJumper.TryJumpAndSelect(CameraJumper.GetWorldTargetOfMap(entry2.map));
            // }
            // else if (entry2.pawn != null)
            // {
            // CameraJumper.TryJumpAndSelect(entry2.pawn);
            // }
            // }
        }

        public void Notify_RecachedEntries()
        {
            this.pawnLabelsCache.Clear();
        }

        private static void BuildRects(
            int thisColCount,
            ref Rect outerRect,
            ref Rect pawnRect,
            out Rect moodRect,
            out Rect psiRect)
        {
            float widthMoodFloat = pawnRect.width;
            float heightMoodFloat = pawnRect.height;

            float modifier = 1;

            bool psiHorizontal = Settings.barSettings.ColBarPsiIconPos == Position.Alignment.Left
                                 || Settings.barSettings.ColBarPsiIconPos == Position.Alignment.Right;

            bool moodHorizontal = Settings.barSettings.MoodBarPos == Position.Alignment.Left
                                  || Settings.barSettings.MoodBarPos == Position.Alignment.Right;

            float widthPsiFloat;
            float heightPsiFloat;
            float heightFullPsiFloat;

            if (psiHorizontal)
            {
                widthPsiFloat = ColonistBar_KF.WidthPSIHorizontal * ColonistBar_KF.Scale;
                heightPsiFloat = outerRect.height - ColonistBar_KF.SpacingLabel;
                heightFullPsiFloat = outerRect.height - ColonistBar_KF.SpacingLabel;
            }
            else
            {
                widthPsiFloat = outerRect.width;
                heightPsiFloat = ColonistBar_KF.HeightPSIVertical * ColonistBar_KF.Scale;
                heightFullPsiFloat = ColonistBar_KF.HeightPSIVertical * ColonistBar_KF.Scale;
            }

            if (Settings.barSettings.UsePsi)
            {
                // If lesser rows, move the rect
                if (thisColCount < ColonistBar_KF.PsiRowsOnBar)
                {
                    CalculateSizePSI(thisColCount, modifier, psiHorizontal, ref widthPsiFloat, ref heightPsiFloat);
                }
            }

            if (Settings.barSettings.UseExternalMoodBar)
            {
                if (moodHorizontal)
                {
                    widthMoodFloat /= 4;
                }
                else
                {
                    heightMoodFloat /= 4;
                }
            }

            psiRect = new Rect(outerRect.x, outerRect.y, widthPsiFloat, heightPsiFloat);

            // Widgets.DrawBoxSolid(psiRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            switch (Settings.barSettings.ColBarPsiIconPos)
            {
                case Position.Alignment.Left:
                    pawnRect.x += widthPsiFloat;
                    break;

                case Position.Alignment.Right:
                    psiRect.x = pawnRect.xMax;
                    break;

                case Position.Alignment.Top:
                    pawnRect.y += heightFullPsiFloat;
                    psiRect.y += heightFullPsiFloat - heightPsiFloat;
                    break;

                case Position.Alignment.Bottom:
                    psiRect.y = pawnRect.yMax + ColonistBar_KF.SpacingLabel;
                    break;

                default: throw new ArgumentOutOfRangeException();
            }

            moodRect = new Rect(pawnRect.x, pawnRect.y, widthMoodFloat, heightMoodFloat);

            if (Settings.barSettings.UseExternalMoodBar)
            {
                switch (Settings.barSettings.MoodBarPos)
                {
                    case Position.Alignment.Left:
                        pawnRect.x += widthMoodFloat;
                        if (Settings.barSettings.ColBarPsiIconPos != Position.Alignment.Left)
                        {
                            psiRect.x += widthMoodFloat;
                        }

                        if (!psiHorizontal)
                        {
                            psiRect.width -= widthMoodFloat;
                        }

                        break;

                    case Position.Alignment.Right:
                        moodRect.x = pawnRect.xMax;
                        psiRect.x += Settings.barSettings.ColBarPsiIconPos == Position.Alignment.Right
                                         ? widthMoodFloat
                                         : 0f;
                        break;

                    case Position.Alignment.Top:
                        pawnRect.y += heightMoodFloat;
                        psiRect.y += Settings.barSettings.ColBarPsiIconPos == Position.Alignment.Bottom
                                         ? heightMoodFloat
                                         : 0f;
                        break;

                    case Position.Alignment.Bottom:
                        moodRect.y = pawnRect.yMax + ColonistBar_KF.SpacingLabel;
                        psiRect.y += Settings.barSettings.ColBarPsiIconPos == Position.Alignment.Bottom
                                         ? heightMoodFloat
                                         : 0f;
                        if (psiHorizontal)
                        {
                            psiRect.height -= heightMoodFloat;
                        }

                        break;

                    default: throw new ArgumentOutOfRangeException();
                }
            }

            float offsetX = outerRect.x - Mathf.Min(psiRect.x, moodRect.x, pawnRect.x);
            offsetX += outerRect.xMax - Mathf.Max(psiRect.xMax, moodRect.xMax, pawnRect.xMax);
            offsetX /= 2;

            float height = Mathf.Max(psiRect.yMax, moodRect.yMax, pawnRect.yMax);

            psiRect.x += offsetX;
            moodRect.x += offsetX;
            pawnRect.x += offsetX;

            outerRect.x += offsetX;
            outerRect.width -= offsetX * 2;
            outerRect.yMax =
                Settings.barSettings.ColBarPsiIconPos == Position.Alignment.Bottom
                || Settings.barSettings.MoodBarPos == Position.Alignment.Bottom
                    ? height
                    : height + ColonistBar_KF.SpacingLabel;
        }

        private static void CalculateSizePSI(
            int thisColCount,
            float modifier,
            bool psiHorizontal,
            ref float widthPsiFloat,
            ref float heightPsiFloat)
        {
            switch (thisColCount)
            {
                case 0:
                    modifier = 0f;
                    break;

                case 1:
                    modifier = 0.5f;
                    break;

                default: break;
            }

            if (psiHorizontal)
            {
                widthPsiFloat *= modifier;
            }
            else
            {
                heightPsiFloat *= modifier;
            }
        }

        private static void DrawCurrentJobTooltip([NotNull] Pawn colonist, Rect pawnRect)
        {
            string jobDescription = null;
            Lord lord = colonist.GetLord();
            if (lord?.LordJob != null)
            {
                jobDescription = lord.LordJob.GetReport();
            }

            if (colonist.jobs.curJob != null)
            {
                try
                {
                    string text2 = colonist.jobs.curDriver.GetReport().CapitalizeFirst();
                    if (!jobDescription.NullOrEmpty())
                    {
                        jobDescription = jobDescription + ": " + text2;
                    }
                    else
                    {
                        jobDescription = text2;
                    }
                }
                catch (Exception arg)
                {
                    Log.Message("JobDriver.GetReport() exception: " + arg);
                }
            }

            if (!jobDescription.NullOrEmpty())
            {
                TooltipHandler.TipRegion(pawnRect, jobDescription);
            }
        }

        private static void DrawCurrentMood(
            Rect moodRect,
            [NotNull] Texture2D moodTex,
            float moodPercent,
            float mood,
            out Rect rect1,
            out Rect rect2)
        {
            float x = moodRect.x + moodRect.width * mood;
            float y = moodRect.yMax - moodRect.height * mood;
            rect1 = new Rect(moodRect.x, y, moodRect.width, 1);
            rect2 = new Rect(moodRect.xMax + 1, y - 1, 2, 3);

            if (Settings.barSettings.UseExternalMoodBar)
            {
                switch (Settings.barSettings.MoodBarPos)
                {
                    default:
                        GUI.DrawTexture(moodRect.BottomPart(moodPercent), moodTex);
                        break;

                    case Position.Alignment.Top:
                        rect1 = new Rect(x, moodRect.y, 1, moodRect.height);
                        rect2 = new Rect(x - 1, moodRect.yMin - 1, 3, 2);
                        GUI.DrawTexture(moodRect.LeftPart(moodPercent), moodTex);
                        break;

                    case Position.Alignment.Bottom:
                        rect1 = new Rect(x, moodRect.y, 1, moodRect.height);
                        rect2 = new Rect(x - 1, moodRect.yMax + 1, 3, 2);
                        GUI.DrawTexture(moodRect.LeftPart(moodPercent), moodTex);
                        break;
                }
            }
            else
            {
                GUI.DrawTexture(moodRect.BottomPart(moodPercent), moodTex);
            }
        }

        private static void DrawMentalThreshold(Rect moodRect, float threshold)
        {
            if (Settings.barSettings.UseExternalMoodBar
                && (Settings.barSettings.MoodBarPos == Position.Alignment.Top
                    || Settings.barSettings.MoodBarPos == Position.Alignment.Bottom))
            {
                GUI.DrawTexture(
                    new Rect(moodRect.x + moodRect.width * threshold, moodRect.y, 1, moodRect.height),
                    Textures.MoodBreakTex);
            }
            else
            {
                GUI.DrawTexture(
                    new Rect(moodRect.x, moodRect.yMax - moodRect.height * threshold, moodRect.width, 1),
                    Textures.MoodBreakTex);
            }

            /*if (currentMood <= threshold)
            {
                GUI.DrawTexture(new Rect(moodRect.xMax-4, moodRect.yMax - moodRect.height * threshold, 8, 2), MoodBreakCrossedTex);
            }*/
        }

        private static void DrawNewMoodRect(Rect moodBorderRect, [NotNull] Need mood, [NotNull] MentalBreaker mb)
        {
            Rect moodRect = moodBorderRect.ContractedBy(2f);

            Color color = GUI.color;
            Color moodCol;

            Color critColor = Color.clear;
            bool showCritical = false;

            float moodPercent;
            float curMood = mood.CurLevelPercentage;

            GUI.DrawTexture(moodBorderRect, Textures.MoodBgTex);

            if (curMood > mb.BreakThresholdMinor)
            {
                moodPercent = Mathf.InverseLerp(mb.BreakThresholdMinor, 1f, curMood);
                moodCol = Textures.ColBlue;
                if (moodPercent < 0.3f)
                {
                    critColor = Color.Lerp(
                        Textures.ColorNeutralSoft,
                        Textures.ColorNeutralStatus,
                        Mathf.InverseLerp(0.3f, 0f, moodPercent));
                    critColor *= Textures.ColYellow;
                    showCritical = true;
                }
            }
            else if (curMood > mb.BreakThresholdMajor)
            {
                moodPercent = 1f - Mathf.InverseLerp(mb.BreakThresholdMajor, mb.BreakThresholdMinor, curMood);
                moodCol = Textures.ColYellow;
                if (moodPercent < 0.4f)
                {
                    critColor = Color.Lerp(
                        Textures.ColorNeutralSoft,
                        Textures.ColorNeutralStatus,
                        Mathf.InverseLerp(0.4f, 0f, moodPercent));
                    critColor *= Textures.ColOrange;
                    showCritical = true;
                }
            }
            else if (curMood > mb.BreakThresholdExtreme)
            {
                moodPercent = 1f - Mathf.InverseLerp(mb.BreakThresholdExtreme, mb.BreakThresholdMajor, curMood);
                moodCol = Textures.ColOrange;
                if (moodPercent < 0.5f)
                {
                    critColor = Color.Lerp(
                        Textures.ColorNeutralSoft,
                        Textures.ColorNeutralStatus,
                        Mathf.InverseLerp(0.5f, 0f, moodPercent));
                    critColor *= Textures.ColVermillion;
                    showCritical = true;
                }
            }
            else
            {
                // moodPercent = mb.BreakThresholdExtreme > 0.01f ? Mathf.InverseLerp(0f, mb.BreakThresholdExtreme, curMood) : 1f;
                moodPercent = 1f;
                moodCol = Textures.ColVermillion;
            }

            moodCol.a = color.a;

            GUI.color = moodCol;
            GUI.DrawTexture(moodRect, Textures.MoodNeutralBgTex);
            if (showCritical)
            {
                critColor.a *= color.a;
                GUI.color = critColor;
                GUI.DrawTexture(moodRect, Textures.MoodNeutralTex);
                GUI.color = moodCol;
            }
            float moodFloat = mood.CurInstantLevelPercentage;
            DrawCurrentMood(
                moodRect,
                Textures.MoodNeutralTex,
                moodPercent,
                moodFloat,
                out Rect rect1,
                out Rect rect2);
            GUI.color = color;

            DrawMentalThreshold(moodRect, mb.BreakThresholdExtreme);
            DrawMentalThreshold(moodRect, mb.BreakThresholdMajor);
            DrawMentalThreshold(moodRect, mb.BreakThresholdMinor);

            GUI.DrawTexture(rect1, Textures.MoodTargetTex);
            GUI.DrawTexture(rect2, Textures.MoodTargetTex);

            // TooltipHandler.TipRegion(moodRect, tooltip);
        }

        private void ApplyEntryInAnotherMapAlphaFactor([CanBeNull] Map map, Rect rect, ref float alpha)
        {
            bool flag = Mouse.IsOver(rect);

            if (map == null)
            {
                if (!WorldRendererUtility.WorldRenderedNow)
                {
                    alpha = Mathf.Min(alpha, flag ? 1f : 0.4f);
                }
            }
            else if (map != Find.VisibleMap || WorldRendererUtility.WorldRenderedNow)
            {
                alpha = Mathf.Min(alpha, flag ? 1f : 0.4f);
            }
        }

        private void DrawCaravanSelectionOverlayOnGUI([NotNull] Caravan caravan, Rect rect)
        {
            float num = 0.4f * ColonistBar_KF.Scale;
            float x = SelectionDrawerUtility.SelectedTexGUI.width * num;
            float y = SelectionDrawerUtility.SelectedTexGUI.height * num;
            Vector2 textureSize = new Vector2(x, y);
            SelectionDrawerUtility.CalculateSelectionBracketPositionsUI(
                bracketLocs,
                caravan,
                rect,
                WorldSelectionDrawer.SelectTimes,
                textureSize,
                Settings.barSettings.BaseIconSize * ColonistBar_KF.Scale);
            this.DrawSelectionOverlayOnGUI(bracketLocs, num);
        }

        private void DrawIcon([NotNull] Texture2D icon, ref Vector2 pos, [NotNull] string tooltip)
        {
            float num = Settings.barSettings.BaseIconSize * 0.4f * ColonistBar_KF.Scale;
            Rect rect = new Rect(pos.x, pos.y, num, num);
            GUI.DrawTexture(rect, icon);
            TooltipHandler.TipRegion(rect, tooltip);
            pos.x += num;
        }

        private void DrawIcons(Rect rect, [NotNull] Pawn colonist)
        {
            if (colonist.Dead)
            {
                return;
            }

            Vector2 vector = new Vector2(rect.x + 1f, rect.yMax - rect.width / 5 * 2 - 1f);
            bool attacking = false;
            if (colonist.CurJob != null)
            {
                JobDef def = colonist.CurJob.def;
                if (def == JobDefOf.AttackMelee || def == JobDefOf.AttackStatic)
                {
                    attacking = true;
                }
                else if (def == JobDefOf.WaitCombat)
                {
                    Stance_Busy stanceBusy = colonist.stances.curStance as Stance_Busy;
                    if (stanceBusy != null && stanceBusy.focusTarg.IsValid)
                    {
                        attacking = true;
                    }
                }
            }

            if (colonist.InAggroMentalState)
            {
                // DrawIcon(PSI.PSI.PSIMaterials[Icons.Aggressive].mainTexture as Texture2D, ref vector, colonist.MentalStateDef.LabelCap);
                this.DrawIcon(Textures.IconMentalStateAggro, ref vector, colonist.MentalStateDef.LabelCap);
            }
            else if (colonist.InMentalState)
            {
                // DrawIcon(PSI.PSI.PSIMaterials[Icons.Dazed].mainTexture as Texture2D, ref vector, colonist.MentalStateDef.LabelCap);
                this.DrawIcon(
                    Textures.IconMentalStateNonAggro,
                    ref vector,
                    colonist.MentalStateDef.LabelCap);
            }
            else if (colonist.InBed() && colonist.CurrentBed().Medical)
            {
                // DrawIcon(PSI.PSI.PSIMaterials[Icons.Health].mainTexture as Texture2D, ref vector, "ActivityIconMedicalRest".Translate());
                this.DrawIcon(Textures.IconMedicalRest, ref vector, "ActivityIconMedicalRest".Translate());
            }
            else if (colonist.CurJob != null && colonist.jobs.curDriver.asleep)
            {
                // DrawIcon(PSI.PSI.PSIMaterials[Icons.Tired].mainTexture as Texture2D, ref vector, "ActivityIconSleeping".Translate());
                this.DrawIcon(Textures.IconSleeping, ref vector, "ActivityIconSleeping".Translate());
            }
            else if (colonist.CurJob != null && colonist.CurJob.def == JobDefOf.FleeAndCower)
            {
                // DrawIcon(PSI.PSI.PSIMaterials[Icons.Leave].mainTexture as Texture2D, ref vector, "ActivityIconFleeing".Translate());
                this.DrawIcon(Textures.IconFleeing, ref vector, "ActivityIconFleeing".Translate());
            }
            else if (attacking)
            {
                this.DrawIcon(Textures.IconAttacking, ref vector, "ActivityIconAttacking".Translate());
            }
            else if (colonist.mindState.IsIdle && GenDate.DaysPassed >= 0.1)
            {
                // DrawIcon(PSI.PSI.PSIMaterials[Icons.Idle].mainTexture as Texture2D, ref vector, "ActivityIconIdle".Translate());
                this.DrawIcon(Textures.IconIdle, ref vector, "ActivityIconIdle".Translate());
            }

            if (colonist.IsBurning())
            {
                this.DrawIcon(Textures.IconBurning, ref vector, "ActivityIconBurning".Translate());
            }
        }

        private void DrawSelectionOverlayOnGUI([NotNull] Pawn colonist, Rect rect)
        {
            Thing obj = colonist;
            if (colonist.Dead)
            {
                obj = colonist.Corpse;
            }

            float num = 0.4f * ColonistBar_KF.Scale;
            Vector2 textureSize = new Vector2(
                SelectionDrawerUtility.SelectedTexGUI.width * num,
                SelectionDrawerUtility.SelectedTexGUI.height * num);

            SelectionDrawerUtility.CalculateSelectionBracketPositionsUI(
                bracketLocs,
                obj,
                rect,
                SelectionDrawer.SelectTimes,
                textureSize,
                Settings.barSettings.BaseIconSize * ColonistBar_KF.Scale);
            this.DrawSelectionOverlayOnGUI(bracketLocs, num);
        }

        private void DrawSelectionOverlayOnGUI([NotNull] Vector2[] bracketLocs, float selectedTexScale)
        {
            int num = 90;
            for (int i = 0; i < 4; i++)
            {
                Widgets.DrawTextureRotated(
                    bracketLocs[i],
                    SelectionDrawerUtility.SelectedTexGUI,
                    num,
                    selectedTexScale);
                num += 90;
            }
        }

        private void DrawWeaponIcon(Rect rect, float entryRectAlpha, [NotNull] Pawn colonist)
        {
            Color color = new Color(1f, 1f, 1f, entryRectAlpha);
            GUI.color = color;
            if (colonist.equipment.Primary != null)
            {
                ThingWithComps thing = colonist.equipment.Primary;
                Rect rect2 = rect.ContractedBy(rect.width / 3);

                rect2.x = rect.xMax - rect2.width - rect.width / 12;
                rect2.y = rect.yMax - rect2.height - rect.height / 12;

                GUI.color = color;
                if (!thing.def.uiIconPath.NullOrEmpty())
                {
                    Textures.ResolvedIcon = thing.def.uiIcon;
                }
                else
                {
                    Textures.ResolvedIcon =
                        thing.Graphic.ExtractInnerGraphicFor(thing).MatSingle.mainTexture as Texture2D;
                }

                Color weaponColor = new Color();

                // color labe by thing
                if (thing.def.IsMeleeWeapon)
                {
                    weaponColor = Textures.ColVermillion;
                    weaponColor.a = entryRectAlpha;
                    GUI.color = weaponColor;
                }

                if (thing.def.IsRangedWeapon)
                {
                    weaponColor = Textures.ColBlue;
                    weaponColor.a = entryRectAlpha;
                    GUI.color = weaponColor;
                }

                Color iconcolor = new Color(0.5f, 0.5f, 0.5f, 0.8f * entryRectAlpha);
                Widgets.DrawBoxSolid(rect2, iconcolor);
                Widgets.DrawBox(rect2);
                GUI.color = color;
                Rect rect3 = rect2.ContractedBy(rect2.width / 8);

                Widgets.DrawTextureRotated(rect3, Textures.ResolvedIcon, 0);

                // Not visible, deactivated
                // if (Mouse.IsOver(rect2))
                // {
                // GUI.color = HighlightColor;
                // GUI.DrawTexture(rect2, TexUI.HighlightTex);
                // }
                TooltipHandler.TipRegion(rect2, thing.def.LabelCap);
            }
        }

        private Rect GetPawnTextureRect(float x, float y)
        {
            Vector2 size = PawnTextureSize * ColonistBar_KF.Scale;

            return new Rect(x + 1f, y - (size.y - ColonistBar_KF.PawnSize.y) - 1f, size.x, size.y);
        }

        private void GetSortList([NotNull] out List<FloatMenuOption> sortList)
        {
            sortList = new List<FloatMenuOption>();

            FloatMenuOption sortByVanilla = new FloatMenuOption(
                "CBKF.Settings.Vanilla".Translate(),
                delegate
                    {
                        Settings.barSettings.SortBy = SettingsColonistBar.SortByWhat.vanilla;
                        HarmonyPatches.MarkColonistsDirty_Postfix();
                    });
            FloatMenuOption sortByWeapons = new FloatMenuOption(
                "CBKF.Settings.Weapons".Translate(),
                delegate
                    {
                        Settings.barSettings.SortBy = SettingsColonistBar.SortByWhat.weapons;
                        HarmonyPatches.MarkColonistsDirty_Postfix();
                    });
            FloatMenuOption sortByName = new FloatMenuOption(
                "CBKF.Settings.ByName".Translate(),
                delegate
                    {
                        Settings.barSettings.SortBy = SettingsColonistBar.SortByWhat.byName;
                        HarmonyPatches.MarkColonistsDirty_Postfix();
                    });

            FloatMenuOption sortbySexAge = new FloatMenuOption(
                "CBKF.Settings.SexAge".Translate(),
                delegate
                    {
                        Settings.barSettings.SortBy = SettingsColonistBar.SortByWhat.sexage;
                        HarmonyPatches.MarkColonistsDirty_Postfix();
                    });
            FloatMenuOption sortByMood = new FloatMenuOption(
                "CBKF.Settings.Mood".Translate(),
                delegate
                    {
                        Settings.barSettings.SortBy = SettingsColonistBar.SortByWhat.mood;
                        HarmonyPatches.MarkColonistsDirty_Postfix();

                        // CheckRecacheEntries();
                    });
            FloatMenuOption sortByHealth = new FloatMenuOption(
                // "CBKF.Settings.Health".Translate(),
                "TabHealth".Translate(),
                delegate
                    {
                        Settings.barSettings.SortBy = SettingsColonistBar.SortByWhat.health;
                        HarmonyPatches.MarkColonistsDirty_Postfix();
                    });

            FloatMenuOption sortByBleeding = new FloatMenuOption(
                "BleedingRate".Translate(),
                delegate
                    {
                        Settings.barSettings.SortBy = SettingsColonistBar.SortByWhat.bleedRate;
                        HarmonyPatches.MarkColonistsDirty_Postfix();
                    });

            FloatMenuOption sortByMedic = new FloatMenuOption(
                StatDefOf.MedicalTendQuality.LabelCap,
                delegate
                    {
                        Settings.barSettings.SortBy = SettingsColonistBar.SortByWhat.medicTendQuality;
                        HarmonyPatches.MarkColonistsDirty_Postfix();
                    });

            FloatMenuOption sortByMedic2 = new FloatMenuOption(
                StatDefOf.MedicalSurgerySuccessChance.LabelCap,
                delegate
                    {
                        Settings.barSettings.SortBy = SettingsColonistBar.SortByWhat.medicSurgerySuccess;
                        HarmonyPatches.MarkColonistsDirty_Postfix();
                    });

            FloatMenuOption sortByDiplomacy = new FloatMenuOption(
                StatDefOf.DiplomacyPower.LabelCap,
                delegate
                    {
                        Settings.barSettings.SortBy = SettingsColonistBar.SortByWhat.diplomacy;
                        HarmonyPatches.MarkColonistsDirty_Postfix();
                    });

            FloatMenuOption sortByTrade = new FloatMenuOption(
                StatDefOf.TradePriceImprovement.LabelCap,
                delegate
                    {
                        Settings.barSettings.SortBy = SettingsColonistBar.SortByWhat.tradePrice;
                        HarmonyPatches.MarkColonistsDirty_Postfix();
                    });

            sortList.Add(sortByVanilla);
            sortList.Add(sortByWeapons);
            sortList.Add(sortByName);
            sortList.Add(sortByMood);
            sortList.Add(sortbySexAge);
            sortList.Add(sortByHealth);
            //    if (Find.WorldPawns.AllPawnsAlive.Any(x => x.IsColonist && x.health.hediffSet.BleedRateTotal > 0.01f))
            {
            }
            sortList.Add(sortByBleeding);
            sortList.Add(sortByMedic);
            sortList.Add(sortByMedic2);
            sortList.Add(sortByDiplomacy);
            sortList.Add(sortByTrade);
        }

        // RimWorld.ColonistBarColonistDrawer
        private Rect GroupFrameRect(int group)
        {
            float posX = 99999f;
            float posY = 21f;
            float num2 = 0f;
            float height = 0f;
            List<EntryKF> entries = ColonistBar_KF.BarHelperKf.Entries;
            List<Vector2> drawLocs = ColonistBar_KF.BarHelperKf.DrawLocs;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].group == group)
                {
                    posX = Mathf.Min(posX, drawLocs[i].x);
                    num2 = Mathf.Max(num2, drawLocs[i].x + ColonistBar_KF.FullSize.x);
                    height = Mathf.Max(height, drawLocs[i].y + ColonistBar_KF.FullSize.y);
                }
            }

            if (Settings.barSettings.UseCustomMarginTop)
            {
                posY = Settings.barSettings.MarginTop;
                height -= Settings.barSettings.MarginTop;
            }

            height += ColonistBar_KF.SpacingLabel;

            return new Rect(posX, posY, num2 - posX, height).ContractedBy(-12f * ColonistBar_KF.Scale);
        }
    }
}
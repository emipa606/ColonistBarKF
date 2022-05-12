using JetBrains.Annotations;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using static ColonistBarKF.PSI.GameComponentPSI;

namespace ColonistBarKF.PSI;

public static class PSIDrawer
{
    [NotNull] public static Vector3[] IconPosVectorsPSI;

    public static void RecalcIconPositionsPSI()
    {
        var psiSettings = Settings.PSISettings;

        // _iconPosVectors = new Vector3[18];
        IconPosVectorsPSI = new Vector3[40];
        for (var index = 0; index < IconPosVectorsPSI.Length; ++index)
        {
            var num1 = index / psiSettings.IconsInColumn;
            var num2 = index % psiSettings.IconsInColumn;
            if (psiSettings.IconsHorizontal)
            {
                var num3 = num1;
                num1 = num2;
                num2 = num3;
            }

            var y = AltitudeLayer.MetaOverlays.AltitudeFor();

            IconPosVectorsPSI[index] = new Vector3(
                (float)((-0.600000023841858 * psiSettings.IconMarginX) - (0.550000011920929 * psiSettings.IconSize
                    * psiSettings.IconOffsetX * num1)),
                y,
                (float)((-0.600000023841858 * psiSettings.IconMarginY) + (0.550000011920929 * psiSettings.IconSize
                    * psiSettings.IconOffsetY * num2)));
        }
    }

    public static void DrawIcon_posOffset(
        Vector3 bodyPos,
        Vector3 posOffset,
        [NotNull] Material material,
        Color color,
        float opacity)
    {
        color.a = opacity;
        material.color = color;
        var guiColor = GUI.color;
        GUI.color = color;
        Vector2 vectorAtBody;

        var worldScale = WorldScale;
        if (Settings.PSISettings.IconsScreenScale)
        {
            worldScale = 45f;
            vectorAtBody = bodyPos.MapToUIPosition();
            vectorAtBody.x += posOffset.x * 45f;
            vectorAtBody.y -= posOffset.z * 45f;
        }
        else
        {
            vectorAtBody = (bodyPos + posOffset).MapToUIPosition();
        }

        var num2 = worldScale * (Settings.PSISettings.IconSizeMult * 0.5f);

        // On Colonist
        var position = new Rect(
            vectorAtBody.x,
            vectorAtBody.y,
            num2 * Settings.PSISettings.IconSize,
            num2 * Settings.PSISettings.IconSize);
        position.x -= position.width * 0.5f;
        position.y -= position.height * 0.5f;

        GUI.DrawTexture(position, material.mainTexture, ScaleMode.ScaleToFit, true);
        GUI.color = guiColor;
    }


    public static void DrawIconOnColonist(Vector3 bodyPos, IconEntryPSI entryPSI, int entryCount)
    {
        if (WorldRendererUtility.WorldRenderedNow)
        {
            return;
        }

        var material = PSIMaterials[entryPSI.Icon];
        if (material == null)
        {
            Debug.LogError("Material = null.");
            return;
        }

        var posOffset = IconPosVectorsPSI[entryCount];

        entryPSI.Color.a = entryPSI.Opacity;
        material.color = entryPSI.Color;
        var guiColor = GUI.color;
        GUI.color = entryPSI.Color;
        Vector2 vectorAtBody;

        var worldScale = WorldScale;
        if (Settings.PSISettings.IconsScreenScale)
        {
            worldScale = 45f;
            vectorAtBody = bodyPos.MapToUIPosition();
            vectorAtBody.x += posOffset.x * 45f;
            vectorAtBody.y -= posOffset.z * 45f;
        }
        else
        {
            vectorAtBody = (bodyPos + posOffset).MapToUIPosition();
        }

        var num2 = worldScale * (Settings.PSISettings.IconSizeMult * 0.5f);

        // On Colonist
        var position = new Rect(
            vectorAtBody.x,
            vectorAtBody.y,
            num2 * Settings.PSISettings.IconSize,
            num2 * Settings.PSISettings.IconSize);
        position.x -= position.width * 0.5f;
        position.y -= position.height * 0.5f;

        GUI.DrawTexture(position, material.mainTexture, ScaleMode.ScaleToFit, true);
        GUI.color = guiColor;
    }

    public static void DrawIconOnColonist(Vector3 bodyPos, ref int num, Icon icon, Color color, float opacity)
    {
        if (WorldRendererUtility.WorldRenderedNow)
        {
            return;
        }

        var material = PSIMaterials[icon];
        if (material == null)
        {
            // Debug.LogError("Material = null.");
            return;
        }

        DrawIcon_posOffset(bodyPos, IconPosVectorsPSI[num], material, color, opacity);
        num++;
    }
}
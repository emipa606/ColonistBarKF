﻿using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace ColonistBarKF;

public class Materials(string matLib = "default")
{
    [NotNull] private readonly Material[] _data = new Material[40];

    [NotNull] private readonly string _matLibName = matLib;

    [CanBeNull] public Material this[Icon icon] => _data[(int)icon];

    public void ReloadTextures(bool smooth = false)
    {
        foreach (var icons in Enum.GetValues(typeof(Icon)).Cast<Icon>())
        {
            switch (icons)
            {
                case Icon.None:
                case Icon.Length:
                    continue;
                default:
                    var path = $"{_matLibName}/{Enum.GetName(typeof(Icon), icons)}";
                    _data[(int)icons] = LoadIconMat(path, smooth);
                    continue;
            }
        }
    }

    [CanBeNull]
    private Material LoadIconMat(string path, bool smooth = false)
    {
        var tex = ContentFinder<Texture2D>.Get($"UI/Overlays/PawnStateIcons/{path}", false);

        Material material;
        if (tex == null)
        {
            material = null;
        }
        else
        {
            if (smooth)
            {
                tex.filterMode = FilterMode.Trilinear;
                tex.mipMapBias = -0.5f;
                tex.anisoLevel = 9;
                tex.wrapMode = TextureWrapMode.Repeat;

                // tex.Apply();
                // tex.Compress(true);
            }
            else
            {
                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Repeat;

                // tex.Apply();
                // tex.Compress(true);
            }

            material = MaterialPool.MatFrom(new MaterialRequest(tex, ShaderDatabase.MetaOverlay));
        }

        return material;
    }
}
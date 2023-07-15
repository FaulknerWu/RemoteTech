﻿using HugsLib.Utils;
using Verse;

namespace RemoteTech;

public class CustomFactionGoodwillCaps : UtilityWorldObject
{
    public override void ExposeData()
    {
        base.ExposeData();
        if (Scribe.mode == LoadSaveMode.PostLoadInit && !Destroyed)
        {
            Destroy();
        }
    }
#pragma warning restore 618
}
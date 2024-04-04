using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RemoteTech;

/// <summary>
///     An overlay for explosives and conductors on the wired detonation grid.
///     Displays only when a building designator for a building with a relevant comp is selected.
/// </summary>
public class SectionLayer_DetonationGrid : SectionLayer_Things
{
    private static readonly Type GridNodeCompType = typeof(CompDetonationGridNode);
    private bool cachedVisible;

    private int lastCachedFrame;

    public SectionLayer_DetonationGrid(Section section) : base(section)
    {
        relevantChangeTypes = MapMeshFlagDefOf.Buildings;
    }

    public override void TakePrintFrom(Thing t)
    {
        if (t is not ThingWithComps compThing)
        {
            return;
        }

        foreach (var thingComp in compThing.AllComps)
        {
            var comp = thingComp as CompDetonationGridNode;

            comp?.PrintForDetonationGrid(this);
        }
    }

    public override void DrawLayer()
    {
        // perform check only once per frame, cache result for other visible sections
        if (Time.frameCount > lastCachedFrame)
        {
            cachedVisible = false;
            var selectedDesignator = Find.DesignatorManager.SelectedDesignator;
            var buildingDef = selectedDesignator is not Designator_Build designatorBuild
                ? null
                : designatorBuild.PlacingDef as ThingDef;
            cachedVisible = buildingDef != null && DefHasGridComp(buildingDef) ||
                            selectedDesignator is Designator_SelectDetonatorWire;
            lastCachedFrame = Time.frameCount;
        }

        if (!cachedVisible)
        {
            return;
        }

        base.DrawLayer();
    }

    private bool DefHasGridComp(ThingDef def)
    {
        var comps = def.comps;
        if (comps == null)
        {
            return false;
        }

        foreach (var compProps in comps)
        {
            if (compProps?.compClass != null && compProps.compClass.IsSubclassOf(GridNodeCompType))
            {
                return true;
            }
        }

        return false;
    }
}
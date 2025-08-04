﻿using RimWorld;
using Verse;

namespace RemoteTech;

/// <summary>
///     Allows a non-linked graphic to be connected to by other linked graphics.
///     Only LinkFlags need to be set on the graphicData.
/// </summary>
public class CompLinkGridMember : ThingComp
{
    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        UpdateLinkGrid(parent.Map);
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map, mode);
        UpdateLinkGrid(map);
    }

    private void UpdateLinkGrid(Map map)
    {
        map.linkGrid.Notify_LinkerCreatedOrDestroyed(parent);
        map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things, true, false);
    }
}
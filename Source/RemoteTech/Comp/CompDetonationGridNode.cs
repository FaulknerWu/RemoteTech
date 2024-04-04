using RimWorld;
using Verse;

namespace RemoteTech;

/// <summary>
///     Base class for comps involved in the detonation grid. Required to print the overlay section layer.
/// </summary>
public abstract class CompDetonationGridNode : ThingComp
{
    private IntVec3 cachedPosition = IntVec3.Invalid;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (parent is not Building)
        {
            return;
        }

        cachedPosition = parent.Position;
        parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Buildings);
    }

    public override void PostDeSpawn(Map map)
    {
        base.PostDeSpawn(map);
        if (cachedPosition.IsValid)
        {
            map.mapDrawer.MapMeshDirty(cachedPosition, MapMeshFlagDefOf.Buildings);
        }
    }

    public abstract void PrintForDetonationGrid(SectionLayer layer);

    protected void PrintConnection(SectionLayer layer)
    {
        Resources.Graphics.DetWireOverlayAtlas.Print(layer, parent, 0f);
    }

    protected void PrintEndpoint(SectionLayer layer)
    {
        Resources.Graphics.DetWireOverlayEndpoint.Print(layer, parent, 0f);
    }
}
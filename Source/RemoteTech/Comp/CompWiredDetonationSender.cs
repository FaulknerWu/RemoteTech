using System;
using Verse;

namespace RemoteTech;

/// <summary>
///     Transmits a new detonation signal to CompWiredDetonationTransmitter comps on the same tile.
/// </summary>
public class CompWiredDetonationSender : CompDetonationGridNode
{
    public void SendNewSignal()
    {
        if (parent.Map == null)
        {
            throw new Exception("null map");
        }

        var thingsOnTile = parent.Map.thingGrid.ThingsListAtFast(parent.Position);
        foreach (var thing in thingsOnTile)
        {
            var comp = thing.TryGetComp<CompWiredDetonationTransmitter>();
            comp?.ReceiveSignal(Rand.Int, 0);
        }
    }

    public override void PrintForDetonationGrid(SectionLayer layer)
    {
        PrintEndpoint(layer);
    }
}
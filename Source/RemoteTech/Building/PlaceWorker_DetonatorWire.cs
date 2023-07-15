using System;
using Verse;

namespace RemoteTech;

/// <summary>
///     Allows detonator wire to be placed under existing structures
/// </summary>
public class PlaceWorker_DetonatorWire : PlaceWorker
{
    private readonly Type compTypeCrossing = typeof(CompWiredDetonationCrossing);
    private readonly Type compTypeTransmitter = typeof(CompWiredDetonationTransmitter);

    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        var thingList = loc.GetThingList(map);
        foreach (var tile in thingList)
        {
            if (tile.def == null)
            {
                return false;
            }

            if (tile.def.HasComp(compTypeTransmitter) || tile.def.HasComp(compTypeCrossing))
            {
                return false;
            }

            if (tile.def.entityDefToBuild is ThingDef thingDef && (thingDef.HasComp(compTypeTransmitter) ||
                                                                   tile.def
                                                                       .HasComp(compTypeCrossing)))
            {
                return false;
            }
        }

        return true;
    }
}
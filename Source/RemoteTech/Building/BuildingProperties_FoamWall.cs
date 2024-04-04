using System.Collections.Generic;
using RimWorld;
using Verse;
// ReSharper disable CollectionNeverUpdated.Global

namespace RemoteTech;

public class BuildingProperties_FoamWall : BuildingProperties
{
    public readonly List<ThingDef> smoothVariants = [];
    public int smoothWorkAmount;
}
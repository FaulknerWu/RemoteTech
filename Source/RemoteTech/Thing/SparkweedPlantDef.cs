// ReSharper disable UnassignedField.Global, CollectionNeverUpdated.Global

using System.Collections.Generic;
using Verse;

namespace RemoteTech;

public class SparkweedPlantDef : ThingDef
{
    public readonly int detectEveryTicks = 60;
    public readonly float ignitePawnChance = .2f;
    public readonly float ignitePlantChance = .5f;
    public readonly List<ThingDef> ignitionSuppressorThings = [];
    public readonly float minimumIgnitePlantGrowth = .2f;
    public EffecterDef igniteEffecter;
}
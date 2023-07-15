// ReSharper disable UnassignedField.Global

using RimWorld;
using Verse;

namespace RemoteTech;

public class BuildingProperties_FoamBlob : BuildingProperties
{
    public ThingDef hardenedDef;
    public IntRange ticksBetweenSpreading;
    public IntRange ticksToHarden;
}
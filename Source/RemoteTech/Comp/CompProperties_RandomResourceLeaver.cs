// ReSharper disable UnassignedField.Global

using Verse;

namespace RemoteTech;

public class CompProperties_RandomResourceLeaver : CompProperties
{
    public readonly DestroyMode requiredDestroyMode = DestroyMode.KillFinalize;
    public IntRange amountRange;
    public ThingDef thingDef;

    public CompProperties_RandomResourceLeaver()
    {
        compClass = typeof(CompRandomResourceLeaver);
    }
}
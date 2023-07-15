// ReSharper disable UnassignedField.Global

using RimWorld;
using Verse;

namespace RemoteTech;

public class BuildingProperties_DetonatorWire : BuildingProperties
{
    public readonly float baseDryingTemperature = 20f;
    public readonly float daysToSelfDry = .8f;
    public readonly int dryOffJobDurationTicks = 60;
    public readonly float failureChanceWhenFullyWet = 0.05f;
    public EffecterDef failureEffecter;
    public bool fireOnFailure = true;
}
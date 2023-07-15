// ReSharper disable UnassignedField.Global

using Verse;

namespace RemoteTech;

public class HediffCompProps_SeverityRecovery : HediffCompProperties
{
    public int cooldownAfterSeverityIncrease;
    public float severityIncreaseDetectionThreshold;
    public FloatRange severityRecoveryPerTick;
}
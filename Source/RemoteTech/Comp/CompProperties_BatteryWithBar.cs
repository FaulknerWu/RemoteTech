using RimWorld;
using UnityEngine;

namespace RemoteTech;

public class CompProperties_BatteryWithBar : CompProperties_Battery
{
    public readonly float barMargin = 0.15f;
    public readonly float passiveDischargeWatts = 0f;
    public Vector3 barOffset = new(0f, .1f, 0f);
    public Vector2 barSize = new(1.3f, 0.4f);

    public CompProperties_BatteryWithBar()
    {
        compClass = typeof(CompStatBattery);
    }
}
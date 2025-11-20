using RimWorld;
using UnityEngine;
using Verse;

namespace RemoteTech;

/// <summary>
///     This guy only blinks his overlay-the actual wireless routing is done using a comp.
/// </summary>
/// <see cref="CompWirelessDetonationGridNode" />
public class Building_RadioMast : Building
{
    private const int FlareAlphaLevels = 16;

    private CompPowerTrader compPower;

    private GraphicData_Blinker BlinkerData => Graphic.data as GraphicData_Blinker;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (BlinkerData == null)
        {
            Log.Error(
                $"{nameof(Building_RadioMast)} needs {nameof(GraphicData_Blinker)} in def {def.defName}");
        }

        compPower = GetComp<CompPowerTrader>();
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        if (compPower is not { PowerOn: true })
        {
            return;
        }

        // limit the number of possible alpha levels to avoid leaking materials
        var props = BlinkerData;
        var alpha = Mathf.Round(Mathf.Max(0f,
            Mathf.Sin((Find.TickManager.TicksGame + (thingIDNumber * 1000)) * Mathf.PI /
                      Mathf.Max(.1f, props.blinkerIntervalNormal))) * FlareAlphaLevels) / FlareAlphaLevels;
        if (alpha > 0)
        {
            RemoteTechUtility.DrawFlareOverlay(Resources.Graphics.FlareOverlayGreen, DrawPos, props, alpha);
        }
    }
}
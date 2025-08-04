using UnityEngine;
using Verse;

namespace RemoteTech;

/// <summary>
///     Stat-based power consumption that will switch to Idle mode when the parent device is not in use.
///     By default, looks for pawns on the interaction cells, but can be called directly to report in-use status.
///     Requires Normal ticks.
/// </summary>
public class CompStatPowerIdle : CompStatPower, IPowerUseNotified
{
    private const string IdlePowerUpgradeReferenceId = "IdlePower";
    private const float IdlePowerConsumption = 10f;
    private const int InteractionCellPollIntervalTicks = 20;

    // saved
    private int _highPowerTicks;

    private bool hasUpgrade;

    // we can't override PowerOutput in ComPowerTrader, so we go the sneaky way and use SetUpPowerVars
    private int HighPowerTicksLeft
    {
        get => _highPowerTicks;
        set
        {
            var wasIdle = _highPowerTicks > 0;
            var isIdle = value > 0;
            _highPowerTicks = Mathf.Max(0, value);
            if (wasIdle != isIdle)
            {
                SetUpPowerVars();
            }
        }
    }

    protected override float PowerConsumption => IdlePowerMode ? IdlePowerConsumption : base.PowerConsumption;

    private bool IdlePowerMode => hasUpgrade && HighPowerTicksLeft == 0;

    private bool HasIdlePowerUpgrade => parent.IsUpgradeCompleted(IdlePowerUpgradeReferenceId);

    public void ReportPowerUse(float duration = 1f)
    {
        HighPowerTicksLeft = Mathf.Max(HighPowerTicksLeft, duration.SecondsToTicks());
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        hasUpgrade = HasIdlePowerUpgrade;
        this.RequireTicker(TickerType.Normal);
    }

    public override void CompTick()
    {
        if (HighPowerTicksLeft > 0)
        {
            HighPowerTicksLeft--;
        }

        if (!parent.def.hasInteractionCell || GenTicks.TicksGame % InteractionCellPollIntervalTicks != 0)
        {
            return;
        }

        var pawnInCell = parent.InteractionCell.GetFirstPawn(parent.Map);
        if (pawnInCell is { IsColonist: true })
        {
            ReportPowerUse(3f);
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref _highPowerTicks, "highPowerTicks");
    }

    public override void ReceiveCompSignal(string signal)
    {
        hasUpgrade = HasIdlePowerUpgrade;
        base.ReceiveCompSignal(signal);
    }
}
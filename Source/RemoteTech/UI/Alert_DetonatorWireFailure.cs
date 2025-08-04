using RimWorld;
using Verse;

namespace RemoteTech;

public class Alert_DetonatorWireFailure : Alert_Critical
{
    private const float AutoExpireInSeconds = 8f;

    private static Alert_DetonatorWireFailure instance;

    private int expireTick;
    private Fire wireFire;

    // ReSharper disable once MemberCanBePrivate.Global
    public Alert_DetonatorWireFailure()
    {
        instance = this;
    }

    public static Alert_DetonatorWireFailure Instance => instance ?? (instance = new Alert_DetonatorWireFailure());

    public void ReportFailure(Fire createdFire)
    {
        expireTick = (int)(Find.TickManager.TicksGame + (AutoExpireInSeconds * GenTicks.TicksPerRealSecond));
        wireFire = createdFire;
    }

    public override string GetLabel()
    {
        return "Alert_wireFailure_label".Translate();
    }

    public override TaggedString GetExplanation()
    {
        return "Alert_wireFailure_desc".Translate();
    }

    public override AlertReport GetReport()
    {
        var fireLive = wireFire is { Spawned: true };
        if (fireLive || expireTick > Find.TickManager.TicksGame)
        {
            return fireLive ? AlertReport.CulpritIs(wireFire) : AlertReport.Active;
        }

        return false;
    }
}
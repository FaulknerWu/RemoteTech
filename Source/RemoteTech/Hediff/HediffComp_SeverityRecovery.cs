using Verse;

namespace RemoteTech;

/// <summary>
///     A Hediff Comp that will gradually diminish the severity when it is not actively increasing.
/// </summary>
public class HediffComp_SeverityRecovery : HediffComp
{
    private static readonly string RecoveringStatusSuffix = "HediffRecovery_status_label".Translate();
    private int cooldownTicksLeft;
    private float lastSeenSeverity;

    private bool OffCooldown => cooldownTicksLeft <= 0;

    public override string CompLabelInBracketsExtra => OffCooldown ? RecoveringStatusSuffix : "";

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        if (props is HediffCompProps_SeverityRecovery recoProps)
        {
            if (cooldownTicksLeft > 0)
            {
                cooldownTicksLeft--;
            }

            if (parent.Severity > lastSeenSeverity + recoProps.severityIncreaseDetectionThreshold)
            {
                cooldownTicksLeft = recoProps.cooldownAfterSeverityIncrease;
            }

            if (OffCooldown)
            {
                parent.Severity -= recoProps.severityRecoveryPerTick.RandomInRange;
                if (parent.Severity < 0)
                {
                    parent.Severity = 0;
                }
            }
        }

        if (parent.Severity > parent.def.maxSeverity)
        {
            parent.Severity = parent.def.maxSeverity;
        }

        lastSeenSeverity = parent.Severity;
    }
}
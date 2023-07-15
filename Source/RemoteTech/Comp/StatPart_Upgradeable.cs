using System.Text;
using RimWorld;
using Verse;

namespace RemoteTech;

/// <summary>
///     Required for CompUpgrade to modify arbitrary stats.
///     This is automatically added to StatDefs that have been found to be used in a CompProperties_Upgrade.
/// </summary>
/// <see cref="RemoteTechController.InjectUpgradeableStatParts" />
public class StatPart_Upgradeable : StatPart
{
    public override void TransformValue(StatRequest req, ref float val)
    {
        if (req.Thing is not ThingWithComps tcomps)
        {
            return;
        }

        foreach (var comp in tcomps.AllComps)
        {
            if (comp is not CompUpgrade { Complete: true } upgrade)
            {
                continue;
            }

            var mod = upgrade.TryGetStatModifier(parentStat);
            if (mod != null)
            {
                val *= mod.value;
            }
        }
    }

    public override string ExplanationPart(StatRequest req)
    {
        StringBuilder builder = null;
        if (req.Thing is not ThingWithComps tcomps)
        {
            return null;
        }

        foreach (var comp in tcomps.AllComps)
        {
            if (comp is not CompUpgrade { Complete: true } upgrade)
            {
                continue;
            }

            var mod = upgrade.TryGetStatModifier(parentStat);
            if (mod == null)
            {
                continue;
            }

            if (builder == null)
            {
                builder = new StringBuilder("Upgrade_statModifierCategory".Translate());
                builder.AppendLine();
            }

            builder.Append("    ");
            builder.Append(upgrade.Props.label.CapitalizeFirst());
            builder.Append(": ");
            builder.Append(mod.ToStringAsFactor);
        }

        return builder?.ToString();
    }
}
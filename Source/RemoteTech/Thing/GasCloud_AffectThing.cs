using RimWorld;
using UnityEngine;
using Verse;

namespace RemoteTech;

/// <summary>
///     To be used as a base for custom gases that affect things in the cell they are located in.
///     Effect strength is based on gas concentration.
///     Pawn effects scale based on the ToxicSensitivity stat, and MoteProperties_GasEffect
///     can specify apparel that will negate the effects.
/// </summary>
public abstract class GasCloud_AffectThing : GasCloud
{
    protected MoteProperties_GasEffect Props;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if ((Props = def.mote as MoteProperties_GasEffect) == null)
        {
            RemoteTechController.Instance.Logger.Error(
                $"{nameof(GasCloud_AffectThing)} needs {nameof(MoteProperties_GasEffect)} in def {def.defName}");
        }
    }

    protected override void GasTick()
    {
        base.GasTick();
        if (!Spawned || gasTicksProcessed % Props.effectInterval != 0)
        {
            return;
        }

        var thingsOnTile = Map.thingGrid.ThingsListAt(Position);
        foreach (var thing in thingsOnTile)
        {
            if (thing == this)
            {
                continue;
            }

            var multiplier = 0f;
            if (thing is Pawn { Dead: false } pawn && (Props.affectsDownedPawns || !pawn.Downed)
                                                   && (Props.affectsFleshy && pawn.def.race.IsFlesh ||
                                                       Props.affectsMechanical && pawn.RaceProps.IsMechanoid))
            {
                multiplier = 1 * GetImmunizingApparelMultiplier(pawn) * GetSensitivityStatMultiplier(pawn);
            }
            else if (Props.affectsPlants && thing is Plant || Props.affectsThings)
            {
                multiplier = 1f;
            }

            multiplier *= Mathf.Clamp01(Concentration / Props.FullAlphaConcentration);
            multiplier = Mathf.Clamp01(multiplier);
            if (multiplier > 0f)
            {
                ApplyGasEffect(thing, multiplier);
            }
        }
    }

    protected abstract void ApplyGasEffect(Thing thing, float strengthMultiplier);

    protected float GetSensitivityStatMultiplier(Pawn pawn)
    {
        if (Props.toxicSensitivityStatPower > 0f)
        {
            return 1f - (Mathf.Clamp01(pawn.GetStatValue(StatDefOf.ToxicEnvironmentResistance)) *
                         Props.toxicSensitivityStatPower);
        }

        return 1f;
    }

    protected float GetImmunizingApparelMultiplier(Pawn pawn)
    {
        if (!(Props.immunizingApparelPower > float.Epsilon) || pawn.apparel == null)
        {
            return 1f;
        }

        var apparel = pawn.apparel.WornApparel;
        foreach (var item in apparel)
        {
            if (Props.immunizingApparelDefs.Contains(item.def))
            {
                return 1f - Props.immunizingApparelPower;
            }
        }

        return 1f;
    }
}
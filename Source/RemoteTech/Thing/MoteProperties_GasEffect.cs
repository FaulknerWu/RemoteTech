// ReSharper disable UnassignedField.Global
// ReSharper disable CollectionNeverUpdated.Global

using System.Collections.Generic;
using Verse;

namespace RemoteTech;

/// <summary>
///     Provides settings to gases that affect things or pawns in some way
/// </summary>
public class MoteProperties_GasEffect : MoteProperties_GasCloud
{
    /// <summary>
    ///     Should downed pawns be affected in the same way as mobile ones?
    /// </summary>
    public readonly bool affectsDownedPawns = true;

    /// <summary>
    ///     GasCloud_DamageDealer: armor penetration multiplier of the applied damage
    /// </summary>
    public readonly float damageArmorPenetration = 1f;

    /// <summary>
    ///     GasCloud_DamageDealer: pawn body part tags targeted by damage. A random one will be picked from the list,
    ///     or random outside body parts will be picked when no parts specified.
    /// </summary>
    public readonly List<BodyPartTagDef> damageBodyPartTags = [];

    /// <summary>
    ///     GasCloud_DamageDealer: damage amounts below 1 have a chance to be ignored when true.
    /// </summary>
    public readonly bool damageCanGlance = true;

    /// <summary>
    ///     How many gas ticks pass between attempts to apply the effect of the gas.
    /// </summary>
    public readonly int effectInterval = 1;

    /// <summary>
    ///     Things that can be worn by pawns to negate the effects of the gas
    /// </summary>
    public readonly List<ThingDef> immunizingApparelDefs = [];

    /// <summary>
    ///     Scales the ability of immunizing apparel to negate the effects of the gas
    /// </summary>
    public readonly float immunizingApparelPower = 1f;

    /// <summary>
    ///     Scales the ability of the ToxicSensitivity stat to negate the effects of the gas on pawns
    /// </summary>
    public readonly float toxicSensitivityStatPower = 1f;

    /// <summary>
    ///     Should the gas affect organic targets?
    /// </summary>
    public bool affectsFleshy;

    /// <summary>
    ///     Should the gas affect mechanoids?
    /// </summary>
    public bool affectsMechanical;

    /// <summary>
    ///     Should the gas affect plants?
    /// </summary>
    public bool affectsPlants;

    /// <summary>
    ///     Should the gas affect regular things, as well as building?
    /// </summary>
    public bool affectsThings;

    /// <summary>
    ///     GasCloud_DamageDealer: the amount of damage to apply. Scaled by concentration, apparel, stats, etc.
    /// </summary>
    public float damageAmount;

    /// <summary>
    ///     GasCloud_DamageDealer: the damage type to apply
    /// </summary>
    public DamageDef damageDef;

    /// <summary>
    ///     GasCloud_HediffGiver: sets the hediff caused by the gas in pawns.
    /// </summary>
    public HediffDef hediffDef;

    /// <summary>
    ///     GasCloud_HediffGiver: how much the hediff severity increases per gas tick at maximum gas concentration.
    /// </summary>
    public FloatRange hediffSeverityPerGastick;
}
using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RemoteTech;

/// <summary>
///     Stores custom negative goodwill caps with non-player factions.
///     Lowering the cap circumvents the trader gassing exploit that allows to rob a trader,
///     release them, and end up with net positive faction goodwill in the end.
/// </summary>
public class FactionGoodwillCaps(World world) : WorldComponent(world)
{
    public const int DefaultMinNegativeGoodwill = -100;
    private const int NegativeGoodwillCap = -5000;
    private HashSet<int> betrayedFactions = [];

    private Dictionary<int, int> goodwillCaps = new();

    public static FactionGoodwillCaps GetFromWorld()
    {
        return Find.World.GetComponent<FactionGoodwillCaps>()
               ?? throw new NullReferenceException($"Failed to get {nameof(FactionGoodwillCaps)} from world");
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref goodwillCaps, "goodwillCaps", LookMode.Value, LookMode.Value);
        Scribe_Collections.Look(ref betrayedFactions, "betrayedFactions", LookMode.Value);
        if (Scribe.mode != LoadSaveMode.PostLoadInit)
        {
            return;
        }

        goodwillCaps ??= new Dictionary<int, int>();

        betrayedFactions ??= [];
    }

    public void SetMinNegativeGoodwill(Faction faction, int minGoodwill)
    {
        goodwillCaps[faction.loadID] = Mathf.Max(NegativeGoodwillCap, minGoodwill);
    }

    public int GetMinNegativeGoodwill(Faction faction)
    {
        if (!RemoteTechController.Instance.settings.lowerStandingCap)
        {
            return DefaultMinNegativeGoodwill;
        }

        var factionId = faction.loadID;
        return goodwillCaps.GetValueOrDefault(factionId, DefaultMinNegativeGoodwill);
    }

    public bool HasPlayerBetrayedFaction(Faction faction)
    {
        return betrayedFactions.Contains(faction.loadID);
    }

    public void OnPlayerBetrayedFaction(Faction faction)
    {
        betrayedFactions.Add(faction.loadID);
    }
}

// This ensures that existing data in saved games is properly loaded
// TodoMajor: remove during the next major update
#pragma warning disable 618 // Obsolete warning
using System;
using System.Collections.Generic;
using HugsLib;
using RimWorld;
using Verse;

namespace RemoteTech;

/// <summary>
///     A plant that has a chance to set itself of fire when walked over by a pawn.
/// </summary>
public class Plant_Sparkweed : Plant
{
    private const byte FriendlyPathCost = 150;

    private List<Pawn> touchingPawns = new(1);

    private SparkweedPlantDef CustomDef
    {
        get
        {
            if (def is SparkweedPlantDef plantDef)
            {
                return plantDef;
            }

            throw new Exception("Plant_Sparkweed requires ThingDef of type SparkweedPlantDef");
        }
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        HugsLibController.Instance.DistributedTicker.RegisterTickability(CustomTick, CustomDef.detectEveryTicks, this);
        PlayerAvoidanceGrids.AddAvoidanceSource(this, FriendlyPathCost);
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        PlayerAvoidanceGrids.RemoveAvoidanceSource(this);
        base.DeSpawn(mode);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref touchingPawns, "touchingPawns", LookMode.Reference);
        if (Scribe.mode == LoadSaveMode.LoadingVars && touchingPawns == null)
        {
            touchingPawns = [];
        }
    }

    private void CustomTick()
    {
        var thingsInCell = Map.thingGrid.ThingsListAtFast(Position);
        // detect pawns
        foreach (var thing in thingsInCell)
        {
            if (thing is not Pawn pawn)
            {
                continue;
            }

            if (touchingPawns.Contains(pawn))
            {
                continue;
            }

            touchingPawns.Add(pawn);
            OnNewPawnDetected(pawn);
        }

        // clear known pawns
        for (var i = touchingPawns.Count - 1; i >= 0; i--)
        {
            if (thingsInCell.Contains(touchingPawns[i]))
            {
                continue;
            }

            touchingPawns.RemoveAt(i);
        }
    }

    private void OnNewPawnDetected(Pawn pawn)
    {
        if (Growth < CustomDef.minimumIgnitePlantGrowth)
        {
            return;
        }

        var doEffects = false;
        if (Rand.Range(0f, 1f) < CustomDef.ignitePlantChance)
        {
            if (!BlockedByIgnitionSuppressor())
            {
                FireUtility.TryStartFireIn(Position, Map, Rand.Range(0.15f, 0.4f), null);
            }

            doEffects = true;
        }

        if (Rand.Range(0f, 1f) < CustomDef.ignitePawnChance)
        {
            if (!BlockedByIgnitionSuppressor())
            {
                pawn.TryAttachFire(Rand.Range(0.15f, 0.25f), null);
            }

            doEffects = true;
        }

        if (doEffects)
        {
            CustomDef.igniteEffecter?.Spawn().Trigger(this, pawn);
        }
    }

    private bool BlockedByIgnitionSuppressor()
    {
        var thingsInCell = Map.thingGrid.ThingsListAt(Position);
        foreach (var thing in thingsInCell)
        {
            if (!CustomDef.ignitionSuppressorThings.Contains(thing.def))
            {
                continue;
            }

            // degrade firefoam
            if (thing is Filth filth)
            {
                filth.ThinFilth();
            }

            return true;
        }

        return false;
    }
}
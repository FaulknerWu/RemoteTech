﻿using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace RemoteTech;

/// <summary>
///     A wire to connect the detonator to explosives.
///     Will get wet during rain and have a chance to fail when used unless dried.
/// </summary>
public class Building_DetonatorWire : Building
{
    private const float FreezeTemperature = -1f;
    private const float WetWeatherThreshold = .5f;
    private const float TicksPerDay = 60000;
    private const float RareTicksPerDay = TicksPerDay / GenTicks.TickRareInterval;
    private const float MaxWetness = 1f;
    private bool wantDrying;

    private float wetness;

    private BuildingProperties_DetonatorWire CustomProps
    {
        get
        {
            if (def.building is not BuildingProperties_DetonatorWire wire)
            {
                throw new Exception("Building_DetonatorWire requires BuildingProperties_DetonatorWire");
            }

            return wire;
        }
    }

    public bool WantDrying => wantDrying;

    public int DryOffJobDuration => CustomProps.dryOffJobDurationTicks;

    private float Wetness
    {
        get => wetness;
        set => wetness = Mathf.Min(value, Mathf.Max(MaxWetness, 0));
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        var comp = GetComp<CompWiredDetonationTransmitter>();
        if (comp != null)
        {
            comp.signalPassageTest = SignalPassageTest;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref wetness, "wetness");
        Scribe_Values.Look(ref wantDrying, "wantDrying");
    }

    public override void TickRare()
    {
        base.TickRare();
        var room = Position.GetRoom(Map);
        var temperature = room?.Temperature ?? 0;
        var frozen = temperature < FreezeTemperature;
        var wetWeather = Map.weatherManager.RainRate > WetWeatherThreshold;
        if (wetWeather)
        {
            if (!frozen && !IsCovered())
            {
                Wetness = MaxWetness;
            }
        }
        else
        {
            if (Wetness > 0 && temperature > 0)
            {
                Wetness -= 1 / (CustomProps.daysToSelfDry * RareTicksPerDay) *
                           (temperature / CustomProps.baseDryingTemperature);
            }
        }

        if (!wantDrying || !(Wetness < float.Epsilon))
        {
            return;
        }

        wantDrying = false;
        UpdateDesignation();
    }

    public override string GetInspectString()
    {
        return
            $"{(wetness > 0 ? "Wire_inspect_wet".Translate(Mathf.Round(wetness * 100)) : "Wire_inspect_dry".Translate())}, {(IsCovered() ? "Wire_inspect_covered".Translate() : "Wire_inspect_exposed".Translate())}";
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        if (wetness > 0)
        {
            yield return new Command_Toggle
            {
                toggleAction = DryGizmoAction,
                isActive = () => wantDrying,
                icon = Resources.Textures.rxUIDryOff,
                defaultLabel = "Wire_dry_label".Translate(),
                defaultDesc = "Wire_dry_desc".Translate(),
                hotKey = KeyBindingDefOf.Misc1
            };
        }

        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }
    }

    public void DryOff()
    {
        Wetness = 0;
        wantDrying = false;
        UpdateDesignation();
    }

    private void DryGizmoAction()
    {
        wantDrying = !wantDrying;
        UpdateDesignation();
    }

    private bool SignalPassageTest()
    {
        var failChance = wetness > 0 ? CustomProps.failureChanceWhenFullyWet * wetness : 0;
        var success = !(failChance > 0) || Rand.Range(0, 1f) > failChance;
        if (!success)
        {
            DoFailure();
        }

        return success;
    }

    private bool IsCovered()
    {
        return Position.Roofed(Map) || Map.edificeGrid[Map.cellIndices.CellToIndex(Position)] != null;
    }

    private void DoFailure()
    {
        CustomProps.failureEffecter?.Spawn().Trigger(new TargetInfo(Position, Map), null);

        var map = Map;
        Destroy(DestroyMode.KillFinalize);
        // try spawn fire in own or adjacent cell
        var adjacents = GenAdj.CardinalDirections.ToList();
        adjacents.Shuffle();
        adjacents.Add(IntVec3.Zero); // include own tile
        adjacents.Reverse();
        Fire created = null;
        foreach (var adjacent in adjacents)
        {
            var candidatePos = adjacent + Position;
            FireUtility.TryStartFireIn(candidatePos, map, Rand.Range(.4f, .6f), null);
            created = map.thingGrid.ThingAt<Fire>(candidatePos);
            if (created != null)
            {
                break;
            }
        }

        Alert_DetonatorWireFailure.Instance.ReportFailure(created);
    }

    private void UpdateDesignation()
    {
        this.ToggleDesignation(Resources.Designation.rxDetonatorWireDryOff, wantDrying);
    }
}
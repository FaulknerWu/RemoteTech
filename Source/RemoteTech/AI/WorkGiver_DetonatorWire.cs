﻿using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RemoteTech;

/// <summary>
///     Issues jobs on behalf of Building_DetonatorWire to dry it off when it is appropriately designated
/// </summary>
public class WorkGiver_DetonatorWire : WorkGiver_Scanner
{
    public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        var designations =
            pawn.Map.designationManager.SpawnedDesignationsOfDef(Resources.Designation.rxDetonatorWireDryOff);
        foreach (var designation in designations)
        {
            if (designation.target.Thing == null)
            {
                continue;
            }

            yield return designation.target.Thing;
        }
    }

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (t is not Building_DetonatorWire wire)
        {
            return false;
        }

        return wire.WantDrying && pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.Deadly);
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (t is not Building_DetonatorWire wire)
        {
            return null;
        }

        if (!wire.WantDrying)
        {
            return null;
        }

        var jobDef = Resources.Job.rxDryDetonatorWire;
        return JobMaker.MakeJob(jobDef, t);
    }
}
﻿using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RemoteTech;

/// <summary>
///     Issues jobs on behalf of Building_FoamWall to do smoothing work on it when it is appropriately designated
/// </summary>
public class WorkGiver_FoamWall : WorkGiver_Scanner
{
    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        var designations = pawn.Map.designationManager.SpawnedDesignationsOfDef(Resources.Designation.rxFoamWallSmooth);
        foreach (var designation in designations)
        {
            if (designation.target.Thing == null)
            {
                continue;
            }

            yield return designation.target.Thing;
        }
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (t is not Building_FoamWall { Spawned: true } ||
            !pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.Deadly))
        {
            return null;
        }

        return JobMaker.MakeJob(Resources.Job.rxSmoothFoamWall, t);
    }
}
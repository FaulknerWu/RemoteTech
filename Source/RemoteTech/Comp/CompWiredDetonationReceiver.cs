﻿using HugsLib;
using RimWorld;
using Verse;

namespace RemoteTech;

/// <summary>
///     Receives detonation signals from CompWiredDetonationTransmitter and light explosives attached to parent thing.
/// </summary>
public class CompWiredDetonationReceiver : CompDetonationGridNode
{
    public void ReceiveSignal(int delayTicks)
    {
        if (parent is Building_RemoteExplosive { IsArmed: false })
        {
            return;
        }

        var customExplosive = parent.GetComp<CompCustomExplosive>();
        var vanillaExplosive = parent.GetComp<CompExplosive>();
        if (customExplosive != null)
        {
            HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(() => customExplosive.StartWick(true),
                delayTicks, parent);
        }

        if (vanillaExplosive != null)
        {
            HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(() => vanillaExplosive.StartWick(),
                delayTicks, parent);
        }
    }

    public override void PrintForDetonationGrid(SectionLayer layer)
    {
        PrintEndpoint(layer);
    }
}
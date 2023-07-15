using System;
using UnityEngine;
using Verse;

namespace RemoteTech;

// Transmits the wired detonation signal to signal receiver comps and transmitter comps on adjacent tiles. 
public class CompWiredDetonationTransmitter : CompDetonationGridNode
{
    public delegate bool AllowSignalPassage();

    private int lastSignalId;

    public AllowSignalPassage signalPassageTest;

    private CompProperties_WiredDetonationTransmitter CustomProps
    {
        get
        {
            if (props is not CompProperties_WiredDetonationTransmitter transmitter)
            {
                throw new Exception(
                    "CompWiredDetonationTransmitter requires CompProperties_WiredDetonationTransmitter");
            }

            return transmitter;
        }
    }

    public override void PrintForDetonationGrid(SectionLayer layer)
    {
        PrintConnection(layer);
    }

    public virtual void ReceiveSignal(int signalId, int signalSteps, CompWiredDetonationTransmitter source = null)
    {
        if (signalId == lastSignalId || signalSteps > 5000)
        {
            return;
        }

        if (signalPassageTest != null && !signalPassageTest())
        {
            return;
        }

        lastSignalId = signalId;
        PassSignalToReceivers(signalSteps);
        ConductSignalToNeighbors(signalId, signalSteps);
    }

    private void ConductSignalToNeighbors(int signalId, int signalSteps)
    {
        if (parent.Map == null)
        {
            throw new Exception("null map");
        }

        var neighbors = GenAdj.CardinalDirectionsAround;
        foreach (var intVec3 in neighbors)
        {
            var neighborPos = intVec3 + parent.Position;
            if (!neighborPos.InBounds(parent.Map))
            {
                continue;
            }

            var tileThings = parent.Map.thingGrid.ThingsListAtFast(neighborPos);
            foreach (var thing in tileThings)
            {
                var comp = thing.TryGetComp<CompWiredDetonationTransmitter>();
                comp?.ReceiveSignal(signalId, signalSteps + 1, this);
            }
        }
    }

    private void PassSignalToReceivers(int signalSteps)
    {
        if (parent.Map == null)
        {
            throw new Exception("null map");
        }

        var delayOnThisTile = Mathf.RoundToInt(signalSteps * CustomProps.signalDelayPerTile);
        var thingsOnTile = parent.Map.thingGrid.ThingsListAtFast(parent.Position);
        foreach (var thing in thingsOnTile)
        {
            var comp = thing.TryGetComp<CompWiredDetonationReceiver>();
            comp?.ReceiveSignal(delayOnThisTile);
        }
    }
}
using System;
using Verse;

namespace RemoteTech;

/// <summary>
///     Transmits a signal only along a straight line.
/// </summary>
public class CompWiredDetonationCrossing : CompWiredDetonationTransmitter
{
    private int lastSignalIdHorizontal;
    private int lastSignalIdVertical;

    public override void ReceiveSignal(int signalId, int signalSteps, CompWiredDetonationTransmitter source = null)
    {
        if (parent.Map == null)
        {
            throw new Exception("null map");
        }

        var sourcePos = source?.parent?.Position ?? IntVec3.Invalid;
        var ownPos = parent.Position;
        if (!sourcePos.IsValid)
        {
            return;
        }

        // allow same signal to pass horizontally and vertically once
        if (sourcePos.x == ownPos.x)
        {
            if (lastSignalIdHorizontal == signalId)
            {
                return;
            }

            lastSignalIdHorizontal = signalId;
        }

        if (sourcePos.z == ownPos.z)
        {
            if (lastSignalIdVertical == signalId)
            {
                return;
            }

            lastSignalIdVertical = signalId;
        }

        // transmit to neighbors
        var neighbors = GenAdj.CardinalDirectionsAround;
        foreach (var intVec3 in neighbors)
        {
            var neighborPos = intVec3 + parent.Position;
            if (!neighborPos.InBounds(parent.Map))
            {
                continue;
            }

            if (sourcePos.x != neighborPos.x && sourcePos.z != neighborPos.z)
            {
                continue;
            }

            var tileThings = parent.Map.thingGrid.ThingsListAtFast(neighborPos);
            foreach (var thing in tileThings)
            {
                var comp = thing.TryGetComp<CompWiredDetonationTransmitter>();
                comp?.ReceiveSignal(signalId, signalSteps + 1, source);
            }
        }
    }

    public override void PrintForDetonationGrid(SectionLayer layer)
    {
        Resources.Graphics.DetWireOverlayCrossing.Print(layer, parent, 0f);
    }
}
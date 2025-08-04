using HarmonyLib;
using RimWorld;
using Verse;

namespace RemoteTech.Patches;

/// <summary>
///     Returns a pathfinding avoid grid for player colonists and other friendly non-animal pawns.
///     Normally non-hostile factions are not granted the use of an avoid grid.
/// </summary>
[HarmonyPatch(typeof(PawnUtility), nameof(PawnUtility.TryGetAvoidGrid))]
internal static class PawnUtility_TryGetAvoidGrid
{
    public static void Postfix(this Pawn p, ref ByteGrid grid)
    {
        if (grid == null && p?.Map != null && PlayerAvoidanceGrids.PawnHasPlayerAvoidanceGridKnowledge(p))
        {
            grid = PlayerAvoidanceGrids.TryGetByteGridForMap(p.Map);
        }
    }
}
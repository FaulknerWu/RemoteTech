using HarmonyLib;
using RimWorld;
using Verse;

namespace RemoteTech.Patches;

/// <summary>
///     Specifies the friendly avoid grid as a known danger source to prevent colonists from wandering into marked cells
/// </summary>
[HarmonyPatch(typeof(PawnUtility), nameof(PawnUtility.KnownDangerAt), typeof(IntVec3), typeof(Map), typeof(Pawn))]
internal static class PawnUtility_KnownDangerAt
{
    public static void Posftix(IntVec3 c, Pawn forPawn, ref bool __result)
    {
        if (!__result && PlayerAvoidanceGrids.PawnShouldAvoidCell(forPawn, c))
        {
            __result = true;
        }
    }
}
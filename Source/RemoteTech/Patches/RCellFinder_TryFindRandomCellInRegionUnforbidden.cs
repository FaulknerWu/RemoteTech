using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RemoteTech.Patches;

/// <summary>
///     Prevents friendly pawns from picking random cells that are listed in the <see cref="PlayerAvoidanceGrids" />
///     .
/// </summary>
[HarmonyPatch(typeof(RCellFinder), nameof(RCellFinder.TryFindRandomCellInRegionUnforbidden),
    [typeof(Region), typeof(Pawn), typeof(Predicate<IntVec3>), typeof(IntVec3)],
    [ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out])]
internal static class RCellFinder_TryFindRandomCellInRegionUnforbidden
{
    public static void Prefix(Pawn pawn, ref Predicate<IntVec3> validator)
    {
        var originalValidator = validator;
        validator = cell => (originalValidator == null || originalValidator(cell)) &&
                            !PlayerAvoidanceGrids.PawnShouldAvoidCell(pawn, cell);
    }
}
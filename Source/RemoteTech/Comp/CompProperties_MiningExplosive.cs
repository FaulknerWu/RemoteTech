using RimWorld;

namespace RemoteTech;

public class CompProperties_MiningExplosive : CompProperties_Explosive
{
    public readonly float breakingPower = 68400;
    public readonly float miningRadius = 2f;
    public readonly float resourceBreakingCost = 2f;
    public readonly float woodBreakingCost = 2f;

    public CompProperties_MiningExplosive()
    {
        compClass = typeof(CompMiningExplosive);
    }
}
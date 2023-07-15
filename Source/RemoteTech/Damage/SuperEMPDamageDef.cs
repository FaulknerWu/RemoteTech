using Verse;

namespace RemoteTech;

public class SuperEMPDamageDef : DamageDef
{
    public readonly float incapChance = .33f;
    public readonly float incapHealthThreshold = .25f;

    public SuperEMPDamageDef()
    {
        workerClass = typeof(DamageWorker_SuperEMP);
    }
}
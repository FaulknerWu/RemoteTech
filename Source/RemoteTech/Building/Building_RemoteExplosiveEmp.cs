using Verse.Sound;

namespace RemoteTech;

/// <summary>
///     A remote explosive with a custom wind-up sound.
/// </summary>
public class Building_RemoteExplosiveEmp : Building_RemoteExplosive
{
    private bool chargeSoundRequested;

    public Building_RemoteExplosiveEmp()
    {
        beepWhenLit = false;
    }

    protected override void LightFuse()
    {
        if (!FuseLit)
        {
            chargeSoundRequested = true;
        }

        base.LightFuse();
    }

    protected override void Tick()
    {
        base.Tick();
        if (!chargeSoundRequested)
        {
            return;
        }

        Resources.Sound.rxEmpCharge.PlayOneShot(this);
        chargeSoundRequested = false;
    }
}
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RemoteTech;

/// <summary>
///     Initiates a detonation signal carried by wire when triggered by a colonist.
/// </summary>
public class Building_DetonatorManual : Building, IGraphicVariantProvider, IPawnDetonateable, IRedButtonFeverTarget
{
    private const float PlungerDownTime = 0.2f;

    private VisualVariant currentVariant;

    private float plungerExpireTime;
    private bool wantDetonation;

    public int GraphicVariant => (int)currentVariant;

    public bool UseInteractionCell => false;

    public bool WantsDetonation
    {
        get => wantDetonation;
        set => wantDetonation = value;
    }

    public void DoDetonation()
    {
        wantDetonation = false;
        currentVariant = VisualVariant.PlungerDown;
        plungerExpireTime = Time.realtimeSinceStartup + PlungerDownTime;
        Resources.Sound.rxDetonatorLever.PlayOneShot(new TargetInfo(Position, Map));
        var transmitterComp = GetComp<CompWiredDetonationSender>();
        transmitterComp?.SendNewSignal();
    }

    public bool RedButtonFeverCanInteract => true;

    public void RedButtonFeverDoInteraction(Pawn p)
    {
        DoDetonation();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref wantDetonation, "wantDetonation");
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        Command detonate;
        if (CanDetonateImmediately())
        {
            detonate = new Command_Action
            {
                action = DoDetonation,
                defaultLabel = "Detonator_detonateNow_label".Translate()
            };
        }
        else
        {
            detonate = new Command_Toggle
            {
                toggleAction = DetonateToggleAction,
                isActive = () => wantDetonation,
                defaultLabel = "DetonatorManual_detonate_label".Translate()
            };
        }

        detonate.icon = Resources.Textures.rxUIDetonateManual;
        detonate.defaultDesc = "DetonatorManual_detonate_desc".Translate();
        detonate.hotKey = Resources.KeyBinging.rxRemoteTableDetonate;
        yield return detonate;

        foreach (var g in base.GetGizmos())
        {
            yield return g;
        }
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        if (plungerExpireTime < Time.realtimeSinceStartup)
        {
            currentVariant = VisualVariant.PlungerUp;
            plungerExpireTime = 0;
        }

        base.DrawAt(drawLoc, flip);
    }

    // quick detonation option for drafted pawns
    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
    {
        var opt = RemoteTechUtility.TryMakeDetonatorFloatMenuOption(selPawn, this);
        if (opt != null)
        {
            yield return opt;
        }

        foreach (var option in base.GetFloatMenuOptions(selPawn))
        {
            yield return option;
        }
    }

    private void DetonateToggleAction()
    {
        wantDetonation = !wantDetonation;
    }

    private bool CanDetonateImmediately()
    {
        // a drafted pawn in any of the adjacent cells allows for immediate detonation
        foreach (var intVec3 in GenAdj.AdjacentCellsAround)
        {
            var manningPawn = (Position + intVec3).GetFirstPawn(Map);
            if (manningPawn is { Drafted: true })
            {
                return true;
            }
        }

        return false;
    }

    private enum VisualVariant
    {
        PlungerUp = 0,
        PlungerDown = 1
    }
}
using System.Collections.Generic;
using Verse;

namespace RemoteTech;

/// <summary>
///     When the thing with this comp is selected, it will show Build buttons for all buildings that use this thing as an
///     ingredient.
///     The selected thing must be part of the buildings costList for this to work.
///     Relies on RemoteTechController to maintain a list of buildings for each thing def with this comp.
/// </summary>
/// <see cref="RemoteTechController" />
/// <see cref="CompProperties_BuildGizmo" />
public class CompBuildGizmo : ThingComp
{
    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (!RemoteTechController.Instance.MaterialToBuilding.TryGetValue(parent.def, out var buildingDefs))
        {
            yield break;
        }

        foreach (var thingDef in buildingDefs)
        {
            var des = new Designator_BuildLabeled(thingDef);
            des.replacementLabel = "buildGizmo_label".Translate(des.Label);
            yield return des;
        }
    }
}
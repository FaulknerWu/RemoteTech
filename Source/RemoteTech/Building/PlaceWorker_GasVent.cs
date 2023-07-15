using UnityEngine;
using Verse;

namespace RemoteTech;

public class PlaceWorker_GasVent : PlaceWorker
{
    private readonly Color BlockedArrowColor = Color.red;
    private readonly Color DefaultArrowColor = Color.white;

    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        var map = Find.CurrentMap;
        if (map == null)
        {
            return;
        }

        var targetCell = center + IntVec3.North.RotatedBy(rot);
        var sourceCell = center + IntVec3.South.RotatedBy(rot);
        if (!targetCell.InBounds(map) || !sourceCell.InBounds(map))
        {
            return;
        }

        DrawArrow(sourceCell, rot, sourceCell.Impassable(map) ? BlockedArrowColor : DefaultArrowColor);
        DrawArrow(targetCell, rot, targetCell.Impassable(map) ? BlockedArrowColor : DefaultArrowColor);
    }

    private void DrawArrow(IntVec3 pos, Rot4 rot, Color color)
    {
        var material =
            MaterialPool.MatFrom(Resources.Textures.rxGasVentArrow, ShaderDatabase.TransparentPostLight, color);
        Graphics.DrawMesh(MeshPool.plane10, pos.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays), rot.AsQuat,
            material, 0);
    }
}
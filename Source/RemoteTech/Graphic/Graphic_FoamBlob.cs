using UnityEngine;
using Verse;

namespace RemoteTech;

/// <summary>
///     A Graphic_Single with scaling support
/// </summary>
public class Graphic_FoamBlob : Graphic_Single
{
    public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
    {
        var blob = (Building_FoamBlob)thing;
        var scaleMultiplier = blob.spriteScaleMultiplier;
        var meshScale = new Vector2(drawSize.x * scaleMultiplier.x, drawSize.y * scaleMultiplier.y);
        var material = thing.Graphic.MatAt(thing.Rotation, thing);

        var matrix = default(Matrix4x4);

        var drawPos = thing.DrawPos;
        var customAltitude = thing.def.altitudeLayer.AltitudeFor() +
                             (Altitudes.AltInc * (thing.Map.Size.z - thing.Position.z));
        matrix.SetTRS(new Vector3(drawPos.x, customAltitude, drawPos.z), rot.AsQuat,
            new Vector3(meshScale.x, 0, meshScale.y));
        Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);

        ShadowGraphic?.DrawWorker(thing.Position.ToVector3(), thing.Rotation, thing.def, thing, extraRotation);
    }
}
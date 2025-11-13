using UnityEngine;
using Verse;

namespace RemoteTech;

/// <summary>
///     A graphic that can change appearance between multiple graphics, current variant is specified by
///     IGraphicVariantProvider
/// </summary>
public class Graphic_Variants : Graphic_Collection
{
    public override Material MatSingle => GetDefaultMat();

    public override Material MatAt(Rot4 rot, Thing thing = null)
    {
        return MatSingleFor(thing);
    }

    public override Material MatSingleFor(Thing thing)
    {
        if (thing is not IGraphicVariantProvider provider)
        {
            return GetDefaultMat();
        }

        var variantIndex = provider.GraphicVariant;
        if (variantIndex >= 0 && variantIndex <= subGraphics.Length)
        {
            return subGraphics[variantIndex].MatSingleFor(thing);
        }

        Log.Error(
            $"No material with index {variantIndex} available, as requested by {thing.GetType()}");
        return GetDefaultMat();
    }

    private Material GetDefaultMat()
    {
        return subGraphics.Length > 0 ? subGraphics[0].MatSingle : base.MatSingle;
    }
}
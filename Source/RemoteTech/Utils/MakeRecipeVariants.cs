using System;
using Verse;

namespace RemoteTech;

/// <summary>
///     Put on RecipeDef to specify additional variants of the recipe to be created
/// </summary>
public class MakeRecipeVariants : DefModExtension, ICloneable
{
    public RecipeVariantType Variant;

    public object Clone()
    {
        return new MakeRecipeVariants { Variant = Variant };
    }
}
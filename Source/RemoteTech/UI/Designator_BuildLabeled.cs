using RimWorld;
using Verse;

namespace RemoteTech;

/// <summary>
///     A Designator_Build with a replaceable label.
/// </summary>
public class Designator_BuildLabeled(BuildableDef entDef) : Designator_Build(entDef)
{
    public string replacementLabel;

    public override string Label => replacementLabel ?? base.Label;
}
// ReSharper disable UnassignedField.Global

using RimWorld;
using Verse;

namespace RemoteTech;

public class CompProperties_ChemicalExplosive : CompProperties_Explosive
{
    public SoundDef breakSound;
    public float gasConcentration;
    public int numFoamBlobs;
    public ThingDef spawnThingDef;
}
using HarmonyLib;
using RimWorld;
using Verse;

namespace RemoteTech.Patches;

/// <summary>
///     Allows types extending CompPowerTrader to be recognized as power grid connectables
/// </summary>
[HarmonyPatch(typeof(ThingDef))]
[HarmonyPatch("ConnectToPower", MethodType.Getter)]
internal class ThingDef_ConnectToPower_Patch
{
    [HarmonyPostfix]
    public static void AllowPolymorphicComps(ThingDef __instance, ref bool __result)
    {
        if (__instance.EverTransmitsPower)
        {
            return;
        }

        foreach (var compProperties in __instance.comps)
        {
            if (!typeof(CompPowerTrader).IsAssignableFrom(compProperties?.compClass))
            {
                continue;
            }

            __result = true;
            return;
        }
    }
}
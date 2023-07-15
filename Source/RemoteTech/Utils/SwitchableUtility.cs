using System.Collections.Generic;
using System.Linq;
using HugsLib.Utils;
using Verse;

namespace RemoteTech;

public static class SwitchableUtility
{
    public static void UpdateSwitchDesignation(this Thing thing)
    {
        if (thing.Map == null)
        {
            return;
        }

        thing.ToggleDesignation(Resources.Designation.rxSwitchThing, thing.WantsSwitching());
    }

    public static bool WantsSwitching(this Thing thing)
    {
        return SwitchablesOnThing(thing).Any(s => s.WantsSwitch());
    }

    public static void TrySwitch(this Thing thing)
    {
        foreach (var s in SwitchablesOnThing(thing))
        {
            if (s.WantsSwitch())
            {
                s.DoSwitch();
            }
        }
    }

    private static IEnumerable<ISwitchable> SwitchablesOnThing(Thing thing)
    {
        var list = new List<ISwitchable>();
        if (thing is ISwitchable t)
        {
            list.Add(t);
        }

        if (thing is not ThingWithComps comps)
        {
            return list;
        }

        foreach (var comp in comps.AllComps)
        {
            if (comp is ISwitchable s)
            {
                list.Add(s);
            }
        }

        return list;
    }
}
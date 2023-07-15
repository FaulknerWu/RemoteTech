﻿using Verse;

namespace RemoteTech;

/// <summary>
///     Stores the AutoReplaceWatcher for an individual map
/// </summary>
public class MapComponent_RemoteTech : MapComponent
{
    private AutoReplaceWatcher replaceWatcher;

    public MapComponent_RemoteTech(Map map) : base(map)
    {
        replaceWatcher = new AutoReplaceWatcher();
        replaceWatcher.SetParentMap(map);
    }

    public AutoReplaceWatcher ReplaceWatcher => replaceWatcher;

    public override void ExposeData()
    {
        Scribe_Deep.Look(ref replaceWatcher, "replaceWatcher");
        if (replaceWatcher == null)
        {
            replaceWatcher = new AutoReplaceWatcher();
        }

        replaceWatcher.SetParentMap(map);
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        replaceWatcher.Tick();
    }
}
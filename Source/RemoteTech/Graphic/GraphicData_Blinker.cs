using UnityEngine;
using Verse;

namespace RemoteTech;

/// <summary>
///     Additional settings for buildings with blinking overlays, such as the radio mast and remote charges
/// </summary>
public class GraphicData_Blinker : GraphicData
{
    public readonly int blinkerIntervalActive = 7;
    public readonly int blinkerIntervalNormal = 100;
    public Color blinkerColor = Color.white;
    public Vector3 blinkerOffset;
    public Vector3 blinkerScale = Vector3.one;
}
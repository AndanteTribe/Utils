using System.Runtime.InteropServices;
using AndanteTribe.Utils.MasterSample.Enums;
using MessagePack;

namespace AndanteTribe.Utils.MasterSample.Units;

/// <summary>
/// 属性相性グループ.
/// </summary>
/// <param name="Physics">物理.</param>
/// <param name="Fire">火.</param>
/// <param name="Water">水.</param>
/// <param name="Wind">風.</param>
/// <param name="Earth">土.</param>
[MessagePackObject]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct CompatibilityGroup(
    [property: Key(0)] Compatibility Physics,
    [property: Key(1)] Compatibility Fire,
    [property: Key(2)] Compatibility Water,
    [property: Key(3)] Compatibility Wind,
    [property: Key(4)] Compatibility Earth);
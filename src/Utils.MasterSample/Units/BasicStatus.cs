using System.Runtime.InteropServices;
using MessagePack;

namespace AndanteTribe.Utils.MasterSample.Units;

[MessagePackObject]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct BasicStatus(
    [property: Key(0)] int HP,
    [property: Key(1)] ushort Strength,
    [property: Key(2)] ushort Vitality,
    ushort Agility,
    [property: Key(4)] byte Luck,
    [property: Key(5)] ushort WillPower,
    [property: Key(6)] ushort LinkGauge)
{
    private readonly byte _agilityOffset = (byte)(Agility - 50u);

    /// <summary>
    /// 敏捷.
    /// </summary>
    [Key(3)]
    public ushort Agility
    {
        get => (ushort)(_agilityOffset + 50u);
        init => _agilityOffset = (byte)(value - 50u);
    }
}
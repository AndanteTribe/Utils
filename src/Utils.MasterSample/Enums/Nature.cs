using System.Runtime.Serialization;

namespace AndanteTribe.Utils.MasterSample.Enums;

[Flags]
public enum Nature
{
    /// <summary>
    /// 無効値.
    /// </summary>
    [EnumMember(Value = "無効値")]
    Invalid = 0,

    /// <summary>
    /// 混沌.
    /// </summary>
    [EnumMember(Value = "混沌")]
    Chaos = 1 << 0,

    /// <summary>
    /// 中庸.
    /// </summary>
    [EnumMember(Value = "中庸")]
    Neutral = 1 << 1,

    /// <summary>
    /// 善.
    /// </summary>
    [EnumMember(Value = "善")]
    Good = 1 << 2,

    /// <summary>
    /// 秩序.
    /// </summary>
    [EnumMember(Value = "秩序")]
    Order = 1 << 3,
}
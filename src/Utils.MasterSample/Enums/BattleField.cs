using System.Runtime.Serialization;

namespace AndanteTribe.Utils.MasterSample.Enums;

public enum BattleField : byte
{
    Invalid = 0,

    /// <summary>
    /// 空中.
    /// </summary>
    [EnumMember(Value = "空中")]
    Air = 1,

    /// <summary>
    /// 地上.
    /// </summary>
    [EnumMember(Value = "地上")]
    Ground = 2,
}
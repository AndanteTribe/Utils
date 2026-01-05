using System.Runtime.Serialization;

namespace AndanteTribe.Utils.MasterSample.Enums;

public enum Compatibility : byte
{
    /// <summary>
    /// 無効値.
    /// </summary>
    [EnumMember(Value = "無効値")]
    Invalid = 0,

    /// <summary>
    /// 弱点. 被ダメージが2倍.
    /// </summary>
    [EnumMember(Value = "弱点")]
    Weak = 1,

    /// <summary>
    /// 等倍. 被ダメージが通常通り.
    /// </summary>
    [EnumMember(Value = "等倍")]
    Normal = 2,

    /// <summary>
    /// 半減. 被ダメージが1/2倍.
    /// </summary>
    [EnumMember(Value = "半減")]
    Resist = 3,

    /// <summary>
    /// 無効. 被ダメージなし.
    /// </summary>
    [EnumMember(Value = "無効")]
    Nullify = 4,
}
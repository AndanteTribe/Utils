namespace AndanteTribe.Utils.GameServices;

/// <summary>
/// ローカライズ対象メンバーを指定する属性.
/// </summary>
/// <remarks>
/// 現在string型のみ対応.
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class LocalizedMemberAttribute : Attribute
{
}
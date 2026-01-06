namespace AndanteTribe.Utils;

/// <summary>
/// ファイルパスを指定する属性.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Delegate)]
public class FileNameAttribute : Attribute
{
    /// <summary>
    /// ファイル名.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// Initialize a new instance of <see cref="FileNameAttribute"/>.
    /// </summary>
    /// <param name="name">ファイル名.</param>
    public FileNameAttribute(string name) => Name = name;
}
using AndanteTribe.Utils.GameServices;
using AndanteTribe.Utils.MasterSample.Enums;
using AndanteTribe.Utils.MasterSample.Units;
using AndanteTribe.Utils.MasterSample.Utils;
using MasterMemory;
using MessagePack;

namespace AndanteTribe.Utils.MasterSample;

[MemoryTable(nameof(EnemyMasterEntity)), MessagePackObject]
[FileName("enemy.csv")]
public record EnemyMasterEntity : IValidatable<EnemyMasterEntity>
{
    private readonly Obscured<MasterId<BattleField>> _id;

    /// <summary>
    /// マスターID.
    /// </summary>
    [PrimaryKey, Key(0)]
    public required MasterId<BattleField> Id
    {
        get => _id;
        init => _id = value;
    }

    /// <summary>
    /// グループ（フィールド種別）.
    /// </summary>
    [SecondaryKey(0), NonUnique, IgnoreMember]
    public BattleField Group => Id.Group;

    /// <summary>
    /// 種族.
    /// </summary>
    [LocalizedMember, Key(1)]
    public required string Species { get; init; }

    /// <summary>
    /// 性質.
    /// </summary>
    [Key(2)]
    public required Nature Property { get; init; }

    private readonly Obscured<BasicStatus> _status;

    /// <summary>
    /// 基礎ステータス.
    /// </summary>
    [Key(3)]
    public required BasicStatus Status
    {
        get => _status;
        init => _status = value;
    }

    private readonly Obscured<CompatibilityGroup> _compatibilities;

    /// <summary>
    /// 相性情報.
    /// </summary>
    [Key(4)]
    public required CompatibilityGroup Compatibilities
    {
        get => _compatibilities;
        init => _compatibilities = value;
    }

    void IValidatable<EnemyMasterEntity>.Validate(IValidator<EnemyMasterEntity> validator)
    {
#if !DISABLE_MASTERMEMORY_VALIDATOR
        validator.Validate(e => ValidateUtils.ValidateBasicStatus(e.Status), "BasicStatus部分の値設定が不正です。");
#endif
    }
}
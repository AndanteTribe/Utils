using AndanteTribe.Utils.GameServices;
using AndanteTribe.Utils.MasterSample.Enums;
using MasterMemory;
using MessagePack;

namespace AndanteTribe.Utils.MasterSample;

[MemoryTable(nameof(GroundEnemyMasterEntity)), MessagePackObject]
[FileName("enemy_ground")]
public record GroundEnemyMasterEntity : IValidatable<GroundEnemyMasterEntity>
{
    private readonly Obscured<MasterId<GroundEnemyCategory>> _id;

    /// <summary>
    /// マスターID.
    /// </summary>
    [PrimaryKey, Key(0)]
    public required MasterId<GroundEnemyCategory> Id
    {
        get => _id;
        init => _id = value;
    }

    /// <summary>
    /// グループ（地上敵種別）.
    /// </summary>
    [SecondaryKey(0), NonUnique, IgnoreMember]
    public GroundEnemyCategory Group => Id.Group;

    private readonly Obscured<MasterId<BattleField>> _enemyId;

    /// <summary>
    /// <see cref="EnemyMasterEntity"/>のマスターID.
    /// </summary>
    [Key(1)]
    public required MasterId<BattleField> EnemyId
    {
        get => _enemyId;
        init => _enemyId = value;
    }

    private readonly Obscured<uint> _idleChaseDistance;

    /// <summary>
    /// 非戦闘時の敵とプレイヤーの距離.
    /// </summary>
    [Key(2)]
    public required uint IdleChaseDistance
    {
        get => _idleChaseDistance;
        init => _idleChaseDistance = value;
    }

    private readonly Obscured<uint> _battleChaseDistance;

    /// <summary>
    /// 戦闘時の敵とプレイヤーの距離.
    /// </summary>
    [Key(3)]
    public required uint BattleChaseDistance
    {
        get => _battleChaseDistance;
        init => _battleChaseDistance = value;
    }

    void IValidatable<GroundEnemyMasterEntity>.Validate(IValidator<GroundEnemyMasterEntity> validator)
    {
#if !DISABLE_MASTERMEMORY_VALIDATOR
        var enemies = validator.GetReferenceSet<EnemyMasterEntity>();
        enemies.Exists(static x => x.EnemyId, static x => x.Id);
#endif
    }
}
using Assets.Prototypes.Characters.Configuration;
using Assets.Prototypes.Characters.Stats;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGenericCharacter", menuName = "Character/Character")]
public class Character : ScriptableObject
{
    [Header("Character Configuration")]
    [SerializeField]
    private CharacterData _configBlock;

    public CharacterData Data => _configBlock;

    // Convenience accessors for stats
    public System.Collections.Generic.List<BoundedCharacterStat> BoundedStats =>
        _configBlock?.BoundedStats;
    public System.Collections.Generic.List<CharacterStat> UnboundedStats =>
        _configBlock?.UnboundedStats;

    public BoundedCharacterStat GetBoundedStat(BoundedStatType type) =>
        _configBlock?.GetBoundedStat(type);

    public CharacterStat GetUnboundedStat(UnboundedStatType type) =>
        _configBlock?.GetUnboundedStat(type);
}

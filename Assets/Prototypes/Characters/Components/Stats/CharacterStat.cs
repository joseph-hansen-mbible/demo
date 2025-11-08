using UnityEngine;

namespace Assets.Prototypes.Characters.Stats
{
    [System.Serializable]
    public class CharacterStat : BaseCharacterStat
    {
        [SerializeField]
        private UnboundedStatType _statType = UnboundedStatType.Strength;

        public CharacterStat() { }

        public CharacterStat(
            float current = 0,
            UnboundedStatType statType = UnboundedStatType.Strength
        )
        {
            _statType = statType;
            _current = current;
            _bonus = 0;
        }

        public UnboundedStatType StatType => _statType;
        public override string Name => _statType.ToString();
        public override string DisplayName => _statType.GetDisplayName();
        public override string Description => _statType.GetDescription();

        public override void SetCurrent(float value) => _current = value;

        // Allow using CharacterStat as an int in code: int value = myStat;
        public static implicit operator int(CharacterStat s) => s?.Get() ?? 0;
    }
}

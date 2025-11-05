using UnityEngine;

namespace Assets.Prototypes.Characters.Stats
{
    [System.Serializable]
    public class CharacterStat
    {
        [SerializeField]
        private UnboundedStatType _statType = UnboundedStatType.Strength;

        [SerializeField]
        private float _current;

        [SerializeField]
        private float _bonus;

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
        public string Name => _statType.ToString();
        public string DisplayName => _statType.GetDisplayName();
        public string Description => _statType.GetDescription();
        public float Current => Mathf.Round(_current);
        public float Bonus => Mathf.Round(_bonus);

        // Int accessors
        public int CurrentInt => Mathf.RoundToInt(_current);
        public int BonusInt => Mathf.RoundToInt(_bonus);

        // Returns current + bonus as int
        public int Get() => Mathf.RoundToInt(_current + _bonus);

        public void SetCurrent(float value) => _current = value;

        public void SetBonus(float value) => _bonus = value;

        // Allow using CharacterStat as an int in code: int value = myStat;
        public static implicit operator int(CharacterStat s) => s?.Get() ?? 0;
    }
}

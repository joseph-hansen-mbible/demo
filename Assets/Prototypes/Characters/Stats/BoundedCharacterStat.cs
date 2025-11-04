using UnityEngine;

namespace Assets.Prototypes.Characters.Stats
{
    /// <summary>
    /// Bounded stat - clamped between min and max values with progress bar visualization
    /// </summary>
    [System.Serializable]
    public class BoundedCharacterStat
    {
        [SerializeField]
        private BoundedStatType _statType = BoundedStatType.Health;

        [SerializeField]
        private float _max = 100f;

        [SerializeField]
        private float _current = 100f;

        [SerializeField]
        private float _bonus;

        [SerializeField]
        private float _min;

        public BoundedCharacterStat() { }

        public BoundedCharacterStat(
            float max,
            float current = -1,
            float min = 0,
            BoundedStatType statType = BoundedStatType.Health
        )
        {
            _statType = statType;
            _max = max;
            _min = min;
            _current = current < 0 ? max : Mathf.Clamp(current, min, max);
            _bonus = 0;
        }

        public BoundedStatType StatType => _statType;
        public string Name => _statType.ToString();
        public string DisplayName => _statType.GetDisplayName();
        public string Description => _statType.GetDescription();
        public float Current => Mathf.Round(Mathf.Clamp(_current, _min, _max));
        public float Bonus => Mathf.Round(_bonus);
        public float Max => Mathf.Round(_max);
        public float Min => Mathf.Round(_min);
        public float Ratio => _max == 0 ? 0 : (Mathf.Clamp(_current, _min, _max) + _bonus) / _max;

        // Int accessors for all values
        public int CurrentInt => Mathf.RoundToInt(Mathf.Clamp(_current, _min, _max));
        public int BonusInt => Mathf.RoundToInt(_bonus);
        public int MaxInt => Mathf.RoundToInt(_max);
        public int MinInt => Mathf.RoundToInt(_min);

        // Returns current + bonus as int (clamped)
        public int Get() => Mathf.RoundToInt(Mathf.Clamp(_current, _min, _max) + _bonus);

        public void SetMax(float value)
        {
            _max = value;
            _current = Mathf.Clamp(_current, _min, _max);
        }

        public void SetMin(float value)
        {
            _min = value;
            _current = Mathf.Clamp(_current, _min, _max);
        }

        public void SetCurrent(float value) => _current = Mathf.Clamp(value, _min, _max);

        public void SetBonus(float value) => _bonus = value;

        public void SetBonusPercent(float percent) => _bonus = _max * percent / 100f;

        // Allow using BoundedCharacterStat as an int in code: int hp = myStat;
        public static implicit operator int(BoundedCharacterStat s) => s?.Get() ?? 0;
    }
}

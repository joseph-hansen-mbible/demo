using UnityEngine;

namespace Turnroot.Characters.Stats
{
    /// <summary>
    /// Base class for all character stats (bounded and unbounded)
    /// </summary>
    [System.Serializable]
    public abstract class BaseCharacterStat
    {
        [SerializeField]
        protected float _current;

        [SerializeField]
        protected float _bonus;

        public abstract string Name { get; }
        public abstract string DisplayName { get; }
        public abstract string Description { get; }

        public virtual float Current => Mathf.Round(_current);
        public virtual float Bonus => Mathf.Round(_bonus);

        // Int accessors
        public virtual int CurrentInt => Mathf.RoundToInt(_current);
        public virtual int BonusInt => Mathf.RoundToInt(_bonus);

        // Returns current + bonus as int
        public virtual int Get() => Mathf.RoundToInt(_current + _bonus);

        public abstract void SetCurrent(float value);

        public virtual void SetBonus(float value) => _bonus = value;
    }
}

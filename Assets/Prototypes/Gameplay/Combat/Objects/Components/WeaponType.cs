using UnityEngine;

namespace Assets.Prototypes.Gameplay.Combat.Objects.Components
{
    [CreateAssetMenu(fileName = "WeaponType", menuName = "Game Settings/Gameplay/Weapon Type")]
    [System.Serializable]
    public class WeaponType : ScriptableObject
    {
        [SerializeField]
        private string _name;

        [SerializeField]
        private Sprite _icon;

        [SerializeField]
        private string _id;

        [SerializeField]
        private int[] _ranges;

        [SerializeField]
        private int _defaultRange;

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public Sprite Icon
        {
            get => _icon;
            set => _icon = value;
        }

        public string Id
        {
            get => _id;
            set => _id = value;
        }

        public int[] Ranges
        {
            get => _ranges;
            set => _ranges = value;
        }

        public int DefaultRange
        {
            get => _defaultRange;
            set => _defaultRange = value;
        }
    }
}

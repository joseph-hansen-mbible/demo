using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Prototypes.Characters.Subclasses
{
    [Serializable]
    public class SupportLevels
    {
        public enum Level
        {
            None,
            E,
            D,
            C,
            B,
            A,
            S,
        }
        private Level _currentLevel = Level.None;
        public Level CurrentLevel
        {
            get => _currentLevel;
            set => _currentLevel = value;
        }

        public SupportLevels(Level initialLevel = Level.None)
        {
            _currentLevel = initialLevel;
        }

        public void IncreaseLevel()
        {
            if (_currentLevel < Level.S)
            {
                _currentLevel++;
            }
        }

        public void DecreaseLevel()
        {
            if (_currentLevel > Level.None)
            {
                _currentLevel--;
            }
        }
    }
}

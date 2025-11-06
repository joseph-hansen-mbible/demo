using System;
using Assets.Prototypes.Graphics2D;
using UnityEngine;

namespace Assets.Prototypes.Characters.Subclasses
{
    [Serializable]
    public class Portrait : StackedImage<CharacterData>
    {
        protected override string GetSaveSubdirectory()
        {
            return "Portraits";
        }

        public override void UpdateTintColorsFromOwner()
        {
            // Ensure array is initialized
            if (_tintColors == null || _tintColors.Length < 3)
            {
                _tintColors = new Color[3] { Color.white, Color.white, Color.white };
            }

            if (_owner != null)
            {
                _tintColors[0] = _owner.AccentColor1;
                _tintColors[1] = _owner.AccentColor2;
                _tintColors[2] = _owner.AccentColor3;
            }
        }
    }
}

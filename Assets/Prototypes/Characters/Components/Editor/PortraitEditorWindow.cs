using Assets.Prototypes.Characters.Subclasses;
using Assets.Prototypes.Graphics2D.Editor;
using UnityEditor;

namespace Assets.Prototypes.Characters.Subclasses.Editor
{
    public class PortraitEditorWindow : StackedImageEditorWindow<CharacterData, Portrait>
    {
        protected override string WindowTitle => "Portrait Editor";
        protected override string OwnerFieldLabel => "Character";

        [MenuItem("Window/Portrait Editor")]
        public static void ShowWindow()
        {
            GetWindow<PortraitEditorWindow>("Portrait Editor");
        }

        public static void OpenPortrait(CharacterData character, int portraitIndex = 0)
        {
            var window = GetWindow<PortraitEditorWindow>("Portrait Editor");
            window._currentOwner = character;
            window._selectedImageIndex = portraitIndex;
            if (character != null && character.Portraits != null && portraitIndex < character.Portraits.Length)
            {
                window._currentImage = character.Portraits[portraitIndex];
                window.RefreshPreview();
            }
        }

        protected override Portrait[] GetImagesFromOwner(CharacterData owner)
        {
            return owner?.Portraits;
        }
    }
}

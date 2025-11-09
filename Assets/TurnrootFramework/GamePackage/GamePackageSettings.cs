using NaughtyAttributes;
using UnityEngine;

namespace Turnroot.GamePackage
{
    [CreateAssetMenu(
        fileName = "GamePackageSettings",
        menuName = "Turnroot/Game Settings/Game Package Settings"
    )]
    public class GamePackageSettings : SingletonScriptableObject<GamePackageSettings>
    {
        public string gameName;

        [ResizableTextArea]
        public string gameDescription;
        public string gameTagline;

        public Color gameThemeColor;
        public SerializableDictionary<string, string> credits = new();

        [Label("Game Icon 1:1")]
        public Sprite gameIcon;

        [Label("Game Banner 16:9")]
        public Sprite gameBannerDesktop;

        [Label("Game Banner 4:1")]
        public Sprite gameBannerWide;
    }
}

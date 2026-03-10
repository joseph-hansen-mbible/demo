using UnityEngine;
using TMPro;
using Turnroot.Gameplay.Brain;
using UnityEngine.UI;
using Turnroot.Utilities;
using Coffee.UIEffects;

namespace Turnroot.Demos.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SaveFileUiManager : MonoBehaviour
    {
        public TextMeshProUGUI AvatarName;
        public TextMeshProUGUI Chapter;
        public TextMeshProUGUI PlayTime;
        public Image AvatarPortrait;
        public Sprite PortraitFrame;
        public RectTransform ToScale => GetComponent<RectTransform>();
        public UIEffect Effect;

        public bool IsActive { get; private set; } = false;

        public void Select()
        {
            IsActive = true;
            ToScale.localScale = Vector3.one * 1.1f;
            Effect.enabled = true;
        }

        public void Deselect()
        {
            IsActive = false;
            ToScale.localScale = Vector3.one;
            Effect.enabled = false;
        }

        public void UpdateSaveFileInfo(SaveFile saveFile)
        {
            // treat a slot as "new" only if it lacks a proper name; other fields may not
            // be populated during early testing and shouldn't hide the rest of the info.
            // TODO: once portrait/body type are part of the creation flow we can
            // revisit this logic and possibly show placeholders or warning icons.
            if (string.IsNullOrEmpty(saveFile.FileName) || saveFile.FileName == "Unnamed")
            {
                AvatarName.text = "NEW GAME";
                Chapter.text = "";
                Chapter.gameObject.SetActive(false);
                PlayTime.text = "";
                AvatarPortrait.sprite = PortraitFrame;
            }
            else
            {
                AvatarName.text = saveFile.FileName;
                Chapter.text = $"Chapter: {saveFile.ChapterNumber} - {saveFile.ChapterName}";
                PlayTime.text = $"{Converters.SecondsToHoursAndMinutes(saveFile.playTimeSeconds)}";

                // use frame if the portrait isn't set yet
                AvatarPortrait.sprite = saveFile.AvatarPortrait ?? PortraitFrame;
                Chapter.gameObject.SetActive(true);
            }
        }
    }

}
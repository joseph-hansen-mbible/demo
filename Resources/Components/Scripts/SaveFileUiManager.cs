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
            if (saveFile.AvatarBodyType == AvatarBody.None || saveFile.AvatarPortrait == null || string.IsNullOrEmpty(saveFile.FileName))
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
                AvatarPortrait.sprite = saveFile.AvatarPortrait;
                Chapter.gameObject.SetActive(true);
            }
        }
    }

}
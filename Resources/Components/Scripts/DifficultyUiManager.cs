using Coffee.UIEffects;
using UnityEngine;

namespace Turnroot.Demos.UI
{
    public class DifficultyUiManager : MonoBehaviour
    {
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
    }
}

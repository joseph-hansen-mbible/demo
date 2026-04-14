using UnityEngine;
using Turnroot.Utilities.AbstractScripts;
using Turnroot.Graphics2D;
using Turnroot.Utilities;

namespace Turnroot.Demos
{
    public partial class GameStartManager
    {
        public void ReadyKeyboard(string defaultText = null)
        {
            ScreenKeyboard.GetComponent<UIFade>().Show();
            _keyboard = ScreenKeyboard.GetComponentInChildren<ScreenKeyboard>();
            _keyboard.OnSubmit = OnKeyboardSubmit;
            if (!string.IsNullOrEmpty(defaultText))
            {
                _keyboard.SetText(defaultText);
            }

            SetInputMode(InputMode.Keyboard);
        }

        private void OnKeyboardSubmit(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                saveFileBrain.Brain.PublishUpdateSaveFileName(text);
            }
            ScreenKeyboard.GetComponent<UIFade>().Hide();
            SetInputMode(InputMode.None);
        }

        public void ReadyDateKeyboard()
        {
            NumberDateKeyboard.GetComponent<UIFade>().Show();
            _dateKeyboard = NumberDateKeyboard.GetComponentInChildren<ScreenKeyboard>();
            _dateKeyboard.OnSubmit = OnDateKeyboardSubmit;
            SetInputMode(InputMode.Date);
        }

        private void OnDateKeyboardSubmit(string text)
        {
            if (!string.IsNullOrEmpty(text) && int.TryParse(text, out int day))
            {
                selectedBirthdayDay = Mathf.Clamp(day, 1, 31);
                $"Selected birthday day: {selectedBirthdayDay}".LogInfo("GameStartManager");
            }
            NumberDateKeyboard.GetComponent<UIFade>().Hide();
            SetInputMode(InputMode.None);
        }
    }
}
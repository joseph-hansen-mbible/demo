using Turnroot.Characters.Subclasses;
using Turnroot.Gameplay.PlayerSettings;
using Turnroot.UI;
using Turnroot.Utilities;

namespace Turnroot.Demos
{
    public partial class GameStartManager
    {
        private enum InputMode { None, Keyboard, SaveFiles, Pronouns, StarGifts, Date, Difficulty, Permadeath }

        private void HandleInput(string action)
        {
            switch (_currentInputMode)
            {
                case InputMode.Keyboard when _keyboard != null:
                    _keyboard.HandleInput(action);
                    return;
                case InputMode.SaveFiles:
                    HandleSaveFileInput(action);
                    return;
                case InputMode.Pronouns:
                    HandlePronounsInput(action);
                    return;
                case InputMode.StarGifts:
                    HandleStarGiftInput(action);
                    return;
                case InputMode.Date when _dateKeyboard != null:
                    _dateKeyboard.HandleInput(action);
                    return;
                case InputMode.Difficulty:
                    HandleDifficultyInput(action);
                    return;
                case InputMode.Permadeath:
                    HandlePermadeathInput(action);
                    return;
            }

            if (action is "Select" or "Start")
            {
                if (EntryFade.Visible)
                {
                    StartFx.PlayOneShot(StartClip);
                    EntryFade.Hide();

                    if (saveFileBrain == null)
                    {
                        "SaveFileBrain not found!".LogError("GameStartManager");
                        return;
                    }

                    SaveFilesFade.Show();
                    SetInputMode(InputMode.SaveFiles);
                }
            }
        }

        private void SetInputMode(InputMode mode)
        {
            _currentInputMode = mode;
            currentIndex = 0;
        }

        public void ShowAvatarPronounSelection() => SetInputMode(InputMode.Pronouns);

        public void ShowDifficultySelection() => SetInputMode(InputMode.Difficulty);

        public void ShowPermadeathSelection() => SetInputMode(InputMode.Permadeath);

        private void HandlePermadeathInput(string action)
        {
            if (_currentInputMode != InputMode.Permadeath)
            {
                return;
            }

            if (InputProvider != null)
            {
                InputProvider.Navigate(
                    action,
                    PermadeathUiManagers,
                    ref currentIndex,
                    2,
                    () =>
                    {
                        GameplayPlayerSettings.Instance.Permadeath = currentIndex == 0;
                        // disable further input while fade is happening
                        SetInputMode(InputMode.None);
                        enabled = false;
                        PermadeathFade.Hide();
                    });
            }
            else
            {
                UiChoiceHandler.HandleNavigation(
                    action,
                    PermadeathUiManagers,
                    ref currentIndex,
                    2,
                    () =>
                    {
                        GameplayPlayerSettings.Instance.Permadeath = currentIndex == 0;
                        // disable further input while fade is happening
                        SetInputMode(InputMode.None);
                        enabled = false;
                        PermadeathFade.Hide();
                    });
            }
        }

        private void HandlePronounsInput(string action)
        {
            if (_currentInputMode != InputMode.Pronouns)
            {
                return;
            }

            if (InputProvider != null)
            {
                InputProvider.Navigate(
                    action,
                    PronounsUiManagers,
                    ref currentIndex,
                    3,
                    () =>
                    {
                        selectedPronouns = currentIndex switch
                        {
                            0 => new Pronouns("she"),
                            1 => new Pronouns("he"),
                            2 => new Pronouns("they"),
                            _ => selectedPronouns
                        };

                        AvatarData.SetAvatarNameAndPronouns(
                            saveFileBrain.ActiveSaveFile.FileName,
                            saveFileBrain.ActiveSaveFile.FileName + " " + LastName,
                            selectedPronouns
                        );

                        SetInputMode(InputMode.None);
                        enabled = false;
                        PronounsFade.Hide();
                    });
            }
            else
            {
                UiChoiceHandler.HandleNavigation(
                    action,
                    PronounsUiManagers,
                    ref currentIndex,
                    3,
                    () =>
                    {
                        selectedPronouns = currentIndex switch
                        {
                            0 => new Pronouns("she"),
                            1 => new Pronouns("he"),
                            2 => new Pronouns("they"),
                            _ => selectedPronouns
                        };

                        AvatarData.SetAvatarNameAndPronouns(
                            saveFileBrain.ActiveSaveFile.FileName,
                            saveFileBrain.ActiveSaveFile.FileName + " " + LastName,
                            selectedPronouns
                        );

                        SetInputMode(InputMode.None);
                        enabled = false;
                        PronounsFade.Hide();
                    });
            }
        }

        private void HandleDifficultyInput(string action)
        {
            if (_currentInputMode != InputMode.Difficulty)
            {
                return;
            }

            if (InputProvider != null)
            {
                InputProvider.Navigate(
                    action,
                    DifficultyUiManagers,
                    ref currentIndex,
                    4,
                    () =>
                    {
                        GameplayPlayerSettings.Instance.GameDifficulty = currentIndex switch
                        {
                            0 => GameplayPlayerSettings.DifficultyLevel.Easy,
                            1 => GameplayPlayerSettings.DifficultyLevel.Normal,
                            2 => GameplayPlayerSettings.DifficultyLevel.Hard,
                            3 => GameplayPlayerSettings.DifficultyLevel.Extreme,
                            _ => GameplayPlayerSettings.Instance.GameDifficulty
                        };
                        SetInputMode(InputMode.None);
                        enabled = false;
                        DifficultyFade.Hide();
                    });
            }
            else
            {
                UiChoiceHandler.HandleNavigation(
                    action,
                    DifficultyUiManagers,
                    ref currentIndex,
                    4,
                    () =>
                    {
                        GameplayPlayerSettings.Instance.GameDifficulty = currentIndex switch
                        {
                            0 => GameplayPlayerSettings.DifficultyLevel.Easy,
                            1 => GameplayPlayerSettings.DifficultyLevel.Normal,
                            2 => GameplayPlayerSettings.DifficultyLevel.Hard,
                            3 => GameplayPlayerSettings.DifficultyLevel.Extreme,
                            _ => GameplayPlayerSettings.Instance.GameDifficulty
                        };
                        SetInputMode(InputMode.None);
                        enabled = false;
                        DifficultyFade.Hide();
                    });
            }
        }

        public void ShowStarGiftSelection() => SetInputMode(InputMode.StarGifts);

        private void HandleStarGiftInput(string action)
        {
            if (_currentInputMode != InputMode.StarGifts)
            {
                return;
            }

            StarGiftManager?.HandleInput(action);
        }

        private void OnStarGiftSelected(StarGift starGift)
        {
            selectedStarGift = starGift;
            CreateAndSaveAvatarInstance(starGift);

            SetInputMode(InputMode.None);

            $"Applied {starGift.name} star gift to avatar".LogInfo("GameStartManager");
        }
    }
}
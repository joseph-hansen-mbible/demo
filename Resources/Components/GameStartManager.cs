using Turnroot.Characters;
using Turnroot.Characters.Subclasses;
using Turnroot.Gameplay.Brain;
using Turnroot.Graphics2D;
using Turnroot.Utilities.AbstractScripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Turnroot.Demos
{
    public class GameStartManager : MonoBehaviour
    {
        public InputAction Select;
        public InputAction Back;
        public InputAction StartAction;
        public InputAction NavigateUp;
        public InputAction NavigateDown;
        public InputAction NavigateLeft;
        public InputAction NavigateRight;

        public UI.SaveFileUiManager[] SaveFileUiManagers;
        public UI.PronounsUiManager[] PronounsUiManagers;

        private int currentIndex = 0;

        private void OnEnable()
        {
            Select.Enable();
            Back.Enable();
            StartAction.Enable();
            NavigateUp.Enable();
            NavigateDown.Enable();
            NavigateLeft.Enable();
            NavigateRight.Enable();
        }

        private void OnDisable()
        {
            Select.Disable();
            Back.Disable();
            StartAction.Disable();
            NavigateUp.Disable();
            NavigateDown.Disable();
            NavigateLeft.Disable();
            NavigateRight.Disable();
        }

        private void Start()
        {
            Select.performed += ctx => HandleInput("Select");
            Back.performed += ctx => HandleInput("Back");
            StartAction.performed += ctx => HandleInput("Start");
            NavigateUp.performed += ctx => HandleInput("NavigateUp");
            NavigateDown.performed += ctx => HandleInput("NavigateDown");
            NavigateLeft.performed += ctx => HandleInput("NavigateLeft");
            NavigateRight.performed += ctx => HandleInput("NavigateRight");
            saveFileBrain = FindFirstObjectByType<SaveFileBrain>();
            InitializeSaveFiles();
        }

        public UIFade EntryFade;
        public UIFade SaveFilesFade;

        public UIFade PronounsFade;
        private SaveFileBrain saveFileBrain;
        public GameObject ScreenKeyboard;
        private ScreenKeyboard _keyboard;

        public string LastName = "Lastname";

        public AudioSource StartFx;

        public AudioSource UiFx;

        public AudioClip StartClip;

        public AudioClip NavigateClip;

        private bool _forwardInputToKeyboard = false;

        private bool _forwardInputToSaveFiles = false;

        private void InitializeSaveFiles()
        {

            var result = saveFileBrain.SaveFiles;
            for (int i = 0; i < SaveFileUiManagers.Length; i++)
            {
                if (i < result.Count)
                {
                    SaveFileUiManagers[i].UpdateSaveFileInfo(result[i]);
                    if ((int)saveFileBrain.ActiveSaveFileSubfolderPath == i)
                    {
                        SaveFileUiManagers[i].Select();
                        currentIndex = i;
                    }
                    else
                    {
                        SaveFileUiManagers[i].Deselect();
                    }
                }
                else
                {
                    SaveFileUiManagers[i].UpdateSaveFileInfo(new SaveFile());
                    SaveFileUiManagers[i].Deselect();
                    // select the first empty slot
                    SaveFileUiManagers[0].Select();
                }
            }

        }

        public void ReadyKeyboard()
        {
            ScreenKeyboard.GetComponent<UIFade>().Show();
            _keyboard = ScreenKeyboard.GetComponentInChildren<ScreenKeyboard>();
            _keyboard.OnSubmit = OnKeyboardSubmit;
            _forwardInputToKeyboard = true;
            _forwardInputToSaveFiles = false;
        }

        public void ReadyKeyboard(string defaultText)
        {
            ScreenKeyboard.GetComponent<UIFade>().Show();
            _keyboard = ScreenKeyboard.GetComponentInChildren<ScreenKeyboard>();
            _keyboard.OnSubmit = OnKeyboardSubmit;
            _keyboard.SetText(defaultText);
            _forwardInputToKeyboard = true;
            _forwardInputToSaveFiles = false;
        }

        private void OnKeyboardSubmit(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                saveFileBrain.Brain.PublishUpdateSaveFileName(text);
            }
            ScreenKeyboard.GetComponent<UIFade>().Hide();
            _forwardInputToKeyboard = false;
        }

        public CharacterData AvatarData;
        private bool _forwardInputToPronouns = false;

        private Pronouns selectedPronouns = new();

        public void ShowAvatarPronounSelection()
        {
            Debug.Log("Showing avatar pronoun selection");
            currentIndex = 0;
            _forwardInputToPronouns = true;
        }

        public void ChangeAvatarForm() { }

        private void HandleInput(string action)
        {
            if (_forwardInputToKeyboard && _keyboard != null)
            {
                _keyboard.HandleInput(action);
            }

            if (_forwardInputToSaveFiles)
            {
                HandleSaveFileInput(action);
            }

            if (_forwardInputToPronouns)
            {
                Debug.Log($"Handling pronouns input: {action}");
                HandlePronounsInput(action);
            }

            if (action is "Select" or "Start")
            {
                if (EntryFade.Visible)
                {
                    StartFx.PlayOneShot(StartClip);
                    EntryFade.Hide();

                    Debug.Log(saveFileBrain.SaveFiles.ToString());

                    if (saveFileBrain == null)
                    {
                        Debug.LogError("SaveFileBrain not found!");
                        return;
                    }

                    SaveFilesFade.Show();
                    _forwardInputToSaveFiles = true;
                }
            }
        }

        private void HandleSaveFileInput(string action)
        {
            // Deselect all
            foreach (var manager in SaveFileUiManagers)
            {
                manager.Deselect();
            }

            if (action is "NavigateUp" or "NavigateLeft")
            {
                UiFx.PlayOneShot(NavigateClip);
                currentIndex = (currentIndex - 1 + SaveFileUiManagers.Length) % SaveFileUiManagers.Length;
            }
            else if (action is "NavigateDown" or "NavigateRight")
            {
                UiFx.PlayOneShot(NavigateClip);
                currentIndex = (currentIndex + 1) % SaveFileUiManagers.Length;
            }
            else if (action == "Select")
            {
                UiFx.PlayOneShot(NavigateClip);

                if (currentIndex >= saveFileBrain.SaveFiles.Count)
                {
                    SaveFileSubfolders subfolderEnum = (SaveFileSubfolders)currentIndex;
                    saveFileBrain.CreateNewSaveFile(subfolderEnum);
                    currentIndex = saveFileBrain.SaveFiles.Count - 1; // Point to newly created save file
                    InitializeSaveFiles();
                }

                var selectedSaveFile = saveFileBrain.SaveFiles[currentIndex];
                
                // Set the active subfolder based on the selected save file
                if (System.Enum.TryParse<SaveFileSubfolders>(selectedSaveFile.LtmSubfolderPath, true, out var subfolder))
                {
                    saveFileBrain.ActiveSaveFileSubfolderPath = subfolder;
                    saveFileBrain.Brain.PublishLongTermMemorySubfolderSet(selectedSaveFile.LtmSubfolderPath);
                }

                if (selectedSaveFile.AvatarBodyType == AvatarBody.None ||
                    selectedSaveFile.AvatarPortrait == null ||
                    string.IsNullOrEmpty(selectedSaveFile.FileName) ||
                    selectedSaveFile.FileName == "Unnamed")
                {
                    SaveFilesFade.Hide();
                }
                else
                {
                    Debug.Log("TODO: Load existing save file");
                }

                return;
            }
            SaveFileUiManagers[currentIndex].Select();
        }

        private void HandlePronounsInput(string action)
        {
            foreach (var manager in PronounsUiManagers)
            {
                manager.Deselect();
            }

            if (action is "NavigateUp" or "NavigateLeft")
            {
                UiFx.PlayOneShot(NavigateClip);
                PronounsUiManagers[currentIndex].Deselect();
                currentIndex = (currentIndex - 1 + 3) % 3;
            }
            else if (action is "NavigateDown" or "NavigateRight")
            {
                UiFx.PlayOneShot(NavigateClip);
                PronounsUiManagers[currentIndex].Deselect();
                currentIndex = (currentIndex + 1) % 3;
            }
            else if (action == "Select")
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

                PronounsFade.Hide();
                _forwardInputToPronouns = false;
            }
            PronounsUiManagers[currentIndex].Select();

        }
    }
}

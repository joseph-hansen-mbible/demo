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
        private SaveFileBrain saveFileBrain;
        public GameObject ScreenKeyboard;
        private ScreenKeyboard _keyboard;

        public AudioSource UiSfx;

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

        private void ReadyKeyboard()
        {
            ScreenKeyboard.GetComponent<UIFade>().Show();
            _keyboard = ScreenKeyboard.GetComponentInChildren<ScreenKeyboard>();
            _forwardInputToKeyboard = true;
        }

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

            if (action is "Select" or "Start")
            {
                if (EntryFade.Visible)
                {
                    UiSfx.PlayOneShot(StartClip);
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

            if (action == "NavigateUp" || action == "NavigateLeft")
            {
                UiSfx.PlayOneShot(NavigateClip);
                currentIndex = (currentIndex - 1 + SaveFileUiManagers.Length) % SaveFileUiManagers.Length;
            }
            else if (action == "NavigateDown" || action == "NavigateRight")
            {
                UiSfx.PlayOneShot(NavigateClip);
                currentIndex = (currentIndex + 1) % SaveFileUiManagers.Length;
            }

            SaveFileUiManagers[currentIndex].Select();
        }
    }
}

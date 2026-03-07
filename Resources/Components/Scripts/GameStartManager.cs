using System;
using NaughtyAttributes;
using Turnroot.Characters;
using Turnroot.Characters.CharacterClass;
using Turnroot.Characters.Stats;
using Turnroot.Characters.Subclasses;
using Turnroot.Gameplay.Brain;
using Turnroot.Gameplay.Brain.Components;
using Turnroot.Graphics2D;
using Turnroot.Utilities;
using Turnroot.Utilities.AbstractScripts;
using Turnroot.Utilities.SceneFlows;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Turnroot.Demos
{
    public class GameStartManager : MonoBehaviour
    {
        private SceneFlowBrain sceneFlowBrain;
        public LoadingController loadingController;
        #region Input Actions

        [BoxGroup("Input Actions"), Tooltip("Input action for selecting items")]
        public InputAction Select;

        [BoxGroup("Input Actions")]
        public InputAction Back;

        [BoxGroup("Input Actions")]
        public InputAction StartAction;

        [BoxGroup("Input Actions")]
        public InputAction NavigateUp;

        [BoxGroup("Input Actions")]
        public InputAction NavigateDown;

        [BoxGroup("Input Actions")]
        public InputAction NavigateLeft;

        [BoxGroup("Input Actions")]
        public InputAction NavigateRight;

        #endregion

        #region UI Managers

        [BoxGroup("UI Managers"), HorizontalLine(color: EColor.Blue)]
        public UI.SaveFileUiManager[] SaveFileUiManagers;

        [BoxGroup("UI Managers")]
        public UI.PronounsUiManager[] PronounsUiManagers;

        #endregion

        #region UI Fades & Screens

        [BoxGroup("UI Fades"), HorizontalLine(color: EColor.Indigo)]
        public UIFade EntryFade;

        [BoxGroup("UI Fades")]
        public UIFade SaveFilesFade;

        [BoxGroup("UI Fades")]
        public UIFade PronounsFade;

        [BoxGroup("UI Fades")]
        public UIFade StarGiftsFade;

        [BoxGroup("UI Fades")]
        public UIFade LoadingFade;

        [BoxGroup("UI Fades")]
        public UiFillDriver LoadingFillDriver;

        [BoxGroup("UI Fades")]
        public GameObject ScreenKeyboard;

        #endregion

        #region Audio

        [BoxGroup("Audio"), HorizontalLine(color: EColor.Orange)]
        public AudioSource StartFx;

        [BoxGroup("Audio")]
        public AudioSource UiFx;

        [BoxGroup("Audio")]
        public AudioClip StartClip;

        [BoxGroup("Audio")]
        public AudioClip NavigateClip;

        #endregion

        #region Avatar & Character Data

        [BoxGroup("Avatar"), HorizontalLine(color: EColor.Green)]
        public CharacterData AvatarData;

        [BoxGroup("Avatar")]
        public StarGiftManager StarGiftManager;

        [BoxGroup("Avatar")]
        public string LastName = "Lastname";

        #endregion

        #region Private State

        private SaveFileBrain saveFileBrain;
        private ScreenKeyboard _keyboard;
        private int currentIndex = 0;

        private bool _forwardInputToKeyboard = false;
        private bool _forwardInputToSaveFiles = false;
        private bool _forwardInputToPronouns = false;
        private bool _forwardInputToStarGifts = false;

        private Pronouns selectedPronouns = new();
        private StarGift selectedStarGift;

        #endregion

        #region Unity Lifecycle

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
            sceneFlowBrain = saveFileBrain.Brain.sceneFlowBrain;
            loadingController = saveFileBrain.Brain.GetComponent<LoadingController>();
            
            sceneFlowBrain.SetCurrentScene("scene_0");
            
            InitializeSaveFiles();

            // Subscribe to StarGift selection event
            StarGiftManager.OnStarGiftSelected.AddListener(OnStarGiftSelected);
        }

        #endregion

        #region Input Handling

        private void HandleInput(string action)
        {
            if (_forwardInputToKeyboard && _keyboard != null)
            {
                _keyboard.HandleInput(action);
                return;
            }

            if (_forwardInputToSaveFiles)
            {
                HandleSaveFileInput(action);
                return;
            }

            if (_forwardInputToPronouns)
            {
                HandlePronounsInput(action);
                return;
            }

            if (_forwardInputToStarGifts)
            {
                HandleStarGiftInput(action);
                return;
            }

            if (action == "Select" || action == "Start")
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
                    _forwardInputToSaveFiles = true;
                }
            }
        }

        #endregion

        #region Save File Management

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
                    SaveFileUiManagers[0].Select();
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
                UiFx.PlayOneShot(NavigateClip);
                currentIndex = (currentIndex - 1 + SaveFileUiManagers.Length) % SaveFileUiManagers.Length;
            }
            else if (action == "NavigateDown" || action == "NavigateRight")
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
                    currentIndex = saveFileBrain.SaveFiles.Count - 1;
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
                    "Load existing save file".LogInfo("GameStartManager");
                }

                return;
            }
            SaveFileUiManagers[currentIndex].Select();
        }

        #endregion

        #region Keyboard Input

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

        #endregion

        #region Pronouns Selection

        public void ShowAvatarPronounSelection()
        {
            "Showing avatar pronoun selection".LogInfo("GameStartManager");
            
            // Disable all other input forwarding
            _forwardInputToKeyboard = false;
            _forwardInputToSaveFiles = false;
            _forwardInputToStarGifts = false;
            
            _forwardInputToPronouns = true;
            currentIndex = 0;
        }

        private void HandlePronounsInput(string action)
        {
            foreach (var manager in PronounsUiManagers)
            {
                manager.Deselect();
            }

            if (action == "NavigateUp" || action == "NavigateLeft")
            {
                UiFx.PlayOneShot(NavigateClip);
                PronounsUiManagers[currentIndex].Deselect();
                currentIndex = (currentIndex - 1 + 3) % 3;
            }
            else if (action == "NavigateDown" || action == "NavigateRight")
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

                _forwardInputToPronouns = false;
                PronounsFade.Hide();
                return;
            }
            PronounsUiManagers[currentIndex].Select();
        }

        #endregion

        #region StarGift Selection

        public void ShowStarGiftSelection()
        {
            // Disable all other input forwarding
            _forwardInputToKeyboard = false;
            _forwardInputToSaveFiles = false;
            _forwardInputToPronouns = false;
            
            _forwardInputToStarGifts = true;
            currentIndex = 0;
        }

        private void HandleStarGiftInput(string action) => StarGiftManager?.HandleInput(action);

        private void OnStarGiftSelected(StarGift starGift)
        {
            selectedStarGift = starGift;
            CreateAndSaveAvatarInstance(starGift);

            _forwardInputToStarGifts = false;

            $"Applied {starGift.name} star gift to avatar".LogInfo("GameStartManager");
        }

        #endregion

        #region Avatar Creation & Persistence

        private void CreateAndSaveAvatarInstance(StarGift starGift)
        {
            if (AvatarData == null)
            {
                "AvatarData is null!".LogError("GameStartManager");
                return;
            }

            if (saveFileBrain == null)
            {
                "SaveFileBrain is null!".LogError("GameStartManager");
                return;
            }

            // Get the LongTermMemory component and create factory
            var ltm = saveFileBrain.Brain.GetComponent<LongTermMemory>();
            if (ltm == null)
            {
                "LongTermMemory component not found!".LogError("GameStartManager");
                return;
            }

            var factory = new CharacterFactory(ltm);
            var persistence = new CharacterPersistence(saveFileBrain.Brain);

            // Create the avatar instance from the template
            var avatarInstance = factory.CreateOrRecall(AvatarData);
            if (avatarInstance == null)
            {
                "Failed to create avatar instance!".LogError("GameStartManager");
                return;
            }

            // Set the base stats on the instance
            avatarInstance.GetUnboundedStat(UnboundedStatType.Strength)?.SetCurrent(starGift.strength);
            avatarInstance.GetUnboundedStat(UnboundedStatType.Skill)?.SetCurrent(starGift.skill);
            avatarInstance.GetUnboundedStat(UnboundedStatType.Defense)?.SetCurrent(starGift.defense);
            avatarInstance.GetUnboundedStat(UnboundedStatType.Magic)?.SetCurrent(starGift.magic);
            avatarInstance.GetUnboundedStat(UnboundedStatType.Resistance)?.SetCurrent(starGift.resistance);
            avatarInstance.GetUnboundedStat(UnboundedStatType.Speed)?.SetCurrent(starGift.speed);
            avatarInstance.GetUnboundedStat(UnboundedStatType.Luck)?.SetCurrent(starGift.luck);
            avatarInstance.GetUnboundedStat(UnboundedStatType.Dexterity)?.SetCurrent(starGift.dexterity);

            // Set growth rates on the template (runtime only, won't persist to disk)
            AvatarData.PersonalGrowthRates.Clear();
            AvatarData.PersonalGrowthRates.Add(new UnboundedStatModifier(UnboundedStatType.Strength, starGift.strengthGrowth));
            AvatarData.PersonalGrowthRates.Add(new UnboundedStatModifier(UnboundedStatType.Skill, starGift.skillGrowth));
            AvatarData.PersonalGrowthRates.Add(new UnboundedStatModifier(UnboundedStatType.Defense, starGift.defenseGrowth));
            AvatarData.PersonalGrowthRates.Add(new UnboundedStatModifier(UnboundedStatType.Magic, starGift.magicGrowth));
            AvatarData.PersonalGrowthRates.Add(new UnboundedStatModifier(UnboundedStatType.Resistance, starGift.resistanceGrowth));
            AvatarData.PersonalGrowthRates.Add(new UnboundedStatModifier(UnboundedStatType.Speed, starGift.speedGrowth));
            AvatarData.PersonalGrowthRates.Add(new UnboundedStatModifier(UnboundedStatType.Luck, starGift.luckGrowth));
            AvatarData.PersonalGrowthRates.Add(new UnboundedStatModifier(UnboundedStatType.Dexterity, starGift.dexterityGrowth));
            AvatarData.PersonalGrowthRates.Add(new UnboundedStatModifier(BoundedStatType.Health, 85f));

            // Save the avatar instance to LongTermMemory
            persistence.SaveCharacter(avatarInstance, updateIndex: true);

            $"Saved avatar instance with {starGift.name} stats to LongTermMemory".LogInfo("GameStartManager");
        }

        #endregion

        #region Move to Next Scene

        public void StartLoadingNextScene()
        {
            // Show loading screen
            LoadingFade.Show();

            var availableScenes = sceneFlowBrain.GetAvailableScenes();
            
            if (availableScenes == null || availableScenes.Count == 0)
            {
                "No available scenes to transition to!".LogError("GameStartManager");
                LoadingFade.Hide();
                return;
            }

            var nextScene = availableScenes[0];
            
            $"Starting transition to scene: {nextScene.displayName}".LogInfo("GameStartManager");
            
            // Trigger the scene transition via SceneFlowBrain
            // This will load the scene and publish progress events that DynamicSceneFlow can track
            sceneFlowBrain.TransitionToScene(nextScene.sceneId);
        }
        
        public void CheckLoadingProgress(float progress)
        {
            LoadingFillDriver.SetAmount(progress);
            
            if (progress >= 1f)
            {
                MoveToNextSceneAndUnloadThisOne();
            }
        }
        
        public void MoveToNextSceneAndUnloadThisOne()
        {
            // Hide the loading screen
            LoadingFade.Hide();
            
            // The scene has already been loaded and transitioned by SceneFlowBrain.TransitionToScene()
            // This just hides the UI and lets the new scene take over
            $"Scene transition complete".LogInfo("GameStartManager");
        }
        #endregion


    }
}

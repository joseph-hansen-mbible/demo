using System;
using NaughtyAttributes;
using Turnroot.Characters;
using Turnroot.Characters.CharacterClass;
using Turnroot.Characters.Stats;
using Turnroot.Characters.Subclasses;
using Turnroot.Gameplay.Brain;
using Turnroot.Gameplay.Brain.Components;
using Turnroot.Gameplay.PlayerSettings;
using Turnroot.Graphics2D;
using Turnroot.UI;
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

        [HideInInspector]
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
        public UiChoiceWithScaleAndEffect[] PronounsUiManagers;

        [BoxGroup("UI Managers")]
        public UiChoiceWithScaleAndEffect[] DifficultyUiManagers;

        [BoxGroup("UI Managers")]
        public UiChoiceWithScaleAndEffect[] PermadeathUiManagers;

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
        public UIFade DifficultyFade;

        [BoxGroup("UI Fades")]
        public UIFade PermadeathFade;

        [BoxGroup("UI Fades")]
        public UiFillDriver LoadingFillDriver;

        [BoxGroup("UI Fades")]
        public GameObject ScreenKeyboard;

        [BoxGroup("UI Fades")]
        public GameObject NumberDateKeyboard;

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

        private enum InputMode { None, Keyboard, SaveFiles, Pronouns, StarGifts, Date, Difficulty, Permadeath }
        private SaveFileBrain saveFileBrain;
        private ScreenKeyboard _keyboard;

        private ScreenKeyboard _dateKeyboard;
        private int currentIndex = 0;
        private InputMode _currentInputMode = InputMode.None;
        private InputAction[] _allInputActions;
        private Pronouns selectedPronouns = new();
        private StarGift selectedStarGift;
        private int selectedBirthdayDay = 1;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            _allInputActions = new[] { Select, Back, StartAction, NavigateUp, NavigateDown, NavigateLeft, NavigateRight };
            foreach (var action in _allInputActions)
            {
                action.Enable();
            }
        }

        private void OnDisable()
        {
            foreach (var action in _allInputActions)
            {
                action.Disable();
            }
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
            saveFileBrain.Brain.OnSceneReadyToDisplay += HandleSceneReadyToDisplay;
            sceneFlowBrain.SetCurrentScene("scene_0");
            InitializeSaveFiles();
            StarGiftManager.OnStarGiftSelected.AddListener(OnStarGiftSelected);
        }

        private void OnDestroy()
        {
            if (saveFileBrain?.Brain != null)
            {
                saveFileBrain.Brain.OnSceneReadyToDisplay -= HandleSceneReadyToDisplay;
            }

            if (StarGiftManager != null)
            {
                StarGiftManager.OnStarGiftSelected.RemoveListener(OnStarGiftSelected);
            }
        }

        #endregion

        #region Input Handling

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
                    SetInputMode(InputMode.SaveFiles);
                }
            }
        }

        private void SetInputMode(InputMode mode)
        {
            _currentInputMode = mode;
            currentIndex = 0;
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
            UiChoiceHandler.HandleNavigation(
                action,
                SaveFileUiManagers,
                ref currentIndex,
                SaveFileUiManagers.Length,
                () =>
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
                },
                UiFx,
                NavigateClip);
        }

        #endregion

        #region Keyboard Input

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

        #endregion

        #region Pronouns Selection

        public void ShowAvatarPronounSelection() => SetInputMode(InputMode.Pronouns);

        public void ShowDifficultySelection() => SetInputMode(InputMode.Difficulty);

        public void ShowPermadeathSelection() => SetInputMode(InputMode.Permadeath);

        private void HandlePermadeathInput(string action)
        {
            UiChoiceHandler.HandleNavigation(
                action,
                PermadeathUiManagers,
                ref currentIndex,
                2,
                () =>
                {
                    GameplayPlayerSettings.Instance.Permadeath = currentIndex == 0;
                    PermadeathFade.Hide();
                    SetInputMode(InputMode.None);
                },
                UiFx,
                NavigateClip);
        }

        private void HandlePronounsInput(string action)
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
                    PronounsFade.Hide();
                },
                UiFx,
                NavigateClip);
        }

        private void HandleDifficultyInput(string action)
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
                    DifficultyFade.Hide();
                    SetInputMode(InputMode.None);
                },
                UiFx,
                NavigateClip);
        }

        #endregion

        #region StarGift Selection

        public void ShowStarGiftSelection() => SetInputMode(InputMode.StarGifts);

        private void HandleStarGiftInput(string action) => StarGiftManager?.HandleInput(action);

        private void OnStarGiftSelected(StarGift starGift)
        {
            selectedStarGift = starGift;
            CreateAndSaveAvatarInstance(starGift);

            SetInputMode(InputMode.None);

            $"Applied {starGift.name} star gift to avatar".LogInfo("GameStartManager");
        }

        #endregion

        #region Avatar Creation & Persistence

        private void CreateAndSaveAvatarInstance(StarGift starGift)
        {
            _ = OperationResultGuards.All(
                OperationResultGuards.RequireNotNull(AvatarData, nameof(AvatarData)),
                OperationResultGuards.RequireNotNull(saveFileBrain, nameof(saveFileBrain))
            );

            var ltm = saveFileBrain.Brain.GetComponent<LongTermMemory>();
            _ = OperationResultGuards.RequireNotNull(ltm, "LongTermMemory component");

            var factory = new CharacterFactory(ltm);
            var persistence = new CharacterPersistence(saveFileBrain.Brain);

            var avatarInstance = factory.CreateOrRecall(AvatarData);
            _ = OperationResultGuards.RequireNotNull(avatarInstance, "avatar instance");

            ApplyStarGiftStatsToInstance(avatarInstance, starGift);
            ApplyStarGiftGrowthRates(starGift);
            
            // Store birthday using stargift index as month and selected day as date
            int birthdayMonth = Mathf.Clamp(starGift.index, 1, 12);
            int birthdayDay = Mathf.Clamp(selectedBirthdayDay, 1, 31);
            string birthdayKey = $"Avatar/Birthday";
            string birthdayValue = $"{birthdayMonth}/{birthdayDay}";
            ltm.Remember(birthdayKey, birthdayValue);
            $"Set avatar birthday to month {birthdayMonth}, day {birthdayDay}".LogInfo("GameStartManager");
            
            persistence.SaveCharacter(avatarInstance, updateIndex: true);

            $"Saved avatar instance with {starGift.name} stats to LongTermMemory".LogInfo("GameStartManager");
        }

        private void ApplyStarGiftStatsToInstance(CharacterInstance instance, StarGift gift)
        {
            var statMappings = new (UnboundedStatType type, int value)[]
            {
                (UnboundedStatType.Strength, gift.strength),
                (UnboundedStatType.Skill, gift.skill),
                (UnboundedStatType.Defense, gift.defense),
                (UnboundedStatType.Magic, gift.magic),
                (UnboundedStatType.Resistance, gift.resistance),
                (UnboundedStatType.Speed, gift.speed),
                (UnboundedStatType.Luck, gift.luck),
                (UnboundedStatType.Dexterity, gift.dexterity)
            };

            foreach (var (type, value) in statMappings)
            {
                instance.GetUnboundedStat(type)?.SetCurrent(value);
            }
        }

        private void ApplyStarGiftGrowthRates(StarGift gift)
        {
            AvatarData.PersonalGrowthRates.Clear();
            var growthMappings = new (UnboundedStatType type, int growth)[]
            {
                (UnboundedStatType.Strength, gift.strengthGrowth),
                (UnboundedStatType.Skill, gift.skillGrowth),
                (UnboundedStatType.Defense, gift.defenseGrowth),
                (UnboundedStatType.Magic, gift.magicGrowth),
                (UnboundedStatType.Resistance, gift.resistanceGrowth),
                (UnboundedStatType.Speed, gift.speedGrowth),
                (UnboundedStatType.Luck, gift.luckGrowth),
                (UnboundedStatType.Dexterity, gift.dexterityGrowth)
            };

            foreach (var (type, growth) in growthMappings)
            {
                AvatarData.PersonalGrowthRates.Add(new UnboundedStatModifier(type, growth));
            }

            AvatarData.PersonalGrowthRates.Add(new UnboundedStatModifier(BoundedStatType.Health, 85f));
        }

        #endregion

        #region Move to Next Scene
        public void StartLoadingNextScene()
        {
            LoadingFade.Show();

            var availableScenes = sceneFlowBrain.GetAvailableScenes();
            if (availableScenes == null || availableScenes.Count == 0)
            {
                "No available scenes to transition to!".LogError("GameStartManager");
                LoadingFade.Hide();
                return;
            }

            var nextScene = availableScenes[0];
            sceneFlowBrain.TransitionToScene(nextScene.sceneId);
        }

        public void CheckLoadingProgress(float progress) => LoadingFillDriver.SetAmount(progress);

        private void HandleSceneReadyToDisplay(string sceneName, string displayName) =>
            // Scene is ready to display - hide the loading screen
            MoveToNextSceneAndUnloadThisOne();

        public void MoveToNextSceneAndUnloadThisOne() => LoadingFade.Hide();
        #endregion


    }
}

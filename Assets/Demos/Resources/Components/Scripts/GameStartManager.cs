using NaughtyAttributes;
using Turnroot.Characters;
using Turnroot.Characters.Subclasses;
using Turnroot.Gameplay.Brain;
using Turnroot.Graphics2D;
using Turnroot.UI;
using Turnroot.Utilities;
using Turnroot.Utilities.AbstractScripts;
using Turnroot.Utilities.SceneFlows;
using UnityEngine;
using UnityEngine.Events;

namespace Turnroot.Demos
{
    [RequireComponent(typeof(UiInputProvider))]
    /// <remarks>
    /// This may need editing for your project, but if you aren't making major logic changes, you should 
    /// be able to wrangle it to work for you just with UI changes and inspector stuff
    /// </remarks>
    public partial class GameStartManager : MonoBehaviour
    {
        private SceneFlowBrain sceneFlowBrain;

        [BoxGroup("Input")]
        public UiInputProvider InputProvider;

        [HideInInspector]
        public LoadingController loadingController;

        [BoxGroup("Loading")]
        [Tooltip("Optional shared loading screen controller that can be reused across scenes.")]
        public LoadingScreenController LoadingScreen;

        public AudioClip TitleMusic;

        #region UI Managers

        [BoxGroup("UI Managers"), HorizontalLine(color: EColor.Blue)]
        public UI.SaveFileUiManager[] SaveFileUiManagers;

        [BoxGroup("UI Managers")]
        public UiChoice[] PronounsUiManagers;

        [BoxGroup("UI Managers")]
        public UiChoice[] DifficultyUiManagers;

        [BoxGroup("UI Managers")]
        public UiChoice[] PermadeathUiManagers;

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
        public AudioClip StartClip;

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

        private ScreenKeyboard _dateKeyboard;
        private int currentIndex = 0;
        private InputMode _currentInputMode = InputMode.None;
        private Pronouns selectedPronouns = new();
        private StarGift selectedStarGift;
        private int selectedBirthdayDay = 1;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (InputProvider != null)
            {
                InputProvider.OnInput += HandleInput;
            }
            else
            {
                "GameStartManager: InputProvider reference is null".LogWarning("GameStartManager");
            }

            saveFileBrain = FindFirstObjectByType<SaveFileBrain>();

            saveFileBrain.LoadSaveFiles();

            sceneFlowBrain = saveFileBrain.Brain.sceneFlowBrain;
            loadingController = saveFileBrain.Brain.GetComponent<LoadingController>();

            if (LoadingScreen == null)
            {
                LoadingScreen = FindFirstObjectByType<LoadingScreenController>();
            }

            // If the project uses the legacy loading UI (fade + fill driver) but doesn't
            // have a dedicated controller, create one at runtime to keep behavior consistent
            if (LoadingScreen == null && (LoadingFade != null || LoadingFillDriver != null))
            {
                LoadingScreen = gameObject.AddComponent<LoadingScreenController>();
                LoadingScreen.Fade = LoadingFade;
                LoadingScreen.FillDriver = LoadingFillDriver;
            }

            saveFileBrain.Brain.OnSceneReadyToDisplay += HandleSceneReadyToDisplay;
            sceneFlowBrain.SetCurrentScene("scene_0");
            InitializeSaveFiles();

            // keep the save file list in sync when the filename changes while the
            // user is still on the title screen
            // update UI when the filename changes; preserve index if user is actively navigating
            saveFileBrain.Brain.OnUpdateSaveFileName += _ =>
            {
                if (_currentInputMode == InputMode.SaveFiles)
                {
                    InitializeSaveFiles(preserveIndex: true);
                }
                else
                {
                    InitializeSaveFiles();
                }
            };

            saveFileBrain.OnActiveSaveFilePlaytimeUpdated += _ =>
            {
                if (_currentInputMode == InputMode.SaveFiles)
                {
                    InitializeSaveFiles(preserveIndex: true);
                }
                else
                {
                    InitializeSaveFiles();
                }
            };

            StarGiftManager.OnStarGiftSelected.AddListener(OnStarGiftSelected);

            saveFileBrain.Brain.audioBrain.SetMusic(TitleMusic);
        }

        private void OnDestroy()
        {
            if (InputProvider != null)
            {
                InputProvider.OnInput -= HandleInput;
            }

            if (saveFileBrain.Brain != null)
            {
                saveFileBrain.Brain.OnSceneReadyToDisplay -= HandleSceneReadyToDisplay;
            }

            if (saveFileBrain != null)
            {
                saveFileBrain.OnActiveSaveFilePlaytimeUpdated -= _ => InitializeSaveFiles();
            }

            if (StarGiftManager != null)
            {
                StarGiftManager.OnStarGiftSelected.RemoveListener(OnStarGiftSelected);
            }
        }

        #endregion

        #region Move to Next Scene

        public UnityEvent OnStartLoadingNextScene = new();
        public void StartLoadingNextScene()
        {
            // once we start the transition, this manager should stop processing input
            enabled = false;

            OnStartLoadingNextScene.Invoke();
            if (LoadingScreen != null)
            {
                LoadingScreen.Show();
            }
            else
            {
                LoadingFade.Show();
            }

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

        public void CheckLoadingProgress(float progress)
        {
            if (LoadingScreen != null)
            {
                LoadingScreen.SetProgress(progress);
            }
            else
            {
                LoadingFillDriver?.SetAmount(progress);
            }
        }

        private void HandleSceneReadyToDisplay(string sceneName, string displayName) =>
            MoveToNextSceneAndUnloadThisOne();

        public void MoveToNextSceneAndUnloadThisOne() => LoadingFade.Hide();
        #endregion
    }
}

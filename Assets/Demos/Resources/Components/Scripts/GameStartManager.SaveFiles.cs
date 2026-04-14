using UnityEngine;
using Turnroot.Utilities;
using Turnroot.Gameplay.Brain;

namespace Turnroot.Demos
{
    public partial class GameStartManager
    {
        private void InitializeSaveFiles(bool preserveIndex = false)
        {
            int remembered = currentIndex;
            var result = saveFileBrain.SaveFiles;
            for (int i = 0; i < SaveFileUiManagers.Length; i++)
            {
                var manager = SaveFileUiManagers[i];
                if (manager == null)
                {
                    continue; // skip destroyed slots
                }

                if (i < result.Count)
                {
                    manager.UpdateSaveFileInfo(result[i]);
                    if ((int)saveFileBrain.ActiveSaveFileSubfolderPath == i && !preserveIndex)
                    {
                        manager.Select();
                        currentIndex = i;
                    }
                    else
                    {
                        manager.Deselect();
                    }
                }
                else
                {
                    manager.UpdateSaveFileInfo(new SaveFile());
                    manager.Deselect();
                    if (SaveFileUiManagers[0] != null && !preserveIndex)
                    {
                        SaveFileUiManagers[0].Select();
                    }
                }
            }

            if (preserveIndex && _currentInputMode == InputMode.SaveFiles)
            {
                // restore previous index, clamped to available range
                currentIndex = Mathf.Clamp(remembered, 0, SaveFileUiManagers.Length - 1);
                UpdateSaveFileSelectionVisual();
            }
        }

        private void UpdateSaveFileSelectionVisual()
        {
            for (int i = 0; i < SaveFileUiManagers.Length; i++)
            {
                var mgr = SaveFileUiManagers[i];
                if (mgr == null)
                {
                    continue;
                }

                if (i == currentIndex)
                {
                    mgr.Select();
                }
                else
                {
                    mgr.Deselect();
                }
            }
        }

        private void HandleSaveFileInput(string action)
        {
            // ignore stray input once we've left the save-files state
            if (_currentInputMode != InputMode.SaveFiles || !enabled)
            {
                return;
            }

            if (InputProvider != null)
            {
                InputProvider.Navigate(
                    action,
                    SaveFileUiManagers,
                    ref currentIndex,
                    SaveFileUiManagers.Length,
                    () =>
                    {
                        // navigation sound played by InputProvider component

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

                        // TODO: once portrait/body type selection is implemented in this flow,
                        // we should validate them here. For now drop those checks so that
                        // a new save can be written when `CreateAndSaveAvatarInstance` runs.
                        //
                        // once a slot is chosen we no longer accept more navigation input
                        // – canceling the mode prevents further calls to Navigate after the
                        // UI disappears, which was crashing when users mashed Select.
                        SetInputMode(InputMode.None);
                        // disable this manager like StartLoadingNextScene does; avoids any
                        // stray input while fade/scene transition occurs
                        enabled = false;

                        // Always hide the save selection when one is chosen; further progression
                        // (either creating a new avatar or loading an existing one) happens
                        // in later steps.
                        SaveFilesFade.Hide();

                        // If the slot already contains a valid name (i.e. not the default
                        // "Unnamed"), treat it as an existing save and jump straight to the
                        // next scene instead of continuing character creation.
                        if (!string.IsNullOrEmpty(selectedSaveFile.FileName) &&
                            selectedSaveFile.FileName != "Unnamed")
                        {
                            "Load existing save file".LogInfo("GameStartManager");
                            StartLoadingNextScene();
                        }
                    });
            }
        }
    }
}
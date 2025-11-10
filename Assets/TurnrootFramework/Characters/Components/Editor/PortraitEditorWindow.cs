using System.Linq;
using Turnroot.Graphics2D.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Turnroot.Characters.Subclasses.Editor
{
    public class PortraitEditorWindow : StackedImageEditorWindow<CharacterData, Portrait>
    {
        protected override string WindowTitle => "Portrait Editor";
        protected override string OwnerFieldLabel => "Character";

        private ReorderableList _layersReorderList;
        private Turnroot.Graphics.Portrait.ImageStack _lastListImageStack;
        private SerializedObject _layersSerializedObject;

        [MenuItem("/Turnroot/Editors/Portrait Editor")]
        public static void ShowWindow() => GetWindow<PortraitEditorWindow>("Portrait Editor");

        public static void OpenPortrait(CharacterData character, string portraitKey)
        {
            var window = GetWindow<PortraitEditorWindow>("Portrait Editor");
            window._currentOwner = character;
            window._selectedImageIndex = 0;
            window.UpdateCurrentImage();
            window.RefreshPreview();
        }

        protected override void OnGUI()
        {
            EditorGUILayout.LabelField($"Live {WindowTitle}", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            // Owner selection
            EditorGUI.BeginChangeCheck();
            _currentOwner =
                EditorGUILayout.ObjectField(
                    OwnerFieldLabel,
                    _currentOwner,
                    typeof(CharacterData),
                    false
                ) as CharacterData;
            if (EditorGUI.EndChangeCheck())
            {
                _selectedImageIndex = 0;
                UpdateCurrentImage();
            }

            if (_currentOwner == null)
            {
                EditorGUILayout.HelpBox(
                    $"Select a {OwnerFieldLabel} to edit their portraits.",
                    MessageType.Info
                );
                return;
            }

            var portraitsDict = _currentOwner.Portraits;
            if (portraitsDict == null || portraitsDict.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    $"This {OwnerFieldLabel} has no portraits. Add portraits first.",
                    MessageType.Warning
                );
                return;
            }

            var keys = portraitsDict.Keys.ToArray();
            string[] portraitNames = keys.Select(k => k).ToArray();

            int newIndex = EditorGUILayout.Popup(
                "Select Portrait",
                _selectedImageIndex,
                portraitNames
            );
            if (newIndex != _selectedImageIndex)
            {
                _selectedImageIndex = newIndex;
                var arr = _currentOwner.PortraitArray;
                _currentImage =
                    (arr != null && _selectedImageIndex < arr.Length)
                        ? arr[_selectedImageIndex]
                        : null;
                RefreshPreview();
            }

            if (_currentImage == null)
            {
                EditorGUILayout.HelpBox(
                    $"No Portrait asset for key '{keys[_selectedImageIndex]}'.",
                    MessageType.Info
                );
                if (GUILayout.Button("Create Portrait for this key"))
                {
                    var p = new Portrait();
                    p.SetOwner(_currentOwner);
                    p.SetKey(keys[_selectedImageIndex]);
                    _currentOwner.Portraits[keys[_selectedImageIndex]] = p;
                    _currentOwner.InvalidatePortraitArrayCache();
                    EditorUtility.SetDirty(_currentOwner);
                    var arr = _currentOwner.PortraitArray;
                    _currentImage =
                        (arr != null && _selectedImageIndex < arr.Length)
                            ? arr[_selectedImageIndex]
                            : null;
                    RefreshPreview();
                }

                return;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            // Left: layers list
            EditorGUILayout.BeginVertical(GUILayout.Width(600));
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            var imageStack = _currentImage.ImageStack;
            if (imageStack == null)
            {
                EditorGUILayout.HelpBox(
                    "No ImageStack assigned. Use the right column to create or assign one.",
                    MessageType.Info
                );
            }
            else
            {
                EnsureLayersReorderList(imageStack);

                if (_layersReorderList != null)
                {
                    _layersSerializedObject?.Update();
                    _layersReorderList.DoLayoutList();

                    if (
                        _layersSerializedObject != null
                        && _layersSerializedObject.ApplyModifiedProperties()
                    )
                    {
                        EditorUtility.SetDirty(imageStack);
                        if (_autoRefresh)
                            RefreshPreview();
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // Right: assign, preview, metadata, tinting
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(420));
            DrawImageStackSection();
            EditorGUILayout.Space(8);
            DrawPreviewPanel();
            EditorGUILayout.Space(10);
            DrawImageMetadataSection();
            EditorGUILayout.Space(8);
            DrawTintingSection();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void EnsureLayersReorderList(Turnroot.Graphics.Portrait.ImageStack imageStack)
        {
            if (imageStack == null)
            {
                _layersReorderList = null;
                _layersSerializedObject = null;
                _lastListImageStack = null;
                return;
            }

            if (_lastListImageStack == imageStack && _layersReorderList != null)
                return;

            _layersSerializedObject = new SerializedObject(imageStack);
            var layersProp = _layersSerializedObject.FindProperty("_layers");

            _layersReorderList = new ReorderableList(
                _layersSerializedObject,
                layersProp,
                true,
                true,
                true,
                true
            );
            _layersReorderList.drawHeaderCallback = rect =>
                EditorGUI.LabelField(rect, $"Layers ({layersProp.arraySize})");
            _layersReorderList.elementHeightCallback = index =>
                EditorGUI.GetPropertyHeight(layersProp.GetArrayElementAtIndex(index), true) + 12;
            _layersReorderList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var el = layersProp.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(el, true)),
                    el,
                    new GUIContent($"Layer {index}"),
                    true
                );
            };

            var stackRef = imageStack;

            _layersReorderList.onAddCallback = list =>
            {
                layersProp.arraySize++;
                int lastIndex = layersProp.arraySize - 1;
                var newEl = layersProp.GetArrayElementAtIndex(lastIndex);
                if (newEl != null)
                {
                    var spriteProp = newEl.FindPropertyRelative("Sprite");
                    var maskProp = newEl.FindPropertyRelative("Mask");
                    var offsetProp = newEl.FindPropertyRelative("Offset");
                    var scaleProp = newEl.FindPropertyRelative("Scale");
                    var rotationProp = newEl.FindPropertyRelative("Rotation");
                    var orderProp = newEl.FindPropertyRelative("Order");

                    if (spriteProp != null)
                        spriteProp.objectReferenceValue = null;
                    if (maskProp != null)
                        maskProp.objectReferenceValue = null;
                    if (offsetProp != null)
                        offsetProp.vector2Value = Vector2.zero;
                    if (scaleProp != null)
                        scaleProp.floatValue = 1f;
                    if (rotationProp != null)
                        rotationProp.floatValue = 0f;
                    if (orderProp != null)
                        orderProp.intValue = (layersProp.arraySize - 1) - lastIndex;
                }

                _layersSerializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(stackRef);
                if (_autoRefresh)
                    RefreshPreview();
            };

            _layersReorderList.onRemoveCallback = list =>
            {
                int removeIndex = list.index;
                layersProp.DeleteArrayElementAtIndex(removeIndex);
                _layersSerializedObject.ApplyModifiedProperties();

                for (int i = 0; i < layersProp.arraySize; i++)
                {
                    var el = layersProp.GetArrayElementAtIndex(i);
                    var orderProp = el.FindPropertyRelative("Order");
                    if (orderProp != null)
                        orderProp.intValue = (layersProp.arraySize - 1) - i;
                }

                _layersSerializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(stackRef);
                if (_autoRefresh)
                    RefreshPreview();
            };

            _layersReorderList.onChangedCallback = list =>
            {
                for (int i = 0; i < layersProp.arraySize; i++)
                {
                    var el = layersProp.GetArrayElementAtIndex(i);
                    var orderProp = el.FindPropertyRelative("Order");
                    if (orderProp != null)
                        orderProp.intValue = (layersProp.arraySize - 1) - i;
                }

                _layersSerializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(stackRef);
                if (_autoRefresh)
                    RefreshPreview();
            };

            _lastListImageStack = imageStack;
        }

        protected override Portrait[] GetImagesFromOwner(CharacterData owner)
        {
            if (owner?.Portraits == null)
                return null;

            var arr = owner?.PortraitArray;
            if (arr == null || arr.Length == 0)
                return null;

            var nonNull = arr.Where(p => p != null).ToArray();
            if (nonNull.Length == 0)
                return null;

            foreach (var p in nonNull)
                p.SetOwner(owner);
            return nonNull;
        }
    }
}

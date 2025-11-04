using Assets.Prototypes.Characters.Subclasses;
using Assets.Prototypes.Graphics.Portrait;
using UnityEditor;
using UnityEngine;

namespace Assets.Prototypes.Characters.Subclasses.Editor
{
    [CustomPropertyDrawer(typeof(Portrait))]
    public class PortraitDrawer : PropertyDrawer
    {
        private const float PreviewSize = 64f;
        private const float Spacing = 4f;
        private const float FieldHeight = 18f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get properties
            var ownerProp = property.FindPropertyRelative("_owner");
            var imageStackProp = property.FindPropertyRelative("_imageStack");
            var emotionProp = property.FindPropertyRelative("_emotion");
            var keyProp = property.FindPropertyRelative("_key");
            var renderedSpriteProp = property.FindPropertyRelative("_renderedSprite");

            // Auto-assign owner if null
            // This automatically sets the owner to the Character that contains this Portrait
            if (ownerProp != null && ownerProp.objectReferenceValue == null)
            {
                Character ownerCharacter = FindOwnerCharacter(property);
                if (ownerCharacter != null)
                {
                    ownerProp.objectReferenceValue = ownerCharacter;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            // Draw foldout
            property.isExpanded = EditorGUI.Foldout(
                new Rect(position.x, position.y, position.width, FieldHeight),
                property.isExpanded,
                label,
                true
            );

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                float yPos = position.y + FieldHeight + Spacing;

                // Calculate positions for sprite preview on the right
                bool hasSprite = renderedSpriteProp.objectReferenceValue != null;
                float fieldWidth = hasSprite
                    ? position.width - PreviewSize - Spacing * 2
                    : position.width;

                // Draw fields on the left
                Rect fieldRect = new Rect(position.x, yPos, fieldWidth, FieldHeight);

                EditorGUI.PropertyField(fieldRect, ownerProp, new GUIContent("Owner"));
                yPos += FieldHeight + Spacing;

                fieldRect.y = yPos;
                EditorGUI.PropertyField(fieldRect, imageStackProp, new GUIContent("Image Stack"));
                yPos += FieldHeight + Spacing;

                fieldRect.y = yPos;
                EditorGUI.PropertyField(fieldRect, emotionProp, new GUIContent("Emotion"));
                yPos += FieldHeight + Spacing;

                fieldRect.y = yPos;
                EditorGUI.PropertyField(fieldRect, keyProp, new GUIContent("Key"));
                yPos += FieldHeight + Spacing;

                fieldRect.y = yPos;
                EditorGUI.PropertyField(
                    fieldRect,
                    renderedSpriteProp,
                    new GUIContent("Rendered Sprite")
                );
                yPos += FieldHeight + Spacing;

                // Render button
                fieldRect.y = yPos;
                fieldRect.height = 25f;
                if (GUI.Button(fieldRect, "Render Portrait"))
                {
                    // Ensure the key is set before rendering
                    if (keyProp != null && string.IsNullOrEmpty(keyProp.stringValue))
                    {
                        // Generate a key using the ID
                        var idStringProp = property.FindPropertyRelative("_idString");
                        if (idStringProp != null && !string.IsNullOrEmpty(idStringProp.stringValue))
                        {
                            keyProp.stringValue = $"portrait_{idStringProp.stringValue}";
                        }
                        else
                        {
                            // Generate a new GUID if idString is also empty
                            string newGuid = System.Guid.NewGuid().ToString();
                            if (idStringProp != null)
                            {
                                idStringProp.stringValue = newGuid;
                            }
                            keyProp.stringValue = $"portrait_{newGuid}";
                        }
                        property.serializedObject.ApplyModifiedProperties();
                    }

                    // Get the Portrait object and call Render
                    var imageStack = imageStackProp.objectReferenceValue as ImageStack;
                    if (imageStack != null)
                    {
                        // Use reflection to call Render on the Portrait instance
                        object targetObject = GetTargetObjectOfProperty(property);
                        if (targetObject is Portrait portrait)
                        {
                            portrait.Render();
                            property.serializedObject.Update();
                            EditorUtility.SetDirty(property.serializedObject.targetObject);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Cannot render: ImageStack is not assigned.");
                    }
                }

                // Draw sprite preview on the right if available
                if (hasSprite)
                {
                    Sprite sprite = renderedSpriteProp.objectReferenceValue as Sprite;
                    if (sprite != null)
                    {
                        Rect previewRect = new Rect(
                            position.x + fieldWidth + Spacing,
                            position.y + FieldHeight + Spacing,
                            PreviewSize,
                            PreviewSize
                        );

                        // Draw background
                        EditorGUI.DrawRect(previewRect, new Color(0.2f, 0.2f, 0.2f, 1f));

                        // Draw sprite
                        Texture2D texture = AssetPreview.GetAssetPreview(sprite);
                        if (texture != null)
                        {
                            GUI.DrawTexture(previewRect, texture, ScaleMode.ScaleToFit);
                        }
                        else
                        {
                            // Fallback to sprite texture if preview isn't ready
                            if (sprite.texture != null)
                            {
                                GUI.DrawTextureWithTexCoords(
                                    previewRect,
                                    sprite.texture,
                                    GetSpriteTextureCoords(sprite),
                                    true
                                );
                            }
                        }

                        // Draw border
                        EditorGUI.DrawRect(
                            new Rect(previewRect.x, previewRect.y, previewRect.width, 1),
                            Color.gray
                        );
                        EditorGUI.DrawRect(
                            new Rect(previewRect.x, previewRect.yMax - 1, previewRect.width, 1),
                            Color.gray
                        );
                        EditorGUI.DrawRect(
                            new Rect(previewRect.x, previewRect.y, 1, previewRect.height),
                            Color.gray
                        );
                        EditorGUI.DrawRect(
                            new Rect(previewRect.xMax - 1, previewRect.y, 1, previewRect.height),
                            Color.gray
                        );
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return FieldHeight;
            }

            // Height includes: foldout + 5 fields + render button + spacing
            float height = FieldHeight + Spacing; // Foldout
            height += (FieldHeight + Spacing) * 5; // 5 fields
            height += 25f + Spacing; // Render button

            var renderedSpriteProp = property.FindPropertyRelative("_renderedSprite");
            if (renderedSpriteProp != null && renderedSpriteProp.objectReferenceValue != null)
            {
                // If we have a sprite, make sure there's enough space for the preview
                float fieldsHeight = (FieldHeight + Spacing) * 5 + 25f + Spacing;
                if (PreviewSize > fieldsHeight)
                {
                    height += (PreviewSize - fieldsHeight);
                }
            }

            return height;
        }

        private Rect GetSpriteTextureCoords(Sprite sprite)
        {
            if (sprite == null || sprite.texture == null)
                return new Rect(0, 0, 1, 1);

            float x = sprite.textureRect.x / sprite.texture.width;
            float y = sprite.textureRect.y / sprite.texture.height;
            float width = sprite.textureRect.width / sprite.texture.width;
            float height = sprite.textureRect.height / sprite.texture.height;

            return new Rect(x, y, width, height);
        }

        private object GetTargetObjectOfProperty(SerializedProperty property)
        {
            var path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            var elements = path.Split('.');

            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(
                        element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", "")
                    );
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        private object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(
                    name,
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.Instance
                );
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(
                    name,
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.IgnoreCase
                );
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null)
                return null;
            var enm = enumerable.GetEnumerator();

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext())
                    return null;
            }
            return enm.Current;
        }

        private Character FindOwnerCharacter(SerializedProperty property)
        {
            // Navigate up the property path to find the Character ScriptableObject
            SerializedProperty current = property;

            // Walk up the property tree
            while (current != null)
            {
                // Check if the target object is a Character
                if (current.serializedObject.targetObject is Character character)
                {
                    return character;
                }

                // Try to get parent property
                string path = current.propertyPath;
                int lastDot = path.LastIndexOf('.');

                if (lastDot > 0)
                {
                    string parentPath = path.Substring(0, lastDot);
                    current = current.serializedObject.FindProperty(parentPath);
                }
                else
                {
                    break;
                }
            }

            return null;
        }
    }
}

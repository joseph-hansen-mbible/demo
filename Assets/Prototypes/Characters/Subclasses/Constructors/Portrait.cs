using System;
using System.Collections.Generic;
using Assets.Prototypes.Graphics.Portrait;
using UnityEngine;

namespace Assets.Prototypes.Characters.Subclasses
{
    [Serializable]
    public class Portrait
    {
        [SerializeField]
        private Character _owner;

        [SerializeField]
        private ImageStack _imageStack;

        [SerializeField]
        private string _emotion;

        [SerializeField]
        private string _key;

        [SerializeField]
        private Sprite _renderedSprite;

        [SerializeField]
        private string _idString; // Store Guid as string for serialization

        private Guid _id;

        public Character Owner => _owner;
        public ImageStack ImageStack => _imageStack;
        public string Emotion => _emotion;
        public string Key => _key;
        public Sprite RenderedSprite => _renderedSprite;
        public Guid Id => _id;

        public Portrait()
        {
            _id = Guid.NewGuid();
            _idString = _id.ToString();
            _key = $"portrait_{_id}";
        }

        // Called by Unity after deserialization
        private void OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(_idString))
            {
                _id = Guid.Parse(_idString);
            }
            else
            {
                _id = Guid.NewGuid();
                _idString = _id.ToString();
            }

            // Auto-generate key if it's empty
            if (string.IsNullOrEmpty(_key))
            {
                _key = $"portrait_{_id}";
            }
        }

        public override string ToString()
        {
            return $"p{_id}";
        }

        public string Identify()
        {
            return $"Portrait(ID: {_id}, Owner: {_owner.name}, Emotion: {_emotion}, Key: {_key})";
        }

        public void Render()
        {
            // Validate that we have an ImageStack
            if (_imageStack == null)
            {
                Debug.LogWarning("Cannot render portrait: ImageStack is not assigned.");
                return;
            }

            Texture2D texture = _imageStack.PreRender();
            if (texture == null)
            {
                Debug.LogError("PreRender returned null texture.");
                return;
            }

            // Use Application.dataPath to get the correct project path
            string directoryPath = System.IO.Path.Combine(
                UnityEngine.Application.dataPath,
                "Resources",
                "GameContent",
                "Graphics",
                "Portraits"
            );
            string fileName = $"{_key}.png";
            string fullPath = System.IO.Path.Combine(directoryPath, fileName);

            // Create directory if it doesn't exist
            if (!System.IO.Directory.Exists(directoryPath))
            {
                System.IO.Directory.CreateDirectory(directoryPath);
                Debug.Log($"Created directory: {directoryPath}");
            }

            // Check if file already exists
            if (System.IO.File.Exists(fullPath))
            {
                Debug.Log($"Overwriting existing portrait texture at {fullPath}");
            }
            else
            {
                Debug.Log($"Creating new portrait texture at {fullPath}");
            }

            // Save the texture as PNG
            byte[] pngData = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(fullPath, pngData);
            Debug.Log($"Successfully saved portrait texture: {fileName}");

            // Render the sprite
            _renderedSprite = _imageStack.Render();

#if UNITY_EDITOR
            // Refresh the asset database so Unity sees the new file
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
}

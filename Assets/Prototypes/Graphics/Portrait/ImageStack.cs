using System;
using System.Collections.Generic;
using Assets.Prototypes.Characters;
using Assets.Prototypes.Characters.Subclasses;
using UnityEngine;

[Serializable]
public class ImageStackLayer
{
    [SerializeField]
    public Sprite Sprite;

    [SerializeField]
    public Vector2 Offset;

    [SerializeField]
    public Color Tint = Color.white;

    [SerializeField]
    public float Scale = 1f;

    [SerializeField]
    public float Rotation = 0f;

    [SerializeField]
    public int Order = 0;
}

namespace Assets.Prototypes.Graphics.Portrait
{
    [CreateAssetMenu(fileName = "NewImageStack", menuName = "Graphics/Portrait/ImageStack")]
    public class ImageStack : ScriptableObject
    {
        [SerializeField]
        private Character _ownerCharacter;

        [SerializeField]
        private List<ImageStackLayer> _layers = new List<ImageStackLayer>();

        public List<ImageStackLayer> Layers => _layers;
        public Character OwnerCharacter => _ownerCharacter;

        public Texture2D PreRender()
        {
            // Default render dimensions
            int width = 512;
            int height = 512;

            // Create a new texture to render into
            Texture2D renderTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            // Clear the texture with transparent pixels
            Color32[] pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(0, 0, 0, 0);
            }
            renderTexture.SetPixels32(pixels);
            renderTexture.Apply();

            return renderTexture;
        }

        public Sprite Render()
        {
            return null;
        }
    }
}

using System;
using NUnit.Framework;
using Turnroot.Graphics.Portrait;
using Turnroot.Graphics2D;
using UnityEngine;

namespace Turnroot.Tests.Editor
{
    // Minimal concrete subclass used for testing StackedImage behaviour
    [Serializable]
    public class TestStackedImage : StackedImage<UnityEngine.Object>
    {
        protected override string GetSaveSubdirectory() => "TestSubdir";

        public override void UpdateTintColorsFromOwner()
        {
            if (_tintColors == null || _tintColors.Length < 3)
                _tintColors = new Color[3] { Color.white, Color.white, Color.white };
        }
    }

    public class StackedImageTests
    {
        [Test]
        public void Constructor_InitializesIdAndKey()
        {
            var s = new TestStackedImage();
            Assert.AreNotEqual(Guid.Empty, s.Id);
            Assert.IsNotNull(s.Key);
            StringAssert.StartsWith("stackedImage_", s.Key);
            StringAssert.Contains(s.Id.ToString(), s.ToString());
            StringAssert.Contains("StackedImage(ID", s.Identify());
        }

        [Test]
        public void SetKey_UsesProvidedAndDoesNotGenerateWhenNonEmpty()
        {
            var s = new TestStackedImage();
            s.SetKey("my_custom_key");
            Assert.AreEqual("my_custom_key", s.Key);
        }

        [Test]
        public void CompositeLayers_WithImageStack_ReturnsTexture()
        {
            var imageStack = ScriptableObject.CreateInstance<ImageStack>();

            // Create a tiny layer sprite (4x4) so composition is cheap but valid
            var tex = new Texture2D(4, 4);
            var fill = new Color(0.2f, 0.4f, 0.8f, 1f);
            for (int i = 0; i < 16; i++)
                tex.SetPixel(i % 4, i / 4, fill);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), Vector2.zero);

            var layer = new ImageStackLayer()
            {
                Sprite = sprite,
                Scale = 1f,
                Offset = Vector2.zero,
            };
            imageStack.Layers.Add(layer);

            var s = new TestStackedImage();
            s.SetImageStack(imageStack);

            // CompositeLayers will use default Graphics2DSettings (512x512) if none found.
            var result = s.CompositeLayers();
            Assert.IsNotNull(result);
            Assert.AreEqual(512, result.width);
            Assert.AreEqual(512, result.height);
        }
    }
}

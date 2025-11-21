using Assets.AbstractScripts.Graphics2D;
using NUnit.Framework;
using UnityEngine;

namespace Turnroot.Tests.Editor
{
    public class ImageCompositorTests
    {
        [Test]
        public void CreateSpriteFromTexture_Null_ReturnsNull()
        {
            // The method logs an error when passed null — expect it so the test runner
            // doesn't treat the logged error as an unexpected test failure.
            UnityEngine.TestTools.LogAssert.Expect(
                UnityEngine.LogType.Error,
                "Cannot create sprite from null texture."
            );
            var sprite = ImageCompositor.CreateSpriteFromTexture(null);
            Assert.IsNull(sprite);
        }

        [Test]
        public void CreateSpriteFromTexture_ValidTexture_CreatesSprite()
        {
            var tex = new Texture2D(2, 2);
            tex.SetPixels(new[] { Color.red, Color.green, Color.blue, Color.white });
            tex.Apply();

            var sprite = ImageCompositor.CreateSpriteFromTexture(tex);
            Assert.IsNotNull(sprite);
            Assert.AreEqual(2, (int)sprite.rect.width);
            Assert.AreEqual(2, (int)sprite.rect.height);
        }

        [Test]
        public void TintSpritePixels_MasksAndTints_BlendsCorrectly()
        {
            // Base sprite: two pixels - left black, right white
            var baseTex = new Texture2D(2, 1);
            baseTex.SetPixels(new[] { Color.black, Color.white });
            baseTex.Apply();
            var sprite = Sprite.Create(baseTex, new Rect(0, 0, 2, 1), Vector2.zero);

            // Mask: first pixel red channel=1 (select tint[0]), second pixel green channel=1 (select tint[1])
            var maskTex = new Texture2D(2, 1);
            maskTex.SetPixel(0, 0, new Color(1, 0, 0, 1));
            maskTex.SetPixel(1, 0, new Color(0, 1, 0, 1));
            maskTex.Apply();
            var maskSprite = Sprite.Create(maskTex, new Rect(0, 0, 2, 1), Vector2.zero);

            var tints = new Color[] { Color.red, Color.green, Color.blue };

            var result = ImageCompositor.TintSpritePixels(sprite, maskSprite, tints);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Length);

            // Pixel 0: originally black -> blended towards red -> result.r > 0
            Assert.Greater(result[0].r, 0f);
            // Pixel 1: originally white -> blended towards green -> result.g > result.r
            Assert.Greater(result[1].g, result[1].r);
        }

        [Test]
        public void CompositeLayersOnTexture_ElementwiseBlending_MixesAlphaCorrectly()
        {
            // Base texture: white, alpha = 1
            var baseTex = new Texture2D(2, 1);
            baseTex.SetPixels(new[] { Color.white, Color.white });
            baseTex.Apply();

            // Layer texture: red, half alpha
            var layerTex = new Texture2D(2, 1);
            var redHalf = new Color(1f, 0f, 0f, 0.5f);
            layerTex.SetPixels(new[] { redHalf, redHalf });
            layerTex.Apply();
            var layerSprite = Sprite.Create(layerTex, new Rect(0, 0, 2, 1), Vector2.zero);

            var composited = ImageCompositor.CompositeLayersOnTexture(
                baseTex,
                new Sprite[] { layerSprite }
            );

            Assert.IsNotNull(composited);
            var pixels = composited.GetPixels();
            Assert.AreEqual(2, pixels.Length);

            // Expect red overlay with alpha blending against white base
            // outAlpha = srcA + dstA * (1 - srcA) = 0.5 + 1 * 0.5 = 1.0
            // outR = (src.r * srcA + dst.r * dstA * (1-srcA)) / outAlpha = (1*0.5 + 1*1*0.5)/1 = 1
            Assert.AreEqual(1f, pixels[0].r, 1e-5f);
            Assert.AreEqual(1f, pixels[0].a, 1e-5f);
        }
    }
}

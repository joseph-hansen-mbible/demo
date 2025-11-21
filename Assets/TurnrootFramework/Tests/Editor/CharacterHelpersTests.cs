using System.Collections.Generic;
using NUnit.Framework;
using Turnroot.Characters;
using Turnroot.Characters.Components.Support;
using Turnroot.Characters.Stats;
using Turnroot.Characters.Subclasses;
using UnityEngine;

namespace Turnroot.Tests.Editor
{
    public class CharacterHelpersTests
    {
        [Test]
        public void CloneBoundedStats_CreatesDeepCopies()
        {
            var original = new List<BoundedCharacterStat>
            {
                new BoundedCharacterStat(100f, 50f, 0f, BoundedStatType.Health),
                new BoundedCharacterStat(120f, 80f, 0f, BoundedStatType.LevelExperience),
            };

            var copy = CharacterHelpers.CloneBoundedStats(original);

            Assert.AreEqual(original.Count, copy.Count);
            for (int i = 0; i < original.Count; i++)
            {
                Assert.AreNotSame(original[i], copy[i], "Clone should produce new instances");
                Assert.AreEqual(original[i].StatType, copy[i].StatType);
                Assert.AreEqual(original[i].Current, copy[i].Current);
                Assert.AreEqual(original[i].Max, copy[i].Max);
            }
        }

        [Test]
        public void CloneUnboundedStats_CreatesDeepCopies()
        {
            var original = new List<CharacterStat>
            {
                new CharacterStat(10f, UnboundedStatType.Strength),
                new CharacterStat(8f, UnboundedStatType.Speed),
            };

            var copy = CharacterHelpers.CloneUnboundedStats(original);

            Assert.AreEqual(original.Count, copy.Count);
            for (int i = 0; i < original.Count; i++)
            {
                Assert.AreNotSame(original[i], copy[i]);
                Assert.AreEqual(original[i].StatType, copy[i].StatType);
                Assert.AreEqual(original[i].Current, copy[i].Current);
            }
        }

        [Test]
        public void CloneSupportRelationships_SkipsSelfReferences()
        {
            var owner = ScriptableObject.CreateInstance<CharacterData>();
            var other = ScriptableObject.CreateInstance<CharacterData>();

            var templ = new SupportRelationship();
            templ.Character = owner; // should be skipped

            var templ2 = new SupportRelationship();
            templ2.Character = other; // should be kept

            var templates = new List<SupportRelationship> { templ, templ2 };

            var instances = CharacterHelpers.CloneSupportRelationships(templates, owner);

            Assert.AreEqual(1, instances.Count);
            Assert.AreEqual(other, instances[0].Character);
        }

        [Test]
        public void SanitizeForCharacter_RemovesSelfReferencesAndInitializesOthers()
        {
            var owner = ScriptableObject.CreateInstance<CharacterData>();
            var other = ScriptableObject.CreateInstance<CharacterData>();

            var templ = new SupportRelationship();
            templ.Character = owner; // should be removed

            var templ2 = new SupportRelationship();
            templ2.Character = other; // should remain

            var list = new List<SupportRelationship> { templ, templ2 };

            var removed = SupportRelationship.SanitizeForCharacter(owner, list);

            Assert.AreEqual(1, removed.Count);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(other, list[0].Character);
        }

        [Test]
        public void ForEachPortraitLayer_EnumeratesAllLayers()
        {
            // Build a small portraits dictionary with ImageStack layers
            var portraits = new SerializableDictionary<string, Portrait>();

            var p1 = new Portrait();
            var s1 = ScriptableObject.CreateInstance<Turnroot.Graphics.Portrait.ImageStack>();
            s1.Layers.Add(new ImageStackLayer { Tag = "Hair" });
            s1.Layers.Add(new ImageStackLayer { Tag = "Face" });
            p1.SetImageStack(s1);

            var p2 = new Portrait();
            var s2 = ScriptableObject.CreateInstance<Turnroot.Graphics.Portrait.ImageStack>();
            s2.Layers.Add(new ImageStackLayer { Tag = "Eyes" });
            p2.SetImageStack(s2);

            portraits.Add("p1", p1);
            portraits.Add("p2", p2);

            int count = 0;
            CharacterHelpers.ForEachPortraitLayer(portraits, layer => count++);

            Assert.AreEqual(3, count, "Should have enumerated 3 portrait layers total");
        }
    }
}

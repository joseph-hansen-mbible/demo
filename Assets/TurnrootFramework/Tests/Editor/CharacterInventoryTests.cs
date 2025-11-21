using NUnit.Framework;
using Turnroot.Gameplay.Objects;
using UnityEngine;

namespace Turnroot.Tests.Editor
{
    public class CharacterInventoryTests
    {
        [Test]
        public void AddRemoveInventoryAndCapacity()
        {
            var inv = new CharacterInventoryInstance(2);

            var itemA = ScriptableObject.CreateInstance<ObjectItem>();
            var itemB = ScriptableObject.CreateInstance<ObjectItem>();
            var instA = new ObjectItemInstance(itemA);
            var instB = new ObjectItemInstance(itemB);

            Assert.IsTrue(inv.CanAddItem());
            inv.AddToInventory(instA);
            Assert.AreEqual(1, inv.InventoryItems.Count);

            inv.AddToInventory(instB);
            Assert.AreEqual(2, inv.InventoryItems.Count);

            // capacity reached
            Assert.IsFalse(inv.CanAddItem());

            // remove item
            inv.RemoveFromInventory(instA);
            Assert.AreEqual(1, inv.InventoryItems.Count);

            // cleanup
            inv.RemoveFromInventory(instB);
            Assert.AreEqual(0, inv.InventoryItems.Count);
        }

        [Test]
        public void EquipWeaponAndAdjustIndicesOnRemove()
        {
            var inv = new CharacterInventoryInstance(3);

            var w1 = new ObjectItem();
            var w2 = new ObjectItem();
            var w3 = new ObjectItem();

            inv.AddToInventory(new ObjectItemInstance(w1)); // index 0
            inv.AddToInventory(new ObjectItemInstance(w2)); // index 1
            inv.AddToInventory(new ObjectItemInstance(w3)); // index 2

            inv.EquipItem(1); // equip index 1
            Assert.IsTrue(inv.IsItemEquipped(inv.InventoryItems[1]));
            Assert.AreEqual(1, inv.GetEquippedWeaponIndex());

            // Remove an item that is before the equipped index -> indices shift
            inv.RemoveFromInventory(inv.InventoryItems[0]);
            // previously index 1 should become index 0
            Assert.AreEqual(0, inv.GetEquippedWeaponIndex());
        }
    }
}

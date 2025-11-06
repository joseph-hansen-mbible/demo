using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCharacterInventory",
    menuName = "Turnroot/Character/CharacterInventory"
)]
public class CharacterInventory : ScriptableObject
{
    [SerializeField]
    private ObjectItem[] _inventoryItems = new ObjectItem[0];

    [SerializeField]
    private int _capacity = 6;

    [SerializeField]
    private int _currentItemCount;

    [SerializeField]
    private int _currentWeight;

    [SerializeField]
    private int[] _equippedItemIndices = new int[3] { -1, -1, -1 };

    [SerializeField]
    private bool _isWeaponEquipped;

    [SerializeField]
    private bool _isShieldEquipped;

    [SerializeField]
    private bool _isAccessoryEquipped;

    public ObjectItem[] InventoryItems
    {
        get => _inventoryItems;
        set => _inventoryItems = value;
    }

    public int Capacity
    {
        get => _capacity;
        set => _capacity = value;
    }

    public int CurrentItemCount
    {
        get => _currentItemCount;
        set => _currentItemCount = value;
    }

    public int CurrentWeight
    {
        get => _currentWeight;
        set => _currentWeight = value;
    }

    public int[] EquippedItemIndices
    {
        get => _equippedItemIndices;
    }

    public int GetEquippedItemIndex(ObjectItemType itemType)
    {
        return itemType switch
        {
            ObjectItemType.Weapon => _equippedItemIndices[0],
            ObjectItemType.Shield => _equippedItemIndices[1],
            ObjectItemType.Accessory => _equippedItemIndices[2],
            _ => -1,
        };
    }

    public bool IsWeaponEquipped
    {
        get => _isWeaponEquipped;
        set => _isWeaponEquipped = value;
    }

    public bool IsShieldEquipped
    {
        get => _isShieldEquipped;
        set => _isShieldEquipped = value;
    }

    public bool IsAccessoryEquipped
    {
        get => _isAccessoryEquipped;
        set => _isAccessoryEquipped = value;
    }

    // helpers

    public bool IsItemEquipped(ObjectItem item)
    {
        int index = System.Array.IndexOf(_inventoryItems, item);
        if (index < 0)
            return false;

        // Check if this item is equipped in any slot
        return System.Array.IndexOf(_equippedItemIndices, index) >= 0;
    }

    public void AddToInventory(ObjectItem item)
    {
        if (_inventoryItems.Length >= _capacity)
        {
            Debug.LogWarning("Inventory is full. Cannot add item.");
            return;
        }

        ObjectItem[] newArray = new ObjectItem[_inventoryItems.Length + 1];
        _inventoryItems.CopyTo(newArray, 0);
        newArray[_inventoryItems.Length] = item;
        _inventoryItems = newArray;
        _currentItemCount++;
        _currentWeight += item.Weight;
    }

    public void RemoveFromInventory(ObjectItem item)
    {
        int index = System.Array.IndexOf(_inventoryItems, item);
        if (index < 0)
        {
            Debug.LogWarning("Item not found in inventory. Cannot remove item.");
            return;
        }

        ObjectItem[] newArray = new ObjectItem[_inventoryItems.Length - 1];
        for (int i = 0, j = 0; i < _inventoryItems.Length; i++)
        {
            if (i == index)
                continue;
            newArray[j++] = _inventoryItems[i];
        }
        _inventoryItems = newArray;
        _currentItemCount--;
        _currentWeight -= item.Weight;

        // Update equipped indices if the removed item was equipped or affects indices
        for (int i = 0; i < _equippedItemIndices.Length; i++)
        {
            if (_equippedItemIndices[i] == index)
            {
                // Item was equipped, unequip it
                _equippedItemIndices[i] = -1;
                switch (i)
                {
                    case 0:
                        _isWeaponEquipped = false;
                        break;
                    case 1:
                        _isShieldEquipped = false;
                        break;
                    case 2:
                        _isAccessoryEquipped = false;
                        break;
                }
            }
            else if (_equippedItemIndices[i] > index)
            {
                // Adjust equipped index if necessary
                _equippedItemIndices[i]--;
            }
        }
    }

    public void EquipItem(int index)
    {
        if (index < 0 || index >= _inventoryItems.Length)
        {
            Debug.LogWarning("Invalid inventory index. Cannot equip item.");
            return;
        }

        ObjectItem itemToEquip = _inventoryItems[index];

        // Determine which slot to use based on item type
        int slotIndex = itemToEquip.ItemType switch
        {
            ObjectItemType.Weapon => 0,
            ObjectItemType.Shield => 1,
            ObjectItemType.Accessory => 2,
            _ => -1,
        };

        if (slotIndex == -1)
        {
            Debug.LogWarning("Item type cannot be equipped.");
            return;
        }

        // Unequip currently equipped item in this slot if any
        if (_equippedItemIndices[slotIndex] != -1)
        {
            UnequipItemFromSlot(slotIndex);
        }

        _equippedItemIndices[slotIndex] = index;

        // Set equipped flags based on item type
        switch (itemToEquip.ItemType)
        {
            case ObjectItemType.Weapon:
                _isWeaponEquipped = true;
                break;
            case ObjectItemType.Shield:
                _isShieldEquipped = true;
                break;
            case ObjectItemType.Accessory:
                _isAccessoryEquipped = true;
                break;
        }
    }

    public void ReorderItem(int oldIndex, int newIndex)
    {
        if (
            oldIndex < 0
            || oldIndex >= _inventoryItems.Length
            || newIndex < 0
            || newIndex >= _inventoryItems.Length
        )
        {
            Debug.LogWarning("Invalid inventory index. Cannot reorder item.");
            return;
        }

        ObjectItem itemToMove = _inventoryItems[oldIndex];
        ObjectItem[] newArray = new ObjectItem[_inventoryItems.Length];

        for (int i = 0, j = 0; i < _inventoryItems.Length; i++)
        {
            if (i == oldIndex)
                continue;
            if (j == newIndex)
            {
                newArray[j++] = itemToMove;
            }
            newArray[j++] = _inventoryItems[i];
        }

        // If moving to the end
        if (newIndex == _inventoryItems.Length - 1)
        {
            newArray[newIndex] = itemToMove;
        }

        _inventoryItems = newArray;

        // Update equipped item indices if necessary
        for (int i = 0; i < _equippedItemIndices.Length; i++)
        {
            if (_equippedItemIndices[i] == oldIndex)
            {
                _equippedItemIndices[i] = newIndex;
            }
            else if (oldIndex < _equippedItemIndices[i] && newIndex >= _equippedItemIndices[i])
            {
                _equippedItemIndices[i]--;
            }
            else if (oldIndex > _equippedItemIndices[i] && newIndex <= _equippedItemIndices[i])
            {
                _equippedItemIndices[i]++;
            }
        }
    }

    private void UnequipItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _equippedItemIndices.Length)
            return;

        _equippedItemIndices[slotIndex] = -1;

        switch (slotIndex)
        {
            case 0:
                _isWeaponEquipped = false;
                break;
            case 1:
                _isShieldEquipped = false;
                break;
            case 2:
                _isAccessoryEquipped = false;
                break;
        }
    }

    public void UnequipItem(int inventoryIndex)
    {
        if (inventoryIndex < 0 || inventoryIndex >= _inventoryItems.Length)
        {
            Debug.LogWarning("Invalid inventory index. Cannot unequip item.");
            return;
        }

        // Find which slot has this item equipped
        for (int i = 0; i < _equippedItemIndices.Length; i++)
        {
            if (_equippedItemIndices[i] == inventoryIndex)
            {
                UnequipItemFromSlot(i);
                return;
            }
        }

        Debug.LogWarning("Item is not currently equipped.");
    }

    public void UnequipAllItems()
    {
        _equippedItemIndices[0] = -1;
        _equippedItemIndices[1] = -1;
        _equippedItemIndices[2] = -1;
        _isWeaponEquipped = false;
        _isShieldEquipped = false;
        _isAccessoryEquipped = false;
    }
}

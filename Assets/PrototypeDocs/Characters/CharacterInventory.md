# CharacterInventory

**Inherits:** `ScriptableObject`  
**Location:** `Assets/Prototypes/Characters/Components/Inventory/CharacterInventory.cs`

Manages character inventory with support for simultaneous equipment of weapon, shield, and accessory items.

## Creation

```csharp
Assets > Create > Character > CharacterInventory
```

## Equipment System

The inventory supports **3 simultaneous equipment slots**:

| Slot Index | Item Type | Flag Property |
|------------|-----------|---------------|
| 0 | `ObjectItemType.Weapon` | `IsWeaponEquipped` |
| 1 | `ObjectItemType.Shield` | `IsShieldEquipped` |
| 2 | `ObjectItemType.Accessory` | `IsAccessoryEquipped` |

## Properties

### Inventory Data

| Property | Type | Access | Description |
|----------|------|--------|-------------|
| `InventoryItems` | `ObjectItem[]` | Read/Write | Array of items in inventory |
| `Capacity` | `int` | Read/Write | Maximum number of items (default: 6) |
| `CurrentItemCount` | `int` | Read/Write | Current number of items in inventory |
| `CurrentWeight` | `int` | Read/Write | Total weight of all items |

### Equipment State

| Property | Type | Access | Description |
|----------|------|--------|-------------|
| `EquippedItemIndices` | `int[]` | Read-only | Array of 3 indices (-1 if slot empty) |
| `IsWeaponEquipped` | `bool` | Read/Write | Whether weapon slot is occupied |
| `IsShieldEquipped` | `bool` | Read/Write | Whether shield slot is occupied |
| `IsAccessoryEquipped` | `bool` | Read/Write | Whether accessory slot is occupied |

## Methods

### GetEquippedItemIndex

```csharp
int GetEquippedItemIndex(ObjectItemType itemType)
```

Returns the inventory index of the equipped item of the specified type.

**Parameters:**
- `itemType` - Type of item to query (Weapon/Shield/Accessory)

**Returns:** Inventory index (0-based), or -1 if no item of that type is equipped

**Example:**
```csharp
int weaponIndex = inventory.GetEquippedItemIndex(ObjectItemType.Weapon);
if (weaponIndex >= 0)
{
    ObjectItem weapon = inventory.InventoryItems[weaponIndex];
}
```

### IsItemEquipped

```csharp
bool IsItemEquipped(ObjectItem item)
```

Checks if a specific item is currently equipped in any slot.

**Parameters:**
- `item` - The item to check

**Returns:** `true` if item is equipped, `false` otherwise

### AddToInventory

```csharp
void AddToInventory(ObjectItem item)
```

Adds an item to the inventory.

**Parameters:**
- `item` - The item to add

**Behavior:**
- Fails with warning if inventory is at capacity
- Automatically increments `CurrentItemCount` and `CurrentWeight`
- Dynamically resizes the `InventoryItems` array

### RemoveFromInventory

```csharp
void RemoveFromInventory(ObjectItem item)
```

Removes an item from the inventory.

**Parameters:**
- `item` - The item to remove

**Behavior:**
- Fails with warning if item not found
- Automatically decrements `CurrentItemCount` and `CurrentWeight`
- If item was equipped, unequips it and clears appropriate flag
- Adjusts all equipped indices to account for array shift

### EquipItem

```csharp
void EquipItem(int index)
```

Equips an item from the inventory into the appropriate slot based on its type.

**Parameters:**
- `index` - Inventory index of the item to equip

**Behavior:**
- Determines slot from item's `ObjectItemType` (Weapon→0, Shield→1, Accessory→2)
- Automatically unequips previously equipped item in that slot
- Sets appropriate equipped flag (`IsWeaponEquipped`, etc.)
- Fails with warning if index is invalid or item type cannot be equipped
- Only `Weapon`, `Shield`, and `Accessory` types can be equipped

**Example:**
```csharp
// Equip item at index 2
inventory.EquipItem(2);

// Check what was equipped
if (inventory.IsWeaponEquipped)
{
    int weaponIdx = inventory.GetEquippedItemIndex(ObjectItemType.Weapon);
    Debug.Log($"Equipped weapon at index {weaponIdx}");
}
```

### UnequipItem

```csharp
void UnequipItem(int inventoryIndex)
```

Unequips a specific item from its equipment slot.

**Parameters:**
- `inventoryIndex` - Inventory index of the item to unequip

**Behavior:**
- Finds which slot has the item equipped
- Clears that slot and its corresponding flag
- Item remains in inventory
- Fails with warning if index is invalid or item is not equipped

**Example:**
```csharp
// Unequip the weapon at inventory index 0
inventory.UnequipItem(0);

// Check if it was unequipped
if (!inventory.IsWeaponEquipped)
{
    Debug.Log("Weapon successfully unequipped");
}
```

### UnequipAllItems

```csharp
void UnequipAllItems()
```

Unequips all items from all slots.

**Behavior:**
- Sets all 3 equipment slots to -1
- Sets all equipped flags to `false`
- Items remain in inventory, only equipment state changes

### ReorderItem

```csharp
void ReorderItem(int oldIndex, int newIndex)
```

Moves an item from one inventory position to another.

**Parameters:**
- `oldIndex` - Current inventory index
- `newIndex` - Desired inventory index

**Behavior:**
- Shifts other items to accommodate the move
- Updates all equipped indices to track moved items
- If equipped item is moved, equipment state follows it
- Fails with warning if either index is invalid

## Equipment Slot Mapping

The system uses a fixed 3-slot array with type-based routing:

```
_equippedItemIndices[0] → Weapon   → IsWeaponEquipped
_equippedItemIndices[1] → Shield   → IsShieldEquipped
_equippedItemIndices[2] → Accessory → IsAccessoryEquipped
```

Each slot stores the **inventory index** of the equipped item, or -1 if empty.

## Implementation Notes

### Index Tracking

The inventory maintains index integrity across operations:
- **RemoveFromInventory**: Adjusts equipped indices when items below them are removed
- **ReorderItem**: Updates equipped indices to track moved items
- **AddToInventory**: No adjustment needed (appends to end)

### Non-Equippable Types

Only `Weapon`, `Shield`, and `Accessory` can be equipped. Attempting to equip other types (`Consumable`, `Gift`, `Required`) logs a warning and fails.

## Related Types

### ObjectItem

```csharp
public class ObjectItem : MonoBehaviour
{
    public string itemName;
    public ObjectItemType ItemType;
    public int Weight;
    public Sprite icon;
}
```

### ObjectItemType

```csharp
public enum ObjectItemType
{
    Weapon,      // Equipment slot 0
    Shield,      // Equipment slot 1
    Accessory,   // Equipment slot 2
    Consumable,  // Not equippable
    Gift,        // Not equippable
    Required,    // Not equippable
}
```

---

## See Also

- **[Character](Character.md)** - Character system
- **[CharacterComponents](CharacterComponents.md)** - Other character components

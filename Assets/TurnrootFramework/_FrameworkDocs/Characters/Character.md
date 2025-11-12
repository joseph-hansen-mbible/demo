# CharacterData

**Namespace:** `Turnroot.Characters`  
**Inherits:** `ScriptableObject`

Main character configuration asset. Contains all character data including identity, demographics, stats, relationships, and visual settings.

## Creation

```csharp
// Create via Unity menu
Assets > Create > Turnroot/Character/CharacterData
```

## Properties

### Identity
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Which` | `CharacterWhich` | Enemy | Character allegiance (Player/Enemy/Ally) |
| `DisplayName` | `string` | "New Unit" | Character display name |
| `FullName` | `string` | "Newly Created Unit" | Character's full formal name |
| `Team` | `string` | - | Team or faction identifier |

### Demographics
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `CharacterPronouns` | `Pronouns` | - | Pronoun set (subject/object/possessive) |
| `Height` | `float` | 166 | Height in centimeters (100-250) |
| `BirthdayDay` | `int` | 1 | Day of birth (1-31) |
| `BirthdayMonth` | `int` | 1 | Month of birth (1-12) |

### Description
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ShortDescription` | `string` | "A new unit" | Brief character description |
| `Notes` | `string` | "Take private notes..." | Editor-only development notes |

### Character Flags
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `CanSSupport` | `bool` | false | Can form S-rank support relationships |
| `CanSSupportAvatar` | `bool` | false | Can form S-rank support with avatar |
| `CanHaveChildren` | `bool` | false | Can have child units |
| `IsRecruitable` | `bool` | false | Can be recruited by player |
| `IsUnique` | `bool` | false | Unique character (cannot duplicate) |

### Visual
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `AccentColor1` | `Color` | Black | Primary accent color |
| `AccentColor2` | `Color` | Black | Secondary accent color |
| `AccentColor3` | `Color` | Black | Tertiary accent color |
| `Portraits` | `SerializableDictionary<string, Portrait>` | - | Named portrait configurations |
| `PortraitArray` | `Portrait[]` | - | Cached array of portraits |
| `Sprites` | `Sprite[]` | - | Additional sprite assets |

### Stats & Progression
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Level` | `int` | 1 | Character level |
| `Exp` | `int` | 0 | Experience points |
| `BoundedStats` | `List<BoundedCharacterStat>` | - | Bounded stat values (Health, etc.) |
| `UnboundedStats` | `List<CharacterStat>` | - | Unbounded stat values (Strength, etc.) |

### Class & Battalion
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `UnitClass` | `UnityEngine.Object` | - | Primary unit class |
| `Battalion` | `UnityEngine.Object` | - | Assigned battalion |
| `SpecialUnitClasses` | `List<string>` | - | Additional class types |

### Skills & Abilities
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Skills` | `List<Skill>` | - | Standard skills |
| `SpecialSkills` | `List<Skill>` | - | Special/unique skills |

### AI & Behavior
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `AI` | `UnityEngine.Object` | - | AI behavior configuration |

### Inventory
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `StartingInventory` | `List<InventorySlot>` | - | Initial equipment and items |

### Relationships
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `SupportRelationships` | `List<SupportRelationship>` | - | Support relationship configurations |

### Heredity
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PassedDownTraits` | `HereditaryTraits` | - | Traits passed to children |
| `HasDesignatedChildUnit` | `bool` | false | Has designated child character |
| `ChildUnitId` | `CharacterData` | - | Designated child character |

## Methods

### Public Methods
| Method | Returns | Description |
|--------|---------|-------------|
| `InvalidatePortraitArrayCache()` | `void` | Clears cached portrait array |
| `SaveDefaults()` | `void` | Saves current portrait layer defaults for all tagged layers across all portraits |
| `LoadDefaults()` | `void` | Loads saved portrait layer defaults for all tagged layers across all portraits |
| `GetBoundedStat(BoundedStatType)` | `BoundedCharacterStat` | Gets bounded stat by type |
| `GetUnboundedStat(UnboundedStatType)` | `CharacterStat` | Gets unbounded stat by type |

### Default Management

#### SaveDefaults()
Saves the current configuration of all tagged portrait layers as defaults. This method:
- Clears existing tagged layer defaults
- Iterates through all portraits and their ImageStack layers
- For each layer with a non-empty `Tag`, saves: `Sprite`, `Offset`, `Scale`, and `Tint` values
- Stores these in `_taggedLayerDefaults` dictionary keyed by tag

#### LoadDefaults()
Restores saved default configurations for all tagged portrait layers. This method:
- Iterates through all portraits and their ImageStack layers  
- For each layer with a non-empty `Tag` that exists in saved defaults, restores: `Sprite`, `Offset`, `Scale`, and `Tint` values
- Invalidates portrait array cache to refresh editor UI

## Nested Classes

### InventorySlot
| Property | Type | Description |
|----------|------|-------------|
| `Item` | `ObjectItem` | Item in slot |
| `SlotIndex` | `int` | Slot position |

### TaggedLayerDefault
| Property | Type | Description |
|----------|------|-------------|
| `Tag` | `string` | Layer tag |
| `Sprite` | `Sprite` | Default sprite |
| `Offset` | `Vector2` | Default offset |
| `Scale` | `float` | Default scale |
| `Tint` | `Color` | Default tint |

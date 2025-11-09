# Character

**Namespace:** `Turnroot.Characters`  
**Inherits:** `ScriptableObject`

Main character configuration asset. Contains all character data including identity, demographics, stats, relationships, and visual settings.

## Creation

```csharp
// Create via Unity menu
Assets > Create > Character > Character
```

## Properties

### Identity
| Property | Type | Description |
|----------|------|-------------|
| `Which` | `CharacterWhich` | Character allegiance (Player/Enemy/Ally) |
| `Name` | `string` | Character display name |
| `FullName` | `string` | Character's full formal name |
| `Title` | `string` | Character's title or honorific |
| `Team` | `string` | Team or faction identifier |

### Demographics
| Property | Type | Description |
|----------|------|-------------|
| `Age` | `int` | Character age in years |
| `CharacterPronouns` | `Pronouns` | Pronoun set (subject/object/possessive) |
| `Height` | `float` | Height in centimeters |
| `BirthdayDay` | `int` | Day of birth (1-31) |
| `BirthdayMonth` | `int` | Month of birth (1-12) |

### Description
| Property | Type | Description |
|----------|------|-------------|
| `ShortDescription` | `string` | Brief character description |
| `Notes` | `string` | Editor-only development notes |

### Character Flags
| Property | Type | Description |
|----------|------|-------------|
| `CanSSupport` | `bool` | Can form S-rank support relationships |
| `CanHaveChildren` | `bool` | Can have child units |
| `IsRecruitable` | `bool` | Can be recruited by player |
| `IsUnique` | `bool` | Unique character (cannot duplicate) |

### Visual
| Property | Type | Description |
|----------|------|-------------|
| `UseAccentColors` | `bool` | Enable accent color tinting (loaded from settings) |
| `AccentColor1` | `Color` | Primary accent color for portrait tinting |
| `AccentColor2` | `Color` | Secondary accent color |
| `AccentColor3` | `Color` | Tertiary accent color |
| `Portraits` | `Portrait[]` | Array of portrait configurations |
| `Sprites` | `Sprite[]` | Legacy sprite references |

### Stats & Progression
| Property | Type | Description |
|----------|------|-------------|
| `Level` | `int` | Current character level |
| `Exp` | `int` | Current experience points |
| `BoundedStats` | `List<BoundedCharacterStat>` | Stats with min/max bounds (HP, Str, etc.) |
| `UnboundedStats` | `List<CharacterStat>` | Stats without bounds |

### Experience Systems
| Property | Type | Description |
|----------|------|-------------|
| `Experiences` | `Object` | Character experience data |
| `ExperienceGrowths` | `Object` | Stat growth rates |
| `ExperienceAptitudes` | `Object` | Weapon/skill aptitudes |
| `ClassExps` | `SerializableDictionary<string, int>` | Class-specific experience values |

### Class & Battalion
| Property | Type | Description |
|----------|------|-------------|
| `UnitClass` | `Object` | Current class assignment |
| `Battalion` | `Object` | Battalion assignment |
| `SpecialUnitClasses` | `List<string>` | Special/unique classes available |

### Skills & Abilities
| Property | Type | Description |
|----------|------|-------------|
| `Skills` | `List<string>` | Learned skills |
| `SpecialSkills` | `List<string>` | Unique/personal skills |

### AI & Behavior
| Property | Type | Description |
|----------|------|-------------|
| `AI` | `Object` | AI behavior configuration |
| `Goals` | `List<string>` | AI goals/objectives |

### Relationships
| Property | Type | Description |
|----------|------|-------------|
| `SupportRelationships` | `List<SupportRelationship>` | Support relationship data |

### Heredity
| Property | Type | Description |
|----------|------|-------------|
| `PassedDownTraits` | `HereditaryTraits` | Traits inherited by children |
| `ChildUnitId` | `Character` | Reference to child unit |

## Methods

### Stat Queries
```csharp
BoundedCharacterStat GetBoundedStat(BoundedStatType type)
```
Get a bounded stat by type.
- **Returns:** Stat or `null` if not found

```csharp
CharacterStat GetUnboundedStat(UnboundedStatType type)
```
Get an unbounded stat by type.
- **Returns:** Stat or `null` if not found

### Class Experience
```csharp
int GetClassExp(string classId)
```
Get experience for a specific class.
- **Parameters:** `classId` - Class identifier
- **Returns:** Experience value or 0 if not found

```csharp
void SetClassExp(string classId, int value)
```
Set experience for a specific class.
- **Parameters:** 
  - `classId` - Class identifier
  - `value` - Experience value to set

### Support Relationships
```csharp
SupportRelationship GetSupportRelationship(Character character)
```
Get support relationship with another character.
- **Returns:** Relationship or `null` if none exists

```csharp
void AddSupportRelationship(Character character)
```
Add a support relationship (if not already present).

```csharp
void RemoveSupportRelationship(Character character)
```
Remove support relationship with a character.

## Lifecycle

### OnEnable
Automatically loads `CharacterPrototypeSettings` from `Resources/GameSettings/` and applies `UseAccentColors` setting.

## Notes
- Accent colors are managed by `CharacterPrototypeSettings` and loaded on asset initialization
- Portrait tinting uses the three accent colors mapped to RGB channels in mask textures

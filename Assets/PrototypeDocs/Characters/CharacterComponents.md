# Character Components

## Pronouns

**Namespace:** `Assets.Prototypes.Characters.Subclasses`  
**Type:** `[Serializable]`

Pronoun set system for character dialogue and text generation.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Singular` | `string` | Subject pronoun (they/she/he) |
| `PossessiveAdjective` | `string` | Possessive adjective (their/her/his) |
| `PossessivePronoun` | `string` | Possessive pronoun (theirs/hers/his) |
| `Objective` | `string` | Object pronoun (them/her/him) |

### Constructors

```csharp
Pronouns(string pronounType = "they")
```
Creates pronoun set from type: "they", "she", or "he".

```csharp
Pronouns()
```
Default constructor (uses "they" pronouns).

### Methods

```csharp
void SetPronounType(string pronounType)
```
Changes pronoun set. Accepts: "they", "she", "he" (case-insensitive).

```csharp
string Get(string pronounCase)
```
Gets specific pronoun by case name.
- **Parameters:** `pronounCase` - Case name or pronoun type
- **Accepts:** "singular", "they", "possessiveadjective", "their", "possessivepronoun", "theirs", "objective", "them"
- **Returns:** Requested pronoun

```csharp
string Use(string text)
```
Replaces pronoun placeholders in text.
- **Placeholders:** `{they}`, `{them}`, `{their}`, `{theirs}`
- **Example:** `"I saw {them} with {their} friend"` → `"I saw him with his friend"`

### Built-in Pronoun Sets

| Type | Singular | Possessive Adj. | Possessive Pronoun | Objective |
|------|----------|-----------------|-------------------|-----------|
| they | they | their | theirs | them |
| she | she | her | hers | her |
| he | he | his | his | him |

### Usage

```csharp
var pronouns = new Pronouns("she");
string text = pronouns.Use("{they} found {their} sword");
// Result: "she found her sword"

// Direct access
string subject = pronouns.Singular; // "she"
string possessive = pronouns.PossessiveAdjective; // "her"
```

---

## SupportRelationship

**Namespace:** `Assets.Prototypes.Characters.Subclasses`  
**Type:** `[Serializable]`

Tracks relationship between two characters with support levels and progression.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Character` | `Character` | `null` | Related character |
| `CurrentLevel` | `SupportLevels.Level` | `None` | Current support rank |
| `MaxLevel` | `SupportLevels.Level` | `S` | Maximum achievable rank |
| `SupportPoints` | `int` | `0` | Progress points toward next level |
| `SupportSpeed` | `int` | `1` | Point gain rate multiplier |

### Support Levels

**SupportLevels.Level** enum:
- `None` - No relationship
- `C` - Initial support level
- `B` - Growing relationship
- `A` - Strong bond
- `S` - Special/romantic relationship

### Usage

```csharp
// Create relationship
var support = new SupportRelationship {
    Character = otherCharacter,
    CurrentLevel = SupportLevels.Level.C,
    MaxLevel = SupportLevels.Level.A,
    SupportPoints = 50,
    SupportSpeed = 2
};

// On Character
character.AddSupportRelationship(otherCharacter);
var rel = character.GetSupportRelationship(otherCharacter);
rel.SupportPoints += 10 * rel.SupportSpeed;
```

---

## HereditaryTraits

**Namespace:** `Assets.Prototypes.Characters.Subclasses`  
**Type:** `[Serializable]`

Traits and characteristics passed from parent to child units.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `HairColor` | `Color` | Inherited hair color |
| `EyeColor` | `Color` | Inherited eye color |
| `SkinTone` | `Color` | Inherited skin tone |
| `PassedSkills` | `List<string>` | Skills inherited by children |
| `PassedTraits` | `List<string>` | Character traits inherited |
| `GrowthRateBonuses` | `Dictionary<string, float>` | Stat growth modifiers |

### Usage

```csharp
character.PassedDownTraits.HairColor = Color.black;
character.PassedDownTraits.PassedSkills.Add("Swordfaire");

// When creating child
Character child = CreateChildUnit(parent1, parent2);
child.ApplyHereditaryTraits(parent1.PassedDownTraits);
```

---

## CharacterWhich

**Namespace:** `Assets.Prototypes.Characters.Configuration`  
**Type:** `enum`

Character allegiance/type identifier.

### Values

| Value | Description |
|-------|-------------|
| `Player` | Player-controlled unit |
| `Enemy` | Enemy/hostile unit |
| `Ally` | Allied/friendly NPC unit |
| `Neutral` | Neutral/non-combatant |

### Usage

```csharp
if (character.Which == CharacterWhich.Enemy) {
    // Enemy behavior
}
```

---

## SerializableDictionary<TKey, TValue>

**Type:** `[Serializable]`

Unity-serializable dictionary wrapper for inspector editing.

### Usage

```csharp
[SerializeField]
private SerializableDictionary<string, int> classExps;

// Access like normal dictionary
classExps["Fighter"] = 100;
int exp = classExps["Mage"];
```

### Notes
- Allows dictionaries in Unity inspector
- Custom property drawer for editing
- Maintains dictionary semantics

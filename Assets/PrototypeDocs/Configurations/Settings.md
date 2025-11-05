# Settings ScriptableObjects

## CharacterPrototypeSettings

**Namespace:** `Assets.Prototypes.Characters`  
**Inherits:** `ScriptableObject`  
**Location:** `Resources/GameSettings/CharacterPrototypeSettings`

Global settings for character system.

### Creation
```csharp
Assets > Create > Game Settings > CharacterPrototypeSettings
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `UseAccentColors` | `bool` | `true` | Enable accent colors on new characters |

### Behavior

**OnValidate (Editor Only):**
- Automatically updates all `Character` assets when settings change
- Finds all Character assets via AssetDatabase
- Marks them dirty and saves changes
- Deferred to avoid import conflicts

### Usage

```csharp
// Loaded automatically by Character.OnEnable()
var settings = Resources.Load<CharacterPrototypeSettings>(
    "GameSettings/CharacterPrototypeSettings"
);

if (settings.UseAccentColors) {
    // Enable tinting
}
```

### Notes
- Changes propagate to all existing Character assets
- Update runs on next editor frame (via `EditorApplication.delayCall`)
- Skips update during asset import worker processes

---

## GraphicsPrototypesSettings

**Inherits:** `ScriptableObject`  
**Location:** `Resources/GraphicsPrototypesSettings`

Global settings for graphics and portrait rendering.

### Creation
```csharp
Assets > Create > Game Settings > GraphicsPrototypesSettings
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `portraitRenderWidth` | `int` | `512` | Portrait texture width in pixels |
| `portraitRenderHeight` | `int` | `512` | Portrait texture height in pixels |

### Behavior

**OnValidate (Editor Only):**
- Updates all `ImageStack` assets when settings change
- Marks stacks dirty for re-rendering
- Deferred to avoid import conflicts

### Usage

```csharp
var settings = Resources.Load<GraphicsPrototypesSettings>(
    "GraphicsPrototypesSettings"
);

int width = settings.portraitRenderWidth;
int height = settings.portraitRenderHeight;

Texture2D texture = new Texture2D(width, height);
```

### Notes
- Used by `ImageStack.PreRender()` for texture dimensions
- Changes affect all portrait rendering
- All image stacks automatically updated on change

---

## Settings Best Practices

### File Organization
```
Assets/
  Resources/
    GameSettings/
      CharacterPrototypeSettings.asset
    GraphicsPrototypesSettings.asset
```

### Loading Pattern
```csharp
// Always load from Resources
var settings = Resources.Load<SettingsType>("Path/In/Resources");

// Check for null
if (settings != null) {
    // Use settings
}
```

### Modification Flow
1. Edit settings asset in inspector
2. `OnValidate()` triggered automatically
3. Related assets marked dirty
4. Changes saved via `AssetDatabase.SaveAssets()`
5. All references updated on next load

### Performance
- Settings loaded once per asset initialization
- Minimal runtime overhead
- Editor-only validation logic wrapped in `#if UNITY_EDITOR`

### Thread Safety
- `IsAssetImportWorkerProcess()` check prevents worker conflicts
- `EditorApplication.delayCall` ensures main thread execution
- Safe for concurrent editor operations

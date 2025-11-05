# Portrait Editor Window

**Namespace:** `Assets.Prototypes.Characters.Subclasses.Editor`  
**Inherits:** `EditorWindow`  
**Menu:** `Tools > Portrait Editor`

Editor tool for creating and editing character portraits with real-time preview.

## Opening

```csharp
// Via menu
Tools > Portrait Editor

// Via code
var window = EditorWindow.GetWindow<PortraitEditorWindow>();
window.Show();
```

## Features

### Character Selection
- Dropdown list of all Character assets in project
- Portrait index selector for multi-portrait characters
- Create new portraits on existing characters

### Portrait Configuration

**ImageStack Assignment:**
- ObjectField for assigning/creating ImageStack assets
- Button to create new ImageStack
- Reflection-based private field assignment

**Key Management:**
- Text field for portrait filename key
- "Generate New Key" button (GUID-based)
- Validates key before save

**Portrait Identity:**
- Displays unique portrait ID
- Shows owner character name
- Instance verification for debugging

### Layer Management

**Layer List:**
- Reorderable list of ImageStackLayers
- Add/Delete layer buttons
- Per-layer controls:
  - Sprite assignment
  - Mask assignment
  - Offset (Vector2)
  - Scale (float)
  - Rotation (float, not yet implemented in compositor)
  - Order (int, render priority)

### Tint Colors

**Color Configuration:**
- 3 color pickers for tint colors
- Labels: "Tint 1/2/3 (Red/Green/Blue Channel)"
- "Update from Character Colors" button
- Auto-sync with character accent colors

**Color Workflow:**
1. Manual editing in window
2. Or pull from Character.AccentColor1/2/3
3. Colors preserved until manually changed
4. Not overwritten on save

### Preview

**Live Preview:**
- Real-time composited portrait display
- 256x256 preview window
- Updates on any change (if auto-refresh enabled)
- Manual "Refresh Preview" button

**Auto-Refresh Toggle:**
- Checkbox to enable/disable auto-updates
- Prevents performance issues with many layers

### Save & Render

**"Save & Render Portrait" Button:**
- Validates key (must not be empty)
- Calls `Portrait.Render()`
- Saves PNG to disk
- Creates sprite asset
- Updates SavedSprite reference
- Success/error dialogs

## Window Layout

```
┌─────────────────────────────────────┐
│ Character: [Dropdown]               │
│ Portrait Index: [0]                 │
│                                     │
│ ┌─────────────────────────────────┐ │
│ │      Live Preview (256x256)     │ │
│ │          [Preview Image]        │ │
│ └─────────────────────────────────┘ │
│                                     │
│ Portrait ID: p{guid}                │
│ Owner: CharacterName                │
│                                     │
│ ImageStack: [ObjectField]           │
│ [Create New ImageStack]             │
│ [Initialize Portrait]               │
│                                     │
│ Key: [text field]                   │
│ [Generate New Key]                  │
│                                     │
│ === Layers ===                      │
│ [Add Layer] [Refresh Preview]       │
│ ┌─────────────────────────────────┐ │
│ │ Layer 0:                        │ │
│ │   Sprite: [ObjectField]         │ │
│ │   Mask: [ObjectField]           │ │
│ │   Offset: (0, 0)                │ │
│ │   Scale: 1.0                    │ │
│ │   Order: 0                      │ │
│ │   [Delete]                      │ │
│ └─────────────────────────────────┘ │
│                                     │
│ === Tint Colors ===                 │
│ Tint 1 (Red): [Color]               │
│ Tint 2 (Green): [Color]             │
│ Tint 3 (Blue): [Color]              │
│ [Update from Character Colors]      │
│                                     │
│ [Save & Render Portrait]            │
│                                     │
│ Auto Refresh: [✓]                   │
└─────────────────────────────────────┘
```

## API

### Public Methods

```csharp
static void ShowWindow()
```
Opens/focuses the Portrait Editor window.

### Key Features

**Reflection Usage:**
- Sets private `_imageStack` field on Portrait
- Bypasses public API restrictions
- Enables editor-only configuration

**Asset Creation:**
- Creates ImageStack assets with save dialog
- Generates unique portrait keys
- Initializes Portrait instances with proper GUID

**Live Editing:**
- All changes immediately reflected in preview
- Undo/Redo support via `EditorUtility.SetDirty()`
- Asset modifications auto-saved

## Workflow

### Creating New Portrait

1. Select character from dropdown
2. Click "Initialize Portrait" at unused index
3. Assign or create ImageStack
4. Set portrait key (or generate)
5. Add layers (sprites + masks)
6. Adjust layer transforms (offset, scale, order)
7. Set tint colors
8. Preview updates automatically
9. Click "Save & Render Portrait"
10. PNG saved and sprite asset created

### Editing Existing Portrait

1. Select character
2. Select portrait index
3. Modify layers/colors/transforms
4. Preview updates
5. Save when satisfied

### Layer Ordering

- Layers rendered by `Order` property (low to high)
- Lower order = rendered first (behind)
- Higher order = rendered last (in front)
- Negative orders allowed

## Technical Details

### Performance Optimization
- Compositing triggered only on changes (if auto-refresh off)
- Preview resolution lower than save resolution (256px vs 512px)
- Layer validation before render

### Error Handling
- Null checks for character/portrait/imagestack
- Validation dialogs for missing data
- Console logging for debugging
- Try-catch on save operation

### Integration Points
- `Portrait.CompositeLayers()` - Generates preview
- `Portrait.Render()` - Saves to disk
- `ImageCompositor.CompositeImageStackLayers()` - Core rendering
- `AssetDatabase` - Asset management

## Limitations

- Rotation not yet implemented in compositor
- Single undo operation per save
- Preview quality lower than final render
- No zoom/pan in preview window

## Notes

- Window state persists across domain reload
- Multi-portrait characters supported
- Safe to edit during play mode (editor-only)
- Changes affect asset files directly

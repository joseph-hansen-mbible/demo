# StackedImageEditorWindow

**Namespace:** `Turnroot.Graphics2D.Editor`  
**Type:** Generic abstract base class for editing `StackedImage<TOwner>`

Reusable editor window providing UI for layer management, tinting, preview, and rendering. Inherit and implement 3 abstract members to create a custom editor.

## Type Parameters

- `TOwner` (constraint: `UnityEngine.Object`) - Owner type (CharacterData, ItemData, etc.)
- `TStackedImage` (constraint: `StackedImage<TOwner>`) - Concrete StackedImage implementation

## Quick Start

``csharp
using Turnroot.Graphics2D.Editor;
using UnityEditor;

public class PortraitEditorWindow : StackedImageEditorWindow<CharacterData, Portrait>
{
    protected override string WindowTitle => "Portrait Editor";
    protected override string OwnerFieldLabel => "Character";
    
    [MenuItem("Window/Portrait Editor")]
    public static void ShowWindow() => GetWindow<PortraitEditorWindow>("Portrait Editor");
    
    protected override Portrait[] GetImagesFromOwner(CharacterData owner) => owner?.Portraits;
}
``
## Abstract Members (Required)

### Properties
- `string WindowTitle { get; }` - Window title ("Portrait Editor")
- `string OwnerFieldLabel { get; }` - Owner field label ("Character")

### Methods
- `TStackedImage[] GetImagesFromOwner(TOwner owner)` - Returns image array from owner

## Protected Fields (Available to Derived Classes)

| Field | Type | Description |
|-------|------|-------------|
| `_currentOwner` | `TOwner` | Selected owner object |
| `_currentImage` | `TStackedImage` | Selected image being edited |
| `_selectedImageIndex` | `int` | Selected image index |
| `_autoRefresh` | `bool` | Auto-refresh preview on changes |
| `_selectedLayerIndex` | `int` | Selected layer index (-1 = none) |

## Virtual Methods (Optional Overrides)

Override these to customize behavior:

- `OnGUI()` - Main GUI rendering
- `DrawControlPanel()` - Left controls panel
- `DrawPreviewPanel()` - Right preview panel
- `DrawImageMetadataSection()` - ID/key editing
- `DrawImageStackSection()` - ImageStack assignment
- `DrawOwnerSection()` - Owner info
- `DrawTintingSection()` - Tint colors
- `DrawLayerManagementSection()` - Layer list

## Protected Helper Methods

- `UpdateCurrentImage()` - Refresh current image from owner
- `RefreshPreview()` - Recomposite and update preview

## Features Included

**Automatic:**
- Owner/image selection with validation
- Live preview with auto-refresh toggle
- Layer visualization and selection
- Tint color editing (3 channels)
- ImageStack assignment
- Key generation (short/full GUID)
- **Default management**: Save/load tagged layer defaults
- Render to file + asset import
- Saved sprite display

**UI Layout:**
```
+-------------------------------+
� Window Title                   �
+-------------------------------�
� Controls    � Preview         �
� - Metadata  �  [Save Defaults] �
� - Stack     �  [Load Defaults] �
� - Owner     �  [Image]        �
� - Tints     �  [Refresh]      �
� - Layers    �  [Saved Sprite] �
�             �  [Render]       �
+-------------------------------+
```

## Customization Examples

### Add Custom Controls
``csharp
protected override void DrawControlPanel()
{
    base.DrawControlPanel();
    EditorGUILayout.Space(10);
    if (GUILayout.Button("Custom Action"))
    {
        // Your logic
    }
}
``

### Add Static Opener
``csharp
public static void OpenPortrait(CharacterData character, int index = 0)
{
    var window = GetWindow<PortraitEditorWindow>();
    window._currentOwner = character;
    window._selectedImageIndex = index;
    window.UpdateCurrentImage();
    window.RefreshPreview();
}
``

## Architecture

**Template Method Pattern:**
- Base class provides workflow (template)
- Abstract methods are customization points
- Derived classes implement type-specific details

**Benefits:**
- ~600 lines shared, ~30 lines per editor
- Bug fixes apply to all editors
- New features added once
- Type-safe with compile-time checking

---

## See Also

- **[StackedImage](../Graphics2D/StackedImage.md)** - Base image class
- **[Portrait](../Characters/Portrait.md)** - Character implementation

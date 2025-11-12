# Portrait Editor Window

**Namespace:** `Turnroot.Characters.Subclasses.Editor`  
**Inherits:** `StackedImageEditorWindow<CharacterData, Portrait>`  
**Menu:** `Window > Portrait Editor`

Editor tool for creating and editing character portraits with real-time preview. This is a concrete implementation of the generic `StackedImageEditorWindow` base class.

## Architecture

The `PortraitEditorWindow` is a minimal implementation (~30 lines) that extends `StackedImageEditorWindow<CharacterData, Portrait>`. All UI and functionality is inherited from the base class.

### Implementation

## Window Layout

### When No Portraits Exist
+-------------------------------------+
| Character: [Dropdown]               |
|                                     |
| Info: This Character has no portraits. |
|                                     |
| [Portrait Name Text Field] [Create] |
+-------------------------------------+

### When Portraits Exist
+-------------------------------------+
| Character: [Dropdown]               |
| Select Portrait: [Dropdown] [Name] [New +] |
|                                     |
| [Save Character Defaults] [Load Defaults] |
|                                     |
| +-------------------------------+ |
| |      Live Preview (256x256)     | |
| |          [Preview Image]        | |
| +-------------------------------+ |
|                                     |
| === Image Stack Assignment ===     |
| ImageStack: [ObjectField]          |
| [Create New ImageStack]            |
|                                     |
| === Layers ===                     |
| [Add Layer] [Refresh Preview]      |
| +-------------------------------+ |
| | Layer 0:                        | |
| |   Sprite: [ObjectField]         | |
| |   Mask: [ObjectField]           | |
| |   Offset: (0, 0)                | |
| |   Scale: 1.0                    | |
| |   Rotation: 0deg                  | |
| |   Order: 0                      | |
| |   Tint: [Color]                 | |
| |   Tag: [Text]                   | |
| |   [Remove]                      | |
| +-------------------------------+ |
|                                     |
| === Tint Colors ===                |
| Tint 1 (Red): [Color]              |
| Tint 2 (Green): [Color]            |
| Tint 3 (Blue): [Color]             |
| [Update from Character]            |
|                                     |
| Auto Refresh: [x]                  |
+-------------------------------------+
```

## Workflow

### Creating First Portrait

1. Select character from dropdown
2. Enter portrait name in text field (auto-filled with "{CharacterName}_Portrait")
3. Click "Create" button
4. Choose save location for new ImageStack asset
5. Portrait created with auto-generated ImageStack
6. Add layers (sprites + masks) in the layers panel
7. Adjust layer properties (offset, scale, rotation, order, tint)
8. Preview updates automatically
9. Layers are composited and saved when modified

### Adding Additional Portraits

1. Select character from dropdown
2. Use portrait dropdown to select existing portrait
3. Enter new portrait name in text field (auto-filled with "{CharacterName}_Portrait{N}")
4. Click "New +" button
5. Choose save location for new ImageStack asset
6. New portrait created and selected automatically
7. Follow steps 6-9 above

### Editing Existing Portrait

1. Select character from dropdown
2. Select portrait from dropdown
3. Modify layers in the layers panel
4. Adjust layer properties as needed
5. Preview updates automatically

### Managing Defaults

**Save Character Defaults**
- Saves current layer configurations for all tagged layers across all portraits
- Captures Sprite, Offset, Scale, and Tint values for layers with tags
- Useful for establishing baseline configurations

**Load Defaults**
- Restores saved default configurations for tagged layers
- Applies saved values to matching tagged layers across all portraits
- Useful for resetting portraits to known good states

### Layer Ordering

- Layers rendered by `Order` property (low to high)
- Lower order = rendered first (behind)
- Higher order = rendered last (in front)
- Negative orders allowed

## Technical Details

### Inherited Functionality

All features are provided by `StackedImageEditorWindow<TOwner, TStackedImage>`:

- **Image Selection**: Dropdown to select from owner's image array
- **Metadata Editing**: Key (filename) editing and generation
- **Image Stack Management**: Assign ImageStack, view layers
- **Owner Management**: View/update owner reference
- **Tint Colors**: Edit 3 tint colors, reset, update from owner
- **Layer Inspection**: View layer properties (sprite, mask, order, offset, scale, rotation)
- **Preview**: Real-time compositing with auto-refresh toggle
- **Render & Save**: Export to PNG and create sprite asset

### Creating Custom Editors

To create a new stacked image editor for a different owner type:

```csharp
public class MyImageEditorWindow : StackedImageEditorWindow<MyOwnerType, MyImageType>
{
    protected override string WindowTitle => "My Image Editor";
    protected override string OwnerFieldLabel => "My Owner";
    
    [MenuItem("Window/My Image Editor")]
    public static void ShowWindow()
    {
        GetWindow<MyImageEditorWindow>("My Image Editor");
    }
    
    protected override MyImageType[] GetImagesFromOwner(MyOwnerType owner)
    {
        return owner?.Images;
    }
}
```

---

## See Also

- **[StackedImage](../Characters/Portraits/StackedImage.md)** - Base stacked image system
- **[Portrait](../Characters/Portraits/Portrait.md)** - Character-specific implementation
- **[ImageStack](../Characters/Portraits/ImageStack.md)** - Layer container
- **[ImageCompositor](ImageCompositor.md)** - Compositing utility
- `ImageCompositor.CompositeImageStackLayers()` - Core rendering
- `AssetDatabase` - Asset management

## Limitations

- Rotation not yet implemented in compositor
- Single undo operation per save
- Preview quality lower than final render
- No zoom/pan in preview window
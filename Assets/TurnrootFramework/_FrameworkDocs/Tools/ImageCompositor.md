# ImageCompositor

**Namespace:** `Assets.AbstractScripts.Graphics2D`  
**Type:** `static class`

Static utility for compositing sprite layers with tinting, scaling, and alpha blending.

## Methods

### Sprite Creation
```csharp
static Sprite CreateSpriteFromTexture(Texture2D texture)
```
Creates a sprite from a texture with centered pivot.
- **Parameters:** `texture` - Source texture
- **Returns:** Sprite or `null` if texture is null

### Tinting
```csharp
static Color[] TintSpritePixels(Sprite sprite, Sprite mask, Color[] tints)
```
Applies color tinting to sprite pixels based on mask RGB channels.

**Parameters:**
- `sprite` - Source sprite to tint
- `mask` - RGB mask defining tint regions
- `tints` - Array of 3 colors (Red, Green, Blue channels)

**Returns:** Tinted pixel array or `null` on error

**Algorithm:**
1. For each pixel, read mask RGB values (0-1)
2. Calculate `totalStrength = R + G + B`
3. Normalize weights: `rWeight = R/total`, etc.
4. Blend tints: `result = tints[0]*rWeight + tints[1]*gWeight + tints[2]*bWeight`
5. Lerp original color to blended tint by `totalStrength`
6. Preserve original alpha

**Requirements:**
- Sprite and mask must be same dimensions
- Both textures must have Read/Write enabled
- `tints` array must contain at least 3 colors

### Compositing
```csharp
static Texture2D CompositeImageStackLayers(
    Texture2D baseTexture,
    ImageStackLayer[] layers,
    Sprite[] masks = null,
    Color[] tints = null
)
```
Composites multiple sprite layers with transformations and tinting.

**Parameters:**
- `baseTexture` - Base texture to composite onto (typically transparent)
- `layers` - Array of ImageStackLayers to composite
- `masks` - Optional per-layer tint masks
- `tints` - Optional tint colors (applies to all layers)

**Returns:** Composited texture or `null` on error

**Process:**
1. Sorts layers by `Order` property (ascending)
2. For each layer:
   - Applies tinting if mask provided
   - Scales sprite
   - Applies offset
   - Alpha blends onto base texture
3. Returns final composited texture

**Scaling:**
- Uses nearest-neighbor sampling
- Iterates destination pixels to avoid gaps on upscale
- Maps back to source texture coordinates

**Alpha Blending:**
```
finalColor = layerColor * layerAlpha + baseColor * (1 - layerAlpha)
```

### Utility
```csharp
static bool IsTextureReadable(Texture2D texture)
```
Checks if texture has Read/Write enabled.
- **Returns:** `true` if readable

## Usage Examples

### Basic Compositing
```csharp
Texture2D base = new Texture2D(512, 512, TextureFormat.RGBA32, false);
// Clear to transparent...

ImageStackLayer[] layers = GetLayers();
Texture2D result = ImageCompositor.CompositeImageStackLayers(
    base, 
    layers
);
```

### With Tinting
```csharp
Color[] tints = new Color[] {
    Color.red,    // R channel
    Color.green,  // G channel
    Color.blue    // B channel
};

Sprite[] masks = GetMasksForLayers();

Texture2D result = ImageCompositor.CompositeImageStackLayers(
    baseTexture,
    layers,
    masks,
    tints
);
```

### Single Layer Tinting
```csharp
Color[] tintedPixels = ImageCompositor.TintSpritePixels(
    hairSprite,
    hairMask,
    new Color[] { Color.red, Color.blue, Color.yellow }
);

Texture2D tintedTexture = new Texture2D(width, height);
tintedTexture.SetPixels(tintedPixels);
tintedTexture.Apply();
```

## Performance Notes
- `GetPixels()` operations are expensive - minimize calls
- Tinting is O(pixels) per layer
- Scaling uses simple nearest-neighbor (fast but can show pixelation)
- All operations CPU-based (no GPU acceleration)

## Error Handling
Returns `null` and logs error if:
- Texture is not readable
- Mask dimensions don't match sprite
- Parameters are null
- Tint array has fewer than 3 colors

# Documentation & Development Guide

## Project Vision
Tactical RPG with character customization, strategic combat, and visual node-based skill behaviors.

## Core Rule: Document Only What Exists
**NEVER** invent methods, properties, enum values, or usage patterns. **ALWAYS** verify against source code first.

## Documentation Location
- **Base**: `Assets/PrototypeDocs/`
- **Entry**: `README.md` (index with quick reference)

## Key Systems

### Character System (`Assets/Prototypes/Characters/`)
- **Stats**: Bounded (HP, Stamina with min/max) & Unbounded (Strength, Speed)
- **Portraits**: Compositable layered graphics with RGB mask tinting
- **Inventory**: Multi-slot equipment (Weapon/Shield/Accessory)
- **Components**: Pronouns, relationships, traits, demographics
- **Docs**: Character.md, CharacterStats.md, Portrait.md, CharacterInventory.md

### Skills & Node System (`Assets/Prototypes/Skills/`)
- **Skill**: ScriptableObject with AccentColors, Badge, UnityEvents
- **SkillBadge**: StackedImage<Skill> for compositable badge graphics
- **Node System**: xNode-based visual execution graphs for skill behaviors
  - **SkillGraph**: NodeGraph container, executes entry nodes
  - **SkillNode**: Base class for all nodes with Execute(BattleContext)
  - **BattleContext**: Runtime battle data (UnitInstance, Targets, Allies, AdjacentUnits, CustomData, EnvironmentalConditions, SkillUseCount)
  - **Port Types**: ExecutionFlow (violet), FloatValue (teal), BoolValue (sky), StringValue (orange)
  - **Categories**: Flow/Math/Events/Conditions (auto-tint by folder)
- **Docs**: Skill.md, SkillBadge.md, Nodes/README.md

### Combat Mechanics
- **Follow-up Attacks**: Trigger when Speed difference ≥ speed threshold
- **Attack Order**: Speed determines who attacks first
- **Critical Hits**: Flag-based trigger for combat system
- **Turn-Based Strategy**: Grid movement, positioning

### Graphics & Configuration
- **StackedImage<TOwner>**: Abstract base for compositable layered images
- **ImageCompositor**: Static utility for tinting/compositing/scaling
- **Settings**: CharacterPrototypeSettings, GraphicsPrototypesSettings, DefaultCharacterStats
- **Docs**: StackedImage.md, ImageCompositor.md, Settings.md

## Development Patterns

### Skill Node Development
```csharp
[CreateNodeMenu("Events/Action Name")]
[NodeLabel("Action description")]
public class ActionNode : SkillNode
{
    [Input] public ExecutionFlow executionIn;
    [Input] public FloatValue inputValue;
    [Input] public BoolValue affectAllTargets; // For multi-target support
    
    [Tooltip("Test value for editor mode")]
    public float testValue = 10f;
    public bool testAffectAll = false;
    
    public override void Execute(BattleContext context) {
        // Get connected values or use test defaults with helper methods
        float value = GetInputFloat("inputValue", testValue);
        bool affectAll = GetInputBool("affectAllTargets", testAffectAll);
        
        // Access context: context.UnitInstance (caster), context.Targets (enemies), context.Allies
        // Spatial: context.AdjacentUnits[Direction.TopLeft]
        // Store results: context.SetCustomData("key", value)
        
        // Continue execution
        var flow = GetOutputValue("output", output);
        if (flow?.node != null) ((SkillNode)flow.node).Execute(context);
    }
}
```

### Event Nodes (`Assets/Prototypes/Skills/Nodes/Nodes/Events/`)
**Completed nodes**: AffectUnitStat, AffectEnemyStat, AffectAdjacentAllyStat, KillTarget, DealAdditionalDamage, AreaOfEffectDamage, CriticalHit, ChangeBattleOrder, DealDebuff, DealDebuffAreaOfEffect, DisableEnemyFollowup, UnmountEnemy, GainGold, Steal, MoveUnit, SwapUnitWithTarget, TakeAnotherTurn, AffectUnitWeaponUses

**Patterns**:
- Multi-target: `BoolValue affectAllTargets` input + `testAffectAll` field
- Helper methods: Use `GetInputFloat(portName, testValue)` and `GetInputBool(portName, testValue)`
- Stat modification: Use `character.GetBoundedStat(type).SetCurrent(value)`
- Placeholders: Mark incomplete integrations with `TODO:`, use `*Placeholder` field names
- CustomData keys: Use pattern `$"ActionName_{character.Id}"` for per-character state

### Stat Modification
```csharp
// Bounded stats (HP, Stamina with min/max)
var stat = character.GetBoundedStat(BoundedStatType.Health);
stat.SetCurrent(stat.Current + change);

// Unbounded stats (Strength, Speed)
var stat = character.GetUnboundedStat(UnboundedStatType.Strength);
stat.SetCurrent(stat.Current + change);
```

### Node Editor Pattern
- Base: `AffectStatNodeEditorBase` for stat-affecting nodes
- Override `OnBodyGUI()` for custom layouts
- Use `NodeEditorGUILayout.PortField(target.GetInputPort("name"))`
- Stat dropdowns from `DefaultCharacterStats.Instance`


## Documentation Structure

```
PrototypeDocs/
├── README.md                    # Index & quick reference
├── Characters/                  # Character system
│   ├── Character.md             # Main character asset
│   ├── CharacterStats.md        # CharacterStat + BoundedCharacterStat
│   ├── CharacterComponents.md   # Pronouns, SupportRelationship, HereditaryTraits
│   ├── CharacterInventory.md    # Multi-slot equipment system
│   ├── DefaultCharacterStats.md # Default stat initialization
│   └── Portraits/               # Portrait sub-system
│       ├── Portrait.md          # Compositable portraits
│       └── ImageStack.md        # ImageStack + ImageStackLayer
├── Skills/                      # Skills system
│   ├── Skill.md                 # Skill asset
│   ├── SkillBadge.md            # SkillBadge (StackedImage<Skill>)
│   ├── SkillBadgeEditorWindow.md
│   └── Nodes/                   # xNode skill execution
│       └── README.md            # Node architecture & execution flow
├── Graphics2D/
│   └── StackedImage.md          # StackedImage<TOwner> base class
├── Configurations/
│   ├── Settings.md              # CharacterPrototypeSettings + Graphics
│   └── Components/
│       └── ExperienceTypes.md   # ExperienceType + WeaponType
└── Tools/                       # Editor tools
    ├── PortraitEditorWindow.md
    ├── StackedImageEditorWindow.md
    └── ImageCompositor.md
```

## Source to Doc Mapping

| Doc File | Source Files | Key Topics |
|----------|--------------|------------|
| Character.md | `Characters/Character.cs` | Properties, stats access, support relationships |
| CharacterStats.md | `Characters/Components/Stats/*.cs` | CharacterStat, BoundedCharacterStat, enums |
| CharacterInventory.md | `Characters/Components/Inventory/*.cs` | Equipment slots, add/remove/equip/unequip |
| Portrait.md | `Characters/Components/Portrait.cs` | Rendering pipeline, tinting, compositing |
| ImageStack.md | `Graphics2D/Components/Portrait/ImageStack*.cs` | Layer container, ImageStackLayer properties |
| Skill.md | `Skills/Skill.cs` | AccentColors, Badge, UnityEvents |
| SkillBadge.md | `Skills/Components/Badges/SkillBadge.cs` | StackedImage<Skill> implementation |
| Skills/Nodes/README.md | `Skills/Nodes/Core/*.cs` | SkillGraph, SkillNode, execution flow, port types |
| StackedImage.md | `Graphics2D/Components/StackedImage.cs` | Generic base, rendering pipeline, abstract methods |
| ImageCompositor.md | `AbstractScripts/Graphics2D/ImageCompositor.cs` | Tinting algorithm, compositing, scaling |
| Settings.md | `Characters/CharacterPrototypeSettings.cs`<br>`Graphics2D/GraphicsPrototypesSettings.cs` | OnValidate, settings propagation |

## Update Protocol (Condensed)

1. **Read source code FIRST** - Never document without verifying
2. **Identify affected docs** using table above
3. **Update only verified info**:
   - Add new properties/methods that exist
   - Remove documented features that don't exist
   - Update examples to use only existing APIs
4. **Update README.md** if new system added or architecture changed
5. **Update this file** if new doc file added

### Common Mistakes to Avoid
- ❌ "Health, Stamina, etc." when only Health exists
- ❌ Examples setting read-only properties
- ❌ Documenting non-existent methods
- ✅ List actual enum values completely
- ✅ Note read-only properties
- ✅ Verify all method signatures



## Skill Node Development (xNode System)

### Node Categories
- **Values**: Provide data (FloatValue, BoolValue, StringValue)
- **Events**: Perform actions (DealDamage, CriticalHit, AffectStats, etc.)
- **Comparisons**: Test conditions (compare stats, evaluate booleans)
- **Flow**: Control execution (branches, loops, delays)

### Creating New Event Nodes

**Template**:
```csharp
using Assets.Prototypes.Skills.Nodes.Core;
using Assets.Prototypes.Skills.Nodes.Core.Attributes;
using XNode;

namespace Assets.Prototypes.Skills.Nodes.Events
{
    [NodeWidth(300)]
    [CreateNodeMenuAttribute("Events/YourNode")]
    public class YourNode : SkillNode
    {
        [Input(connectionType: ConnectionType.Multiple)] 
        public ExecutionFlow input;

        [Output] 
        public ExecutionFlow output;

        // Dynamic input with test fallback
        [Input(connectionType: ConnectionType.Override)] 
        public FloatValue myValue;
        
        [Tooltip("Test value when myValue unconnected")]
        public float testMyValue = 1.0f;

        public override void Execute(SkillExecutionContext context)
        {
            float value = GetInputValue("myValue", testMyValue);
            
            // Your logic here using:
            // - context.UnitInstance (caster)
            // - context.Targets (enemy list)
            // - context.CustomData (shared state)
            
            ExecutionFlow outFlow = GetOutputValue("output", output);
            outFlow?.Execute(context);
        }
    }
}
```

**Custom Editor** (if needed):
```csharp
using XNodeEditor;
using UnityEditor;

namespace Assets.Prototypes.Skills.Nodes.Events.Editor
{
    [CustomNodeEditor(typeof(YourNode))]
    public class YourNodeEditor : NodeEditor
    {
        public override void OnBodyGUI()
        {
            serializedObject.Update();
            NodeEditorGUILayout.PortField(target.GetInputPort("input"));
            
            // Custom UI here (dropdowns, buttons, etc.)
            
            NodeEditorGUILayout.PortField(target.GetOutputPort("output"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
```

### Multi-Target Pattern (affect all enemies)
```csharp
[Input(connectionType: ConnectionType.Override)] 
public BoolValue affectAllTargets;

[Tooltip("Test: affect all when input unconnected")]
public bool testAffectAll = false;

public override void Execute(BattleContext context) {
    bool shouldAffectAll = GetInputBool("affectAllTargets", testAffectAll);
    
    if (shouldAffectAll) {
        foreach (var target in context.Targets) {
            // Apply to each target
        }
    } else {
        var target = context.Targets[0];  // First target only
    }
    
    // Continue execution
    var flow = GetOutputValue("output", output);
    if (flow?.node != null) ((SkillNode)flow.node).Execute(context);
}
```

### Stat Modification Pattern
```csharp
// Bounded stats (Health, Stamina)
var stat = context.UnitInstance.BoundedStats
    .FirstOrDefault(s => s.Type == BoundedStatType.Health);

if (stat != null) {
    float change = GetInputValue("changeValue", testChange);
    stat.SetCurrent(stat.Current + (int)change);
}

// Unbounded stats (Strength, Speed, etc.)
var stat = context.UnitInstance.UnboundedStats
    .FirstOrDefault(s => s.Type == UnboundedStatType.Strength);

if (stat != null) {
    float change = GetInputValue("changeValue", testChange);
    stat.SetCurrent(stat.Current + (int)change);
}
```

### Port Color Reference
- **ExecutionFlow**: Violet
- **BoolValue**: Sky blue
- **FloatValue**: Teal
- **StringValue**: Orange

### Tactical RPG Mechanics Reference
- **Follow-up Attacks**: Trigger when attacker Speed ≥ defender Speed + speed threshold
- **Critical Hits**: Set `context.CustomData["IsCriticalHit"] = true` flag
- **Battle Order**: Can guarantee/prevent follow-ups or modify speed threshold
- **Turn-Based**: Characters act in Speed order, status effects persist until turn end

## VERIFICATION CHECKLIST

When updating docs, verify:
- [ ] **SOURCE CODE CHECKED FIRST** - Read actual source file before documenting
- [ ] **All enum values listed are real** - No invented values or "etc."
- [ ] **All properties exist in source** - Check actual class definition
- [ ] **All methods exist with correct signatures** - Verify parameters and return types
- [ ] **Usage examples only use existing APIs** - No calls to non-existent methods
- [ ] **Read-only properties not shown being set** - Respect property access modifiers
- [ ] All code examples use correct namespaces
- [ ] Property tables match actual serialized fields
- [ ] File paths in "Sources" sections are accurate
- [ ] Cross-references between docs still valid
- [ ] README.md updated if new system added
````

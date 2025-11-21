# CharacterData — quick reference

ScriptableObject describing a character. Stores identity, visuals, stats, inventory, skills, and relationships.

Key fields
- `AccentColor1/2/3`: used by portraits and badges
- `Portraits`: named portrait configurations (composite ImageStacks)
- `BoundedStats` / `UnboundedStats`: runtime stat containers
- `StartingInventory`: initial equipment

Integration
- Portrait rendering uses `AccentColor*` and `Portraits` to composite sprites
- Inventory integrates with `ObjectItem` and `ObjectSubtype`
- Skills use `Skill` assets referenced here

Common methods
- `GetBoundedStat(type)`, `GetUnboundedStat(type)` — stat accessors
- `SaveDefaults()` / `LoadDefaults()` — editor helpers for portrait layer defaults

Utilities
- Use `SupportRelationship.SanitizeForCharacter(owner, list)` to validate support lists and initialize defaults where necessary.

Where to look
- Source: `Assets/TurnrootFramework/Characters/CharacterData.cs`
- Portraits: `Assets/TurnrootFramework/Characters/Portraits`

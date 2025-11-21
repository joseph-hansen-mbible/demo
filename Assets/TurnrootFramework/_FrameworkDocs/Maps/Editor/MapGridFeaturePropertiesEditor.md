# MapGridFeaturePropertiesEditor — short ref

Editor for `MapGridFeatureProperties` assets; simplifies choosing a feature type and setting defaults.

What it is
- Custom inspector that lists available feature types and allows the developer to set `featureId` and friendly `featureName`.

What it does
- Provides a popup to choose FeatureType (e.g., Treasure, Door), which sets the `featureId` used by `MapGridPoint` to match defaults.
- Displays rest of the `MapGridFeatureProperties` typed property sections for default values.

Key behavior
- `OnInspectorGUI()` builds the UI and validates that `featureId` is not empty.

Where to look
- Source: `Assets/TurnrootFramework/Editor/MapGridFeaturePropertiesEditor.cs`

See also
- `Maps/Property Components/MapGridFeatureProperties.md` — default property asset
- `Maps/MapGridPoint.md` — Apply defaults when painting a feature

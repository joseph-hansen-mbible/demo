using UnityEngine;

public enum FeatureDisplay
{
    Icon,
    Initial,
}

[CreateAssetMenu(
    fileName = "MapGridEditorSettings",
    menuName = "Turnroot/Editor Settings/Map Grid Editor Settings"
)]
public class MapGridEditorSettings : ScriptableObject
{
    [Range(0, 3)]
    public int gridThickness = 1;
    public Color gridColor = Color.black;
    public FeatureDisplay featureDisplay = FeatureDisplay.Icon;

    [Header("UI Layout")]
    [Tooltip("Indentation in pixels for property keys under section headers")]
    public int propertyIndent = 12;

    [Header("Selection & Border Colors")]
    [Tooltip("Border color used for selected features.")]
    public Color selectedFeatureBorderColor = Color.magenta;

    [Tooltip("Border color used for tiles that have modified properties (not selected).")]
    public Color modifiedPropertyBorderColor = new Color(1f, 0.75f, 1f, 0.6f);

    [Header("Header Accent")]
    [Tooltip("Color used as background for section headers in the right-hand panel")]
    public Color headerAccentColor = new Color(0.0f, 0.35f, 0.8f, 0.18f);

    [Header("Per-type Header Accent Colors")]
    [Tooltip("Accent color for string property headers (overrides headerAccentColor when set)")]
    public Color headerAccentStringColor = new Color(0.1f, 0.6f, 0.7f, 0.18f);

    [Tooltip("Accent color for boolean property headers")]
    public Color headerAccentBoolColor = new Color(0.24f, 0.7f, 0.2f, 0.18f);

    [Tooltip("Accent color for integer property headers")]
    public Color headerAccentIntColor = new Color(0.9f, 0.5f, 0.1f, 0.18f);

    [Tooltip("Accent color for float property headers")]
    public Color headerAccentFloatColor = new Color(0.9f, 0.76f, 0.0f, 0.18f);

    [Tooltip("Accent color for unit property headers")]
    public Color headerAccentUnitColor = new Color(0.6f, 0.1f, 0.7f, 0.18f);

    [Tooltip("Accent color for object item headers")]
    public Color headerAccentObjectItemColor = new Color(0.2f, 0.5f, 0.6f, 0.18f);

    [Tooltip("Accent color for event property headers")]
    public Color headerAccentEventColor = new Color(0.7f, 0.2f, 0.4f, 0.18f);
}

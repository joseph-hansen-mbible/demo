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
}

using UnityEngine;

/// <summary>
/// Properties for map grid point features (treasure, doors, etc.).
/// Inherits from the base property system.
/// This replaces the old MapGridPointFeatureProperties class.
/// </summary>
[System.Serializable]
[CreateAssetMenu(
    fileName = "New MapGridFeatureProperties",
    menuName = "Turnroot/Maps/Feature Properties"
)]
public class MapGridFeatureProperties : MapGridPropertyBase
{
    [Header("Feature Identity")]
    [Tooltip("ID used to match this asset to a feature type (e.g. 'treasure').")]
    public string featureId = string.Empty;

    [Tooltip("Optional friendly name for the feature defaults asset.")]
    public string featureName = string.Empty;
}

// DEPRECATED: Keep for backwards compatibility during migration
// This allows existing serialized references to still work
[System.Serializable]
public class MapGridPointFeatureProperties : MapGridFeatureProperties { }

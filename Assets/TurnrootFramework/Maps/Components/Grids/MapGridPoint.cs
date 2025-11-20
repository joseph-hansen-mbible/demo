using System;
using System.Collections.Generic;
using Turnroot.Characters;
using Turnroot.Gameplay.Objects;
using Turnroot.Maps.Components.Grids;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Represents a single tile on the map grid. Each point contains a collection
/// of typed properties (strings, bools, ints, floats, units, object-items and
/// events). The property system is backed by typed containers derived from
/// `MapGridPropertyBase` so the editor and runtime can treat properties
/// generically while still providing typed accessors.
///
/// The defaults for every newly created grid point are defined here in
/// `InitializePresetGridPointProperties()` (starting unit and default events).
/// This keeps defaults local and avoids needing a ScriptableObject preset
/// for the common case of one global set of defaults. If you want a
/// configurable preset, consider the (deprecated) `MapGridPointProperties` SO
/// or create a new editor setting to point to a global preset.
/// </summary>
public class MapGridPoint : MonoBehaviour
{
    [SerializeField]
    private SpawnPoint _spawnPoint = new();
    public SpawnPoint SpawnPoint
    {
        get => _spawnPoint;
        set => _spawnPoint = value ?? new SpawnPoint();
    }

    void OnValidate()
    {
        SpawnPoint ??= new SpawnPoint();
        // Apply built-in defaults in case this point was created before those
        // defaults existed in the codebase.  This won't override existing
        // properties because the initialization only adds missing entries.
        InitializePresetGridPointProperties();
    }

    /* ---------------------------- Grid point data ---------------------------- */
    private static readonly (string name, int dRow, int dCol)[] Directions = new[]
    {
        ("N", -1, 0),
        ("NE", -1, 1),
        ("E", 0, 1),
        ("SE", 1, 1),
        ("S", 1, 0),
        ("SW", 1, -1),
        ("W", 0, -1),
        ("NW", -1, -1),
    };

    private static readonly (string name, int dRow, int dCol)[] CardinalDirections = new[]
    {
        ("N", -1, 0),
        ("E", 0, 1),
        ("S", 1, 0),
        ("W", 0, -1),
    };

    [SerializeField]
    private int _row;

    [SerializeField]
    private int _col;

    [SerializeField]
    [Tooltip("Gizmo sphere radius (world units)")]
    private float _gizmoRadius = 0.35f;

    [SerializeField]
    [Tooltip("Terrain type")]
    private string _terrainTypeId = string.Empty;

    [SerializeField]
    [Tooltip("Feature type")]
    private string _featureTypeId = string.Empty;

    [SerializeField]
    [Tooltip("Feature display name (optional).")]
    private string _featureName = string.Empty;

    /* ---------------------------- Grid Point Properties ---------------------------- */
    [Header("Grid Point Properties")]
    [SerializeField]
    private List<MapGridPropertyBase.StringProperty> _pointStringProperties = new();

    [SerializeField]
    private List<MapGridPropertyBase.EventProperty> _pointEventProperties = new();

    [SerializeField]
    private List<MapGridPropertyBase.UnitProperty> _pointUnitProperties = new();

    [SerializeField]
    private List<MapGridPropertyBase.ObjectItemProperty> _pointObjectItemProperties = new();

    [SerializeField]
    private List<MapGridPropertyBase.BoolProperty> _pointBoolProperties = new();

    [SerializeField]
    private List<MapGridPropertyBase.IntProperty> _pointIntProperties = new();

    [SerializeField]
    private List<MapGridPropertyBase.FloatProperty> _pointFloatProperties = new();

    // Keys for built-in preset properties.
    public const string KEY_STARTING_UNIT = "startingUnit";
    public const string KEY_FRIENDLY_ENTERS = "friendlyEnters";
    public const string KEY_ENEMY_ENTERS = "enemyEnters";

    /* ---------------------------- Feature Properties ---------------------------- */
    [Header("Feature Properties")]
    [SerializeField]
    private List<MapGridPropertyBase.StringProperty> _featureStringProperties = new();

    [SerializeField]
    private List<MapGridPropertyBase.EventProperty> _featureEventProperties = new();

    [SerializeField]
    private List<MapGridPropertyBase.UnitProperty> _featureUnitProperties = new();

    [SerializeField]
    private List<MapGridPropertyBase.ObjectItemProperty> _featureObjectItemProperties = new();

    [SerializeField]
    private List<MapGridPropertyBase.BoolProperty> _featureBoolProperties = new();

    [SerializeField]
    private List<MapGridPropertyBase.IntProperty> _featureIntProperties = new();

    [SerializeField]
    private List<MapGridPropertyBase.FloatProperty> _featureFloatProperties = new();

    public int Row => _row;
    public int Col => _col;
    public string TerrainTypeId => _terrainTypeId;
    public string FeatureTypeId => _featureTypeId;
    public string FeatureName
    {
        get => _featureName;
        set => _featureName = value ?? string.Empty;
    }
    public MapGridPointFeature.FeatureType FeatureType
    {
        get => MapGridPointFeature.TypeFromId(_featureTypeId);
        set => _featureTypeId = MapGridPointFeature.IdFromType(value) ?? string.Empty;
    }

    public TerrainType SelectedTerrainType
    {
        get
        {
            var asset = TerrainTypes.LoadDefault();
            if (asset == null)
                return null;
            var terrainType = asset.GetTypeById(_terrainTypeId);
            return terrainType ?? (asset.Types?.Length > 0 ? asset.Types[0] : null);
        }
    }

    public void Initialize(int row, int col)
    {
        _row = row;
        _col = col;
        InitializePresetGridPointProperties();
    }

    /// <summary>
    /// Initialize the preset properties that every grid point should have.
    /// </summary>
    private void InitializePresetGridPointProperties()
    {
        // Starting Unit
        if (_pointUnitProperties.Find(p => p.key == KEY_STARTING_UNIT) == null)
        {
            _pointUnitProperties.Add(
                new MapGridPropertyBase.UnitProperty { key = KEY_STARTING_UNIT, value = null }
            );
        }

        // Friendly Enters
        if (_pointEventProperties.Find(p => p.key == KEY_FRIENDLY_ENTERS) == null)
        {
            _pointEventProperties.Add(
                new MapGridPropertyBase.EventProperty
                {
                    key = KEY_FRIENDLY_ENTERS,
                    value = new UnityEvent(),
                }
            );
        }

        // Enemy Enters
        if (_pointEventProperties.Find(p => p.key == KEY_ENEMY_ENTERS) == null)
        {
            _pointEventProperties.Add(
                new MapGridPropertyBase.EventProperty
                {
                    key = KEY_ENEMY_ENTERS,
                    value = new UnityEvent(),
                }
            );
        }

        // NOTE: default point properties are defined within this class (see the
        // initialization above).  We intentionally do not auto-load a Scriptable
        // Object preset — there is a `MapGridPointProperties` ScriptableObject type
        // in the project, but we don't use it automatically; if you prefer to
        // manage presets from SOs, move the initialization here into the SO and
        // call it from InitializePresetGridPointProperties.
    }

    /* ---------------------------- Grid Point Property Accessors ---------------------------- */

    public CharacterInstance GetStartingUnit()
    {
        var prop = _pointUnitProperties.Find(p => p.key == KEY_STARTING_UNIT);
        return prop?.value;
    }

    public void SetStartingUnit(CharacterInstance unit)
    {
        var prop = _pointUnitProperties.Find(p => p.key == KEY_STARTING_UNIT);
        if (prop != null)
            prop.value = unit;
        else
            _pointUnitProperties.Add(
                new MapGridPropertyBase.UnitProperty { key = KEY_STARTING_UNIT, value = unit }
            );
    }

    public UnityEvent GetFriendlyEntersEvent()
    {
        var prop = _pointEventProperties.Find(p => p.key == KEY_FRIENDLY_ENTERS);
        return prop?.value;
    }

    public UnityEvent GetEnemyEntersEvent()
    {
        var prop = _pointEventProperties.Find(p => p.key == KEY_ENEMY_ENTERS);
        return prop?.value;
    }

    public void SetTerrainTypeId(string id) => _terrainTypeId = id ?? string.Empty;

    public void SetFeatureTypeId(string id) => _featureTypeId = id ?? string.Empty;

    public void ApplyFeature(string selId, string name, bool singleClickToggle)
    {
        if (string.IsNullOrEmpty(selId))
            return;

        if (selId == "eraser")
        {
            ClearFeature();
            return;
        }

        if (singleClickToggle && _featureTypeId == selId)
        {
            ClearFeature();
            return;
        }

        _featureTypeId = selId;
        _featureName = name ?? string.Empty;
        ApplyDefaultsForFeature(selId);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.EditorUtility.SetDirty(this.gameObject);
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    public void ClearFeature()
    {
        _featureTypeId = string.Empty;
        _featureName = string.Empty;
        _featureStringProperties.Clear();
        _featureUnitProperties.Clear();
        _featureObjectItemProperties.Clear();
        _featureEventProperties.Clear();
        _featureBoolProperties.Clear();
        _featureIntProperties.Clear();
        _featureFloatProperties.Clear();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.EditorUtility.SetDirty(this.gameObject);
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    public void ClearFeatureProperty(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;
        _featureStringProperties.RemoveAll(p => p.key == key);
        _featureUnitProperties.RemoveAll(p => p.key == key);
        _featureObjectItemProperties.RemoveAll(p => p.key == key);
        _featureBoolProperties.RemoveAll(p => p.key == key);
        _featureEventProperties.RemoveAll(p => p.key == key);
        _featureIntProperties.RemoveAll(p => p.key == key);
        _featureFloatProperties.RemoveAll(p => p.key == key);
    }

    // Generic property accessor pattern
    private void SetProperty<T>(List<T> list, string key, object value)
        where T : MapGridPropertyBase.IProperty, new()
    {
        if (string.IsNullOrEmpty(key))
            return;

        var existing = list.Find(p => p.key == key);
        if (existing != null)
            existing.SetValue(value);
        else
        {
            var newProp = new T { key = key };
            newProp.SetValue(value);
            list.Add(newProp);
        }
    }

    private TValue GetProperty<T, TValue>(List<T> list, string key, TValue defaultValue = default)
        where T : MapGridPropertyBase.IProperty
    {
        if (string.IsNullOrEmpty(key))
            return defaultValue;
        var prop = list.Find(p => p.key == key);
        return prop != null ? (TValue)prop.GetValue() : defaultValue;
    }

    private T? GetNullableProperty<T, TProp>(List<TProp> list, string key)
        where T : struct
        where TProp : MapGridPropertyBase.IProperty
    {
        if (string.IsNullOrEmpty(key))
            return null;
        var prop = list.Find(p => p.key == key);
        return prop != null ? (T?)prop.GetValue() : null;
    }

    /* ---------------------------- Feature Property Accessors ---------------------------- */

    public void SetStringFeatureProperty(string key, string value) =>
        SetProperty(_featureStringProperties, key, value ?? string.Empty);

    public string GetStringFeatureProperty(string key) =>
        GetProperty<MapGridPropertyBase.StringProperty, string>(_featureStringProperties, key);

    public List<MapGridPropertyBase.StringProperty> GetAllStringFeatureProperties() =>
        new(_featureStringProperties);

    // ----- Point-level string properties -----
    public void SetStringPointProperty(string key, string value) =>
        SetProperty(_pointStringProperties, key, value ?? string.Empty);

    public string GetStringPointProperty(string key) =>
        GetProperty<MapGridPropertyBase.StringProperty, string>(_pointStringProperties, key);

    public List<MapGridPropertyBase.StringProperty> GetAllStringPointProperties() =>
        new(_pointStringProperties);

    // Unit properties (NEW)
    public void SetUnitFeatureProperty(string key, CharacterInstance value) =>
        SetProperty(_featureUnitProperties, key, value);

    public CharacterInstance GetUnitFeatureProperty(string key) =>
        GetProperty<MapGridPropertyBase.UnitProperty, CharacterInstance>(
            _featureUnitProperties,
            key
        );

    public List<MapGridPropertyBase.UnitProperty> GetAllUnitFeatureProperties() =>
        new(_featureUnitProperties);

    // ----- Point-level unit properties -----
    public void SetUnitPointProperty(string key, CharacterInstance value) =>
        SetProperty(_pointUnitProperties, key, value);

    public CharacterInstance GetUnitPointProperty(string key) =>
        GetProperty<MapGridPropertyBase.UnitProperty, CharacterInstance>(_pointUnitProperties, key);

    public List<MapGridPropertyBase.UnitProperty> GetAllUnitPointProperties() =>
        new(_pointUnitProperties);

    // ObjectItem properties (NEW)
    public void SetObjectItemFeatureProperty(string key, ObjectItemInstance value) =>
        SetProperty(_featureObjectItemProperties, key, value);

    public ObjectItemInstance GetObjectItemFeatureProperty(string key) =>
        GetProperty<MapGridPropertyBase.ObjectItemProperty, ObjectItemInstance>(
            _featureObjectItemProperties,
            key
        );

    public List<MapGridPropertyBase.ObjectItemProperty> GetAllObjectItemFeatureProperties() =>
        new(_featureObjectItemProperties);

    // ----- Point-level object item properties -----
    public void SetObjectItemPointProperty(string key, ObjectItemInstance value) =>
        SetProperty(_pointObjectItemProperties, key, value);

    public ObjectItemInstance GetObjectItemPointProperty(string key) =>
        GetProperty<MapGridPropertyBase.ObjectItemProperty, ObjectItemInstance>(
            _pointObjectItemProperties,
            key
        );

    public List<MapGridPropertyBase.ObjectItemProperty> GetAllObjectItemPointProperties() =>
        new(_pointObjectItemProperties);

    // Bool properties
    public void SetBoolFeatureProperty(string key, bool value) =>
        SetProperty(_featureBoolProperties, key, value);

    public bool? GetBoolFeatureProperty(string key) =>
        GetNullableProperty<bool, MapGridPropertyBase.BoolProperty>(_featureBoolProperties, key);

    public List<MapGridPropertyBase.BoolProperty> GetAllBoolFeatureProperties() =>
        new(_featureBoolProperties);

    // ----- Point-level bool properties -----
    public void SetBoolPointProperty(string key, bool value) =>
        SetProperty(_pointBoolProperties, key, value);

    public bool? GetBoolPointProperty(string key) =>
        GetNullableProperty<bool, MapGridPropertyBase.BoolProperty>(_pointBoolProperties, key);

    public List<MapGridPropertyBase.BoolProperty> GetAllBoolPointProperties() =>
        new(_pointBoolProperties);

    // Int properties
    public void SetIntFeatureProperty(string key, int value) =>
        SetProperty(_featureIntProperties, key, value);

    public int? GetIntFeatureProperty(string key) =>
        GetNullableProperty<int, MapGridPropertyBase.IntProperty>(_featureIntProperties, key);

    public List<MapGridPropertyBase.IntProperty> GetAllIntFeatureProperties() =>
        new(_featureIntProperties);

    // ----- Point-level int properties -----
    public void SetIntPointProperty(string key, int value) =>
        SetProperty(_pointIntProperties, key, value);

    public int? GetIntPointProperty(string key) =>
        GetNullableProperty<int, MapGridPropertyBase.IntProperty>(_pointIntProperties, key);

    public List<MapGridPropertyBase.IntProperty> GetAllIntPointProperties() =>
        new(_pointIntProperties);

    // Float properties
    public void SetFloatFeatureProperty(string key, float value) =>
        SetProperty(_featureFloatProperties, key, value);

    public float? GetFloatFeatureProperty(string key) =>
        GetNullableProperty<float, MapGridPropertyBase.FloatProperty>(_featureFloatProperties, key);

    public List<MapGridPropertyBase.FloatProperty> GetAllFloatFeatureProperties() =>
        new(_featureFloatProperties);

    // ----- Point-level float properties -----
    public void SetFloatPointProperty(string key, float value) =>
        SetProperty(_pointFloatProperties, key, value);

    public float? GetFloatPointProperty(string key) =>
        GetNullableProperty<float, MapGridPropertyBase.FloatProperty>(_pointFloatProperties, key);

    public List<MapGridPropertyBase.FloatProperty> GetAllFloatPointProperties() =>
        new(_pointFloatProperties);

    // Event properties
    public void SetEventFeatureProperty(string key, UnityEvent value) =>
        SetProperty(_featureEventProperties, key, value);

    public UnityEvent GetEventFeatureProperty(string key)
    {
        return GetProperty<MapGridPropertyBase.EventProperty, UnityEvent>(
            _featureEventProperties,
            key
        );
    }

    public List<MapGridPropertyBase.EventProperty> GetAllEventFeatureProperties() =>
        new(_featureEventProperties);

    // ----- Point-level event properties -----
    public void SetEventPointProperty(string key, UnityEvent value) =>
        SetProperty(_pointEventProperties, key, value);

    public UnityEvent GetEventPointProperty(string key) =>
        GetProperty<MapGridPropertyBase.EventProperty, UnityEvent>(_pointEventProperties, key);

    public List<MapGridPropertyBase.EventProperty> GetAllEventPointProperties() =>
        new(_pointEventProperties);

    public void ApplyDefaultsForFeature(string featureId)
    {
        if (string.IsNullOrEmpty(featureId))
            return;

        var allDefaults = Resources.LoadAll<MapGridFeatureProperties>("GameSettings");
        if (allDefaults == null || allDefaults.Length == 0)
            return;

        var defaultProps = FindFeatureProperties(allDefaults, featureId);
        if (defaultProps == null)
            return;

        ApplyDefaultStringProperties(defaultProps.stringProperties);
        ApplyDefaultUnitProperties(defaultProps.unitProperties);
        ApplyDefaultObjectItemProperties(defaultProps.objectItemProperties);
        ApplyDefaultBoolProperties(defaultProps.boolProperties);
        ApplyDefaultEventProperties(defaultProps.eventProperties);
        ApplyDefaultIntProperties(defaultProps.intProperties);
        ApplyDefaultFloatProperties(defaultProps.floatProperties);
    }

    // Deprecated/removed: We no longer look up per-point ScriptableObject presets.
    // If we want a single global preset managed by an SO, use MapGridPointProperties
    // and call the ApplyDefault*PointProperties helpers directly here.

    private void ApplyDefaultStringPointProperties(
        List<MapGridPropertyBase.StringProperty> defaults
    )
    {
        ApplyDefaultRefPointProperties<MapGridPropertyBase.StringProperty, string>(
            defaults,
            GetStringPointProperty,
            SetStringPointProperty
        );
    }

    private void ApplyDefaultUnitPointProperties(List<MapGridPropertyBase.UnitProperty> defaults)
    {
        ApplyDefaultRefPointProperties<MapGridPropertyBase.UnitProperty, CharacterInstance>(
            defaults,
            GetUnitPointProperty,
            SetUnitPointProperty
        );
    }

    private void ApplyDefaultObjectItemPointProperties(
        List<MapGridPropertyBase.ObjectItemProperty> defaults
    )
    {
        ApplyDefaultRefPointProperties<MapGridPropertyBase.ObjectItemProperty, ObjectItemInstance>(
            defaults,
            GetObjectItemPointProperty,
            SetObjectItemPointProperty
        );
    }

    private void ApplyDefaultBoolPointProperties(List<MapGridPropertyBase.BoolProperty> defaults)
    {
        ApplyDefaultNullableValPointProperties<MapGridPropertyBase.BoolProperty, bool>(
            defaults,
            GetBoolPointProperty,
            SetBoolPointProperty
        );
    }

    private void ApplyDefaultEventPointProperties(List<MapGridPropertyBase.EventProperty> defaults)
    {
        ApplyDefaultRefPointProperties<MapGridPropertyBase.EventProperty, UnityEvent>(
            defaults,
            GetEventPointProperty,
            SetEventPointProperty
        );
    }

    private void ApplyDefaultIntPointProperties(List<MapGridPropertyBase.IntProperty> defaults)
    {
        ApplyDefaultNullableValPointProperties<MapGridPropertyBase.IntProperty, int>(
            defaults,
            GetIntPointProperty,
            SetIntPointProperty
        );
    }

    private void ApplyDefaultFloatPointProperties(List<MapGridPropertyBase.FloatProperty> defaults)
    {
        ApplyDefaultNullableValPointProperties<MapGridPropertyBase.FloatProperty, float>(
            defaults,
            GetFloatPointProperty,
            SetFloatPointProperty
        );
    }

    // Generic helpers to DRY default property application. Two variants:
    // - reference types (string, CharacterInstance, ObjectItemInstance, UnityEvent)
    // - nullable value types (bool?, int?, float?)
    private void ApplyDefaultRefPointProperties<TProp, TValue>(
        List<TProp> defaults,
        Func<string, TValue> getter,
        Action<string, TValue> setter
    )
        where TProp : MapGridPropertyBase.IProperty
        where TValue : class
    {
        if (defaults == null)
            return;
        foreach (var prop in defaults)
        {
            if (string.IsNullOrEmpty(prop.key) || getter(prop.key) != null)
                continue;
            var val = prop.GetValue() as TValue;
            setter(prop.key, val);
        }
    }

    private void ApplyDefaultNullableValPointProperties<TProp, TValue>(
        List<TProp> defaults,
        Func<string, TValue?> getter,
        Action<string, TValue> setter
    )
        where TProp : MapGridPropertyBase.IProperty
        where TValue : struct
    {
        if (defaults == null)
            return;
        foreach (var prop in defaults)
        {
            if (string.IsNullOrEmpty(prop.key))
                continue;

            var current = getter(prop.key);
            if (current.HasValue)
                continue;
            var val = (TValue)prop.GetValue();
            setter(prop.key, val);
        }
    }

    private MapGridFeatureProperties FindFeatureProperties(
        MapGridFeatureProperties[] allDefaults,
        string featureId
    )
    {
        foreach (var props in allDefaults)
        {
            if (props == null)
                continue;
            if (
                props.featureId == featureId
                || string.Equals(props.name, featureId, StringComparison.OrdinalIgnoreCase)
            )
                return props;
        }
        return null;
    }

    private void ApplyDefaultStringProperties(List<MapGridPropertyBase.StringProperty> defaults)
    {
        if (defaults == null)
            return;
        foreach (var prop in defaults)
        {
            if (string.IsNullOrEmpty(prop.key) || GetStringFeatureProperty(prop.key) != null)
                continue;
            SetStringFeatureProperty(prop.key, prop.value);
        }
    }

    private void ApplyDefaultUnitProperties(List<MapGridPropertyBase.UnitProperty> defaults)
    {
        if (defaults == null)
            return;
        foreach (var prop in defaults)
        {
            if (string.IsNullOrEmpty(prop.key) || GetUnitFeatureProperty(prop.key) != null)
                continue;
            SetUnitFeatureProperty(prop.key, prop.value);
        }
    }

    private void ApplyDefaultObjectItemProperties(
        List<MapGridPropertyBase.ObjectItemProperty> defaults
    )
    {
        if (defaults == null)
            return;
        foreach (var prop in defaults)
        {
            if (string.IsNullOrEmpty(prop.key) || GetObjectItemFeatureProperty(prop.key) != null)
                continue;
            SetObjectItemFeatureProperty(prop.key, prop.value);
        }
    }

    private void ApplyDefaultBoolProperties(List<MapGridPropertyBase.BoolProperty> defaults)
    {
        if (defaults == null)
            return;
        foreach (var prop in defaults)
        {
            if (string.IsNullOrEmpty(prop.key) || GetBoolFeatureProperty(prop.key).HasValue)
                continue;
            SetBoolFeatureProperty(prop.key, prop.value);
        }
    }

    private void ApplyDefaultEventProperties(List<MapGridPropertyBase.EventProperty> defaults)
    {
        if (defaults == null)
            return;
        foreach (var prop in defaults)
        {
            if (string.IsNullOrEmpty(prop.key) || GetEventFeatureProperty(prop.key) != null)
                continue;
            SetEventFeatureProperty(prop.key, prop.value);
        }
    }

    private void ApplyDefaultIntProperties(List<MapGridPropertyBase.IntProperty> defaults)
    {
        if (defaults == null)
            return;
        foreach (var prop in defaults)
        {
            if (string.IsNullOrEmpty(prop.key) || GetIntFeatureProperty(prop.key).HasValue)
                continue;
            SetIntFeatureProperty(prop.key, prop.value);
        }
    }

    private void ApplyDefaultFloatProperties(List<MapGridPropertyBase.FloatProperty> defaults)
    {
        if (defaults == null)
            return;
        foreach (var prop in defaults)
        {
            if (string.IsNullOrEmpty(prop.key) || GetFloatFeatureProperty(prop.key).HasValue)
                continue;
            SetFloatFeatureProperty(prop.key, prop.value);
        }
    }

    public float GetTerrainTypeCost(
        bool isWalking = true,
        bool isFlying = false,
        bool isRiding = false,
        bool isMagic = false,
        bool isArmored = false
    )
    {
        var terrainType = SelectedTerrainType;
        if (terrainType == null)
            return 1f;

        if (isWalking)
            return terrainType.CostWalk;
        if (isFlying)
            return terrainType.CostFly;
        if (isRiding)
            return terrainType.CostRide;
        if (isMagic)
            return terrainType.CostMagic;
        if (isArmored)
            return terrainType.CostArmor;

        return 1f;
    }

    public Vector2 Coordinates() => new(_row, _col);

    public Dictionary<string, MapGridPoint> GetNeighbors(bool cardinal = false)
    {
        var neighbors = new Dictionary<string, MapGridPoint>();
        var grid = GetComponentInParent<MapGrid>();
        if (grid == null)
            return neighbors;

        var dirs = cardinal ? CardinalDirections : Directions;
        foreach (var (name, dRow, dCol) in dirs)
        {
            var neighbor = grid.GetGridPoint(_row + dRow, _col + dCol);
            if (neighbor != null)
                neighbors[name] = neighbor;
        }

        return neighbors;
    }
}

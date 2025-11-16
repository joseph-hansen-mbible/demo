using System.Collections.Generic;
using UnityEngine;

public class MapGridPoint : MonoBehaviour
{
    // Initialize row/col after AddComponent (MonoBehaviours can't use constructors)
    public void Initialize(int row, int col)
    {
        _row = row;
        _col = col;
    }

    // Allow editor tools to change terrain id
    public void SetTerrainTypeId(string id)
    {
        _terrainTypeId = id ?? string.Empty;
    }

    // Expose the stored terrain id for editor tooling
    public string TerrainTypeId => _terrainTypeId;

    private readonly (string name, int dRow, int dCol)[] directions = new (
        string name,
        int dRow,
        int dCol
    )[]
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

    private readonly (string name, int dRow, int dCol)[] cardinalDirections = new (
        string name,
        int dRow,
        int dCol
    )[]
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

    public int Row => _row;
    public int Col => _col;

    public MapGridPoint() { }

    public MapGridPoint(int row, int col)
    {
        _row = row;
        _col = col;
    }

    [SerializeField]
    [Tooltip("Gizmo sphere radius (world units)")]
    private float _gizmoRadius = 0.35f;

    [SerializeField]
    [Tooltip(
        "ID of the terrain type from the TerrainTypes asset. If empty, the editor/runtime will try to find the default asset and allow selection."
    )]
    private string _terrainTypeId = string.Empty;

    // Optional editor feature layer (second layer) - stores a feature id and an optional name.
    [SerializeField]
    [Tooltip("ID of an editor feature (chest, door, warp, etc). Empty = none.")]
    private string _featureTypeId = string.Empty;

    [SerializeField]
    [Tooltip("Optional name or label for the feature. Editable from the Map Grid Editor.")]
    private string _featureName = string.Empty;

    [System.Serializable]
    public class FeatureProperty
    {
        public string key = string.Empty;
        public string value = string.Empty;
    }

    [SerializeField]
    [Tooltip("Optional key/value properties attached to a feature (persisted).")]
    private List<FeatureProperty> _featureProperties = new List<FeatureProperty>();

    // Expose feature fields for editor tooling
    public string FeatureTypeId => _featureTypeId;
    public string FeatureName
    {
        get => _featureName;
        set => _featureName = value ?? string.Empty;
    }

    // Editor helpers
    public void SetFeatureTypeId(string id)
    {
        _featureTypeId = id ?? string.Empty;
    }

    public void ApplyFeature(string selId, string name, bool singleClickToggle)
    {
        if (string.IsNullOrEmpty(selId))
            return;

        if (selId == "eraser")
        {
            ClearFeature();
            return;
        }

        if (singleClickToggle && !string.IsNullOrEmpty(_featureTypeId) && _featureTypeId == selId)
        {
            ClearFeature();
            return;
        }

        _featureTypeId = selId;
        _featureName = name ?? string.Empty;
    }

    public void ClearFeature()
    {
        _featureTypeId = string.Empty;
        _featureName = string.Empty;
        _featureProperties.Clear();
    }

    // Feature property helpers (key/value pairs persisted on the MapGridPoint)
    public void SetFeatureProperty(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
            return;
        var existing = _featureProperties.Find(p => p.key == key);
        if (existing != null)
        {
            existing.value = value ?? string.Empty;
        }
        else
        {
            _featureProperties.Add(
                new FeatureProperty { key = key, value = value ?? string.Empty }
            );
        }
    }

    public string GetFeatureProperty(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;
        var p = _featureProperties.Find(x => x.key == key);
        return p != null ? p.value : null;
    }

    public void ClearFeatureProperty(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;
        _featureProperties.RemoveAll(p => p.key == key);
    }

    public List<FeatureProperty> GetAllFeatureProperties()
    {
        // return a shallow copy to avoid callers mutating the serialized list directly
        return new List<FeatureProperty>(_featureProperties);
    }

    // Feature letter mapping moved to MapGridPointFeature.GetFeatureLetter(string)

    public TerrainType SelectedTerrainType
    {
        get
        {
            var asset = TerrainTypes.LoadDefault();
            if (asset == null)
                return null;
            var t = asset.GetTypeById(_terrainTypeId);
            if (t != null)
                return t;
            // fallback to first type if available
            if (asset.Types != null && asset.Types.Length > 0)
                return asset.Types[0];
            return null;
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
        var tt = SelectedTerrainType;
        if (tt == null)
            return 1; // default cost
        if (isWalking)
            return tt.CostWalk;
        else if (isFlying)
            return tt.CostFly;
        else if (isRiding)
            return tt.CostRide;
        else if (isMagic)
            return tt.CostMagic;
        else if (isArmored)
            return tt.CostArmor;
        return 1; // default cost
    }

    public Vector2 Coordinates()
    {
        return new Vector2(_row, _col);
    }

    public Dictionary<string, MapGridPoint> GetNeighbors(bool cardinal = false)
    {
        var neighbors = new Dictionary<string, MapGridPoint>();
        var grid = GetComponentInParent<MapGrid>();

        var dirs = cardinal ? cardinalDirections : directions;
        foreach (var (name, dRow, dCol) in dirs)
        {
            int newRow = _row + dRow;
            int newCol = _col + dCol;
            var neighbor = grid.GetGridPoint(newRow, newCol);
            if (neighbor != null)
            {
                neighbors[name] = neighbor;
            }
        }

        return neighbors;
    }
}

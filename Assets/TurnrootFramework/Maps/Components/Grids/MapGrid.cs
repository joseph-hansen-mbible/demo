using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class MapGrid : MonoBehaviour
{
    [SerializeField]
    private float _gridScale = 1f;

    [SerializeField]
    private Vector3 _gridOffset = Vector3.zero;

    [SerializeField]
    private int _gridWidth = 10;

    [SerializeField]
    private int _gridHeight = 10;

    // Optional user-provided map name shown in the editor toolbar
    [SerializeField]
    private string _mapName = string.Empty;

    public string MapName
    {
        get => _mapName;
        set => _mapName = value;
    }

    [SerializeField]
    private Dictionary<Vector2Int, GameObject> _gridPoints = new();

    [System.Serializable]
    private class FeatureRecord
    {
        public int row;
        public int col;
        public string typeId;
        public string name;

        [System.Serializable]
        public class PropertyRecord
        {
            public string key;
            public string value;
        }

        // Optional key/value properties attached to the feature
        public List<PropertyRecord> properties = new List<PropertyRecord>();
    }

    [SerializeField]
    [Tooltip(
        "Serialized feature layer records (second layer) for editor features such as chests, doors, etc."
    )]
    private List<FeatureRecord> _featureRecords = new();

    [SerializeField]
    private SerializableDictionary<GameObject, int> _rowLookup = new();

    [SerializeField]
    private SerializableDictionary<GameObject, int> _colLookup = new();

    [Button("Create Grid Points")]
    public void CreateChildrenPoints()
    {
        if (_gridPoints.Count > 0)
        {
            ClearGrid();
        }
        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                var point = new GameObject($"Point_R{x}_C{y}");
                _rowLookup[point] = x;
                _colLookup[point] = y;
                // add MapGridPoint to the child GameObject (not to the parent)
                var gridPoint = point.AddComponent<MapGridPoint>();
                gridPoint.Initialize(x, y);
                point.transform.parent = transform;
                point.transform.localPosition =
                    new Vector3(x * _gridScale, 0, y * _gridScale) + _gridOffset;
                _gridPoints[new Vector2Int(x, y)] = point;
            }
        }

        // After creating points, restore any serialized feature data
        LoadFeatureLayer();
    }

    public void ClearGrid()
    {
        foreach (var point in _gridPoints.Values)
        {
            if (point != null)
            {
                Destroy(point);
            }
        }
        _gridPoints.Clear();
    }

    // Rebuild the internal lookup dictionary from existing child GameObjects.
    // Useful because serialized dictionaries may not persist between editor sessions.
    public void RebuildGridDictionary()
    {
        var newDict = new Dictionary<Vector2Int, GameObject>();
        foreach (Transform child in transform)
        {
            if (child == null)
                continue;
            var mgp = child.GetComponent<MapGridPoint>();
            if (mgp != null)
            {
                var key = new Vector2Int(mgp.Row, mgp.Col);
                newDict[key] = child.gameObject;
            }
        }
        _gridPoints = newDict;

        // Restore feature layer onto the rebuilt points
        LoadFeatureLayer();
    }

    // Persist current MapGridPoint features into the serialized record list
    public void SaveFeatureLayer()
    {
        _featureRecords.Clear();
        foreach (var kv in _gridPoints)
        {
            var key = kv.Key;
            var go = kv.Value;
            if (go == null)
                continue;
            var mgp = go.GetComponent<MapGridPoint>();
            if (mgp == null)
                continue;
            if (string.IsNullOrEmpty(mgp.FeatureTypeId))
                continue;
            var rec = new FeatureRecord
            {
                row = key.x,
                col = key.y,
                typeId = mgp.FeatureTypeId,
                name = mgp.FeatureName,
            };

            // Copy feature properties
            var props = mgp.GetAllFeatureProperties();
            if (props != null && props.Count > 0)
            {
                foreach (var p in props)
                {
                    rec.properties.Add(
                        new FeatureRecord.PropertyRecord { key = p.key, value = p.value }
                    );
                }
            }
            _featureRecords.Add(rec);
        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    // Apply serialized feature records back onto MapGridPoint children
    public void LoadFeatureLayer()
    {
        if (_featureRecords == null || _featureRecords.Count == 0)
            return;
        foreach (var rec in _featureRecords)
        {
            var mgp = GetGridPoint(rec.row, rec.col);
            if (mgp != null)
            {
                mgp.SetFeatureTypeId(rec.typeId);
                mgp.FeatureName = rec.name ?? string.Empty;
                // Restore any serialized properties
                if (rec.properties != null && rec.properties.Count > 0)
                {
                    foreach (var pr in rec.properties)
                    {
                        if (pr != null && !string.IsNullOrEmpty(pr.key))
                            mgp.SetFeatureProperty(pr.key, pr.value ?? string.Empty);
                    }
                }
            }
        }
    }

    // Ensure grid points exist and the internal index is populated. If there are no
    // child points, create them. If there are children but the index is empty, rebuild it.
    public void EnsureGridPoints()
    {
        if (_gridPoints == null || _gridPoints.Count == 0)
        {
            if (transform.childCount == 0)
            {
                CreateChildrenPoints();
            }
            else
            {
                RebuildGridDictionary();
            }
        }
    }

    public MapGridPoint GetGridPoint(int row, int col)
    {
        if (_gridPoints.TryGetValue(new Vector2Int(row, col), out var point))
        {
            return point.GetComponent<MapGridPoint>();
        }
        return null;
    }

    // Public getters used by editor tools
    public int GridWidth => _gridWidth;
    public int GridHeight => _gridHeight;
    public float GridScale => _gridScale;
    public Vector3 GridOffset => _gridOffset;

    /* -------------------------------- A* Tester ------------------------------- */

    [SerializeField]
    private MapGridPoint _aStarTesterStart;

    [SerializeField]
    private MapGridPoint _aStarTesterGoal;

    [Button("Test A* Pathfinding")]
    public void TestAStar()
    {
        var aStar = new AStarModified();
        var path = aStar.AStarSearch(this, _aStarTesterStart, _aStarTesterGoal);
        // Intentionally silent in editor; use the Test A* button's visual output instead.
    }

    [Button("Reset Point Colors")]
    public void ResetPointColorsButton()
    {
        ResetAllPointColors();
    }

    public void ResetAllPointColors()
    {
        // No per-point gizmos to reset; repaint scene so editor views refresh.
#if UNITY_EDITOR
        UnityEditor.SceneView.RepaintAll();
#endif
    }
}

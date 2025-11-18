using System.Collections.Generic;
using System.Linq;
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

    [SerializeField]
    private string _mapName = string.Empty;
    public string MapName
    {
        get => _mapName;
        set => _mapName = value;
    }

    [SerializeField]
    private Dictionary<Vector2Int, GameObject> _gridPoints = new();

    [SerializeField]
    [Tooltip(
        "Serialized feature layer records (second layer) for editor features such as chests, doors, etc."
    )]
    private List<FeatureRecord> _features = new();

    [SerializeField]
    private GameObject _single3dHeightMesh;

    [SerializeField]
    private Vector3[] _single3dHeightMeshRaycastPoints;

    [SerializeField]
    [Tooltip(
        "Layer mask used when raycasting to the 3D map. Use this to limit raycasts to the map's layer(s)."
    )]
    private LayerMask _raycastLayerMask = ~0;

    public int GridWidth => _gridWidth;
    public int GridHeight => _gridHeight;
    public float GridScale => _gridScale;
    public Vector3 GridOffset => _gridOffset;

    [Button("Create Grid Points")]
    public void CreateChildrenPoints()
    {
        if (_gridPoints.Count > 0)
            ClearGrid();

        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                var point = new GameObject($"Point_R{x}_C{y}");
                var gridPoint = point.AddComponent<MapGridPoint>();
                gridPoint.Initialize(x, y);

                SetDefaultTerrainType(gridPoint);

                point.transform.parent = transform;
                point.transform.localPosition =
                    new Vector3(x * _gridScale, 0, y * _gridScale) + _gridOffset;
                _gridPoints[new Vector2Int(x, y)] = point;
            }
        }

        LoadFeatureLayer();
    }

    private void SetDefaultTerrainType(MapGridPoint gridPoint)
    {
        var terrainAsset = TerrainTypes.LoadDefault();
        if (terrainAsset?.Types == null)
            return;

        var voidType = terrainAsset.Types.FirstOrDefault(t =>
            t != null && t.Name.Equals("Void", System.StringComparison.OrdinalIgnoreCase)
        );

        if (voidType != null)
        {
            gridPoint.SetTerrainTypeId(voidType.Id);
        }
        else if (terrainAsset.Types.Length > 0 && terrainAsset.Types[0] != null)
        {
            gridPoint.SetTerrainTypeId(terrainAsset.Types[0].Id);
        }
    }

    [Button("Add Row")]
    public void AddRow()
    {
        SaveFeatureLayer();
        _gridHeight++;

        int newRow = _gridHeight - 1;
        for (int col = 0; col < _gridWidth; col++)
        {
            if (GetGridPoint(col, newRow) != null)
                continue;

            var point = new GameObject($"Point_R{col}_C{newRow}");
            var gridPoint = point.AddComponent<MapGridPoint>();
            gridPoint.Initialize(col, newRow);
            SetDefaultTerrainType(gridPoint);

            point.transform.parent = transform;
            point.transform.localPosition =
                new Vector3(col * _gridScale, 0, newRow * _gridScale) + _gridOffset;
            _gridPoints[new Vector2Int(col, newRow)] = point;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(point);
            UnityEditor.EditorUtility.SetDirty(gridPoint);
#endif
        }

        LoadFeatureLayer();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    [Button("Add Column")]
    public void AddColumn()
    {
        SaveFeatureLayer();
        _gridWidth++;

        int newCol = _gridWidth - 1;
        for (int row = 0; row < _gridHeight; row++)
        {
            if (GetGridPoint(newCol, row) != null)
                continue;

            var point = new GameObject($"Point_R{newCol}_C{row}");
            var gridPoint = point.AddComponent<MapGridPoint>();
            gridPoint.Initialize(newCol, row);
            SetDefaultTerrainType(gridPoint);

            point.transform.parent = transform;
            point.transform.localPosition =
                new Vector3(newCol * _gridScale, 0, row * _gridScale) + _gridOffset;
            _gridPoints[new Vector2Int(newCol, row)] = point;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(point);
            UnityEditor.EditorUtility.SetDirty(gridPoint);
#endif
        }

        LoadFeatureLayer();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    [Button("Remove Row")]
    public void RemoveRow()
    {
        if (_gridHeight <= 1)
            return;

        SaveFeatureLayer();

        int removeRow = _gridHeight - 1;
        for (int col = 0; col < _gridWidth; col++)
        {
            var mgp = GetGridPoint(col, removeRow);
            if (mgp == null)
                continue;

            _gridPoints.Remove(new Vector2Int(col, removeRow));
            DestroyImmediate(mgp.gameObject);
        }

        _gridHeight--;
        LoadFeatureLayer();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    [Button("Remove Column")]
    public void RemoveColumn()
    {
        if (_gridWidth <= 1)
            return;

        SaveFeatureLayer();

        int removeCol = _gridWidth - 1;
        for (int row = 0; row < _gridHeight; row++)
        {
            var mgp = GetGridPoint(removeCol, row);
            if (mgp == null)
                continue;

            _gridPoints.Remove(new Vector2Int(removeCol, row));
            DestroyImmediate(mgp.gameObject);
        }

        _gridWidth--;
        LoadFeatureLayer();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    public void ClearGrid()
    {
        foreach (var point in _gridPoints.Values.Where(p => p != null))
        {
            DestroyImmediate(point);
        }
        _gridPoints.Clear();
    }

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
                newDict[new Vector2Int(mgp.Row, mgp.Col)] = child.gameObject;
            }
        }
        _gridPoints = newDict;

        LoadFeatureLayer();
    }

    [Button("Connect to 3D Map Height")]
    public void ConnectTo3DMapObject()
    {
        if (_single3dHeightMesh == null)
            return;

        EnsureGridPoints();

        var colliders = _single3dHeightMesh.GetComponentsInChildren<Collider>(true);
        if (colliders == null || colliders.Length == 0)
            return;

        var connector = new MapGridHeightConnector();
        var points = connector.RaycastPointsDownTo3DMap(
            _single3dHeightMesh,
            _gridPoints,
            _raycastLayerMask,
            true
        );

        if (points == null || points.Length == 0)
            return;

        _single3dHeightMeshRaycastPoints = points;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    public void SaveFeatureLayer()
    {
        _features.Clear();
        foreach (var kv in _gridPoints)
        {
            var mgp = kv.Value?.GetComponent<MapGridPoint>();
            if (mgp == null || string.IsNullOrEmpty(mgp.FeatureTypeId))
                continue;

            var rec = new FeatureRecord
            {
                row = kv.Key.x,
                col = kv.Key.y,
                typeId = mgp.FeatureTypeId,
                name = mgp.FeatureName,
                stringProperties = mgp.GetAllStringFeatureProperties()
                    ?.Select(p => new PropertyRecord<string> { key = p.key, value = p.value })
                    .ToList(),
                boolProperties = mgp.GetAllBoolFeatureProperties()
                    ?.Select(p => new PropertyRecord<bool> { key = p.key, value = p.value })
                    .ToList(),
                intProperties = mgp.GetAllIntFeatureProperties()
                    ?.Select(p => new PropertyRecord<int> { key = p.key, value = p.value })
                    .ToList(),
                floatProperties = mgp.GetAllFloatFeatureProperties()
                    ?.Select(p => new PropertyRecord<float> { key = p.key, value = p.value })
                    .ToList(),
            };

            _features.Add(rec);
        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public void LoadFeatureLayer()
    {
        if (_features == null || _features.Count == 0)
            return;

        foreach (var rec in _features)
        {
            var mgp = GetGridPoint(rec.row, rec.col);
            if (mgp == null)
                continue;

            mgp.SetFeatureTypeId(rec.typeId);
            mgp.FeatureName = rec.name ?? string.Empty;
            mgp.ApplyDefaultsForFeature(rec.typeId);

            if (rec.stringProperties != null)
            {
                foreach (var pr in rec.stringProperties.Where(pr => !string.IsNullOrEmpty(pr.key)))
                    mgp.SetStringFeatureProperty(pr.key, pr.value ?? string.Empty);
            }
            if (rec.boolProperties != null)
            {
                foreach (var pr in rec.boolProperties.Where(pr => !string.IsNullOrEmpty(pr.key)))
                    mgp.SetBoolFeatureProperty(pr.key, pr.value);
            }
            if (rec.intProperties != null)
            {
                foreach (var pr in rec.intProperties.Where(pr => !string.IsNullOrEmpty(pr.key)))
                    mgp.SetIntFeatureProperty(pr.key, pr.value);
            }
            if (rec.floatProperties != null)
            {
                foreach (var pr in rec.floatProperties.Where(pr => !string.IsNullOrEmpty(pr.key)))
                    mgp.SetFloatFeatureProperty(pr.key, pr.value);
            }
        }
    }

    public void EnsureGridPoints()
    {
        int expectedCount = _gridWidth * _gridHeight;
        int actualCount = transform
            .Cast<Transform>()
            .Count(child => child != null && child.GetComponent<MapGridPoint>() != null);

        if (
            actualCount != expectedCount
            || _gridPoints == null
            || _gridPoints.Count != expectedCount
        )
        {
            if (actualCount > 0)
                RebuildGridDictionary();
            else
                CreateChildrenPoints();
        }
        else if (_gridPoints.Count == 0 && transform.childCount > 0)
        {
            RebuildGridDictionary();
        }
        else if (_gridPoints.Count == 0 && transform.childCount == 0)
        {
            CreateChildrenPoints();
        }

        RepositionGridPoints();
    }

    private void RepositionGridPoints()
    {
        if (_gridPoints == null || _gridPoints.Count == 0)
            return;

        foreach (var kv in _gridPoints)
        {
            if (kv.Value == null)
                continue;
            kv.Value.transform.localPosition =
                new Vector3(kv.Key.x * _gridScale, 0, kv.Key.y * _gridScale) + _gridOffset;
        }
    }

    public MapGridPoint GetGridPoint(int row, int col)
    {
        return _gridPoints.TryGetValue(new Vector2Int(row, col), out var point)
            ? point.GetComponent<MapGridPoint>()
            : null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (_gridPoints == null || _gridPoints.Count == 0)
        {
            if (transform.childCount > 0)
                RebuildGridDictionary();
        }

        RepositionGridPoints();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Vector3 getPos(int x, int y) =>
            transform.position + new Vector3(x * _gridScale, 0, y * _gridScale) + _gridOffset;

        Vector3 topLeft = getPos(0, 0);
        Vector3 topRight = getPos(_gridWidth - 1, 0);
        Vector3 bottomLeft = getPos(0, _gridHeight - 1);
        Vector3 bottomRight = getPos(_gridWidth - 1, _gridHeight - 1);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);

        var corners = new[] { topLeft, topRight, bottomLeft, bottomRight };
        foreach (var corner in corners)
        {
            Gizmos.DrawSphere(corner, 1f);
        }

        if (_single3dHeightMeshRaycastPoints != null && _single3dHeightMeshRaycastPoints.Length > 0)
        {
            Gizmos.color = Color.yellow;
            float s = Mathf.Max(0.05f, _gridScale * 0.2f);
            foreach (var p in _single3dHeightMeshRaycastPoints)
            {
                Gizmos.DrawSphere(p, s);
            }
        }
    }
#endif
}

[System.Serializable]
public struct PropertyRecord<T>
{
    public string key;
    public T value;
}

[System.Serializable]
public class FeatureRecord
{
    public int row;
    public int col;
    public string typeId;
    public string name;

    // Typed properties attached to the feature
    public List<PropertyRecord<string>> stringProperties = new();
    public List<PropertyRecord<bool>> boolProperties = new();
    public List<PropertyRecord<int>> intProperties = new();
    public List<PropertyRecord<float>> floatProperties = new();
}

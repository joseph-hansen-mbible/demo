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

    [SerializeField]
    private Dictionary<Vector2Int, GameObject> _gridPoints = new();

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

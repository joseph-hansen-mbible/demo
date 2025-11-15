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

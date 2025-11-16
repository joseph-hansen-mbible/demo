#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

public class MapGridEditorWindow : EditorWindow
{
    private MapGrid _grid;
    private TerrainTypes _terrainAsset;
    private int _selectedTerrainIndex = 0;
    private Vector2 _scroll = Vector2.zero;
    private float _zoom = 1f;
    private readonly int _baseCellSize = 24;
    private bool _isDragging = false;
    private Vector2Int _dragStart;
    private Vector2Int _dragEnd;
    private Vector2Int _hoveredCell = new(-1, -1);

    // Snapshot used to stabilize the right-panel UI between Layout and Repaint passes
    private Vector2Int _detailSnapshotHovered = new(-1, -1);
    private List<KeyValuePair<string, string>> _detailSnapshotProps = null;

    // Space-pan state
    private bool _spacePan = false;
    private bool _isPanning = false;

    private enum Mode
    {
        Paint = 0,
        TestMovement = 1,
    }

    private Mode _mode = Mode.Paint;

    // A* options
    private bool _asWalk = true;
    private bool _asFly = false;
    private bool _asRide = false;
    private bool _asMagic = false;
    private bool _asArmor = false;
    private float _sameDirectionMultiplier = 0.95f;

    // Movement tester
    private int _testMovementValue = 5;
    private MapGridPoint _testMovementStart = null;
    private Dictionary<MapGridPoint, float> _testMovementResults = null;

    // Second-layer tools (editor features)
    private readonly string[] _secondToolIds = new string[]
    {
        "treasure",
        "door",
        "warp",
        "healing",
        "ranged",
        "mechanism",
        "control",
        "breakable",
        "shelter",
        "underground",
        "eraser",
    };
    private readonly string[] _secondToolNames = new string[]
    {
        "Treasure",
        "Door",
        "Warp",
        "Healing",
        "Ranged",
        "Mechanism",
        "Control",
        "BreakableWall",
        "Shelter",
        "UndergroundItem",
        "Eraser",
    };
    private int _selectedSecondTool = -1; // -1 = none
    private string _selectedSecondToolName = string.Empty; // name to apply to painted features

    // Icon cache for terrain palette
    private readonly Dictionary<string, Texture2D> _terrainIcons = new();

    // Resource path constants
    private const string PATH_TERRAIN_ICONS = "TerrainIcons/";
    private const string PATH_FEATURE_ICONS = "FeatureIcons/";
    private const string PATH_EDITOR_ICONS = "EditorSettings/MapGridEditorIcons/";

    // GUI size constants
    private const int GUI_BUTTON_SIZE = 40;
    private const int GUI_PREVIEW_SIZE = 40;
    private const int GUI_SMALL_BUTTON_HEIGHT = 24;

    // Cached GUIStyles (avoid reallocating per-frame)
    private GUIStyle _guiStyleButton;
    private GUIStyle _guiStyleButtonSmall;
    private GUIStyle _guiStyleWrap;
    private GUIStyle _guiStyleBoldWrap;

    // Hotkey mapping for second-layer tools (KeyCode -> tool id)
    private Dictionary<KeyCode, string> _toolHotkeys = null;

    [MenuItem("Turnroot/Editors/Map Grid Editor")]
    public static void Open()
    {
        GetWindow<MapGridEditorWindow>("Map Grid Editor");
    }

    private void OnEnable()
    {
        _terrainAsset = TerrainTypes.LoadDefault();
        this.minSize = new Vector2(600, 480);
        this.maxSize = new Vector2(2000, 900);
        // Receive mouse move events so hover updates without clicks
        this.wantsMouseMove = true;
        // Ensure no second-tool is auto-selected when opening the window
        _selectedSecondTool = -1;
        _selectedSecondToolName = string.Empty;
        // Ensure default zoom is reset to 1 when the window is enabled
        _zoom = 1f;

        // Initialize hotkey map for quick tool selection
        _toolHotkeys = new Dictionary<KeyCode, string>()
        {
            { KeyCode.T, "treasure" },
            { KeyCode.D, "door" },
            { KeyCode.W, "warp" },
            { KeyCode.H, "healing" },
            { KeyCode.R, "ranged" },
            { KeyCode.M, "mechanism" },
            { KeyCode.C, "control" },
            { KeyCode.B, "breakable" },
            { KeyCode.S, "shelter" },
            { KeyCode.U, "underground" },
            { KeyCode.E, "eraser" },
        };
    }

    // Create GUIStyles lazily inside OnGUI (safe context for GUI functions).
    private void EnsureStyles()
    {
        if (_guiStyleButton != null)
            return;

        // It's important these are created while inside OnGUI so Unity's GUI skin is available.
        _guiStyleButton = new GUIStyle(GUI.skin.button);
        _guiStyleButton.fixedWidth = GUI_BUTTON_SIZE;
        _guiStyleButton.fixedHeight = GUI_BUTTON_SIZE;

        _guiStyleButtonSmall = new GUIStyle(GUI.skin.button);
        _guiStyleButtonSmall.fixedWidth = GUI_BUTTON_SIZE;
        _guiStyleButtonSmall.fixedHeight = GUI_SMALL_BUTTON_HEIGHT;

        _guiStyleWrap = new GUIStyle(EditorStyles.label) { wordWrap = true };
        _guiStyleBoldWrap = new GUIStyle(EditorStyles.boldLabel) { wordWrap = true };

        // Match style text color to the current editor theme so labels are readable in dark/light skins
        var labelColor = EditorStyles.label.normal.textColor;
        var boldLabelColor = EditorStyles.boldLabel.normal.textColor;
        _guiStyleWrap.normal.textColor = labelColor;
        _guiStyleBoldWrap.normal.textColor = boldLabelColor;
        // Allow the wrap styles to stretch to available width and ensure richText is disabled
        _guiStyleWrap.stretchWidth = true;
        _guiStyleBoldWrap.stretchWidth = true;
        _guiStyleWrap.richText = false;
        _guiStyleBoldWrap.richText = false;
    }

    private void OnGUI()
    {
        // Ensure GUIStyles are created while in the OnGUI context (safe to access GUI.skin / EditorStyles)
        EnsureStyles();

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        _grid = (MapGrid)EditorGUILayout.ObjectField(_grid, typeof(MapGrid), true);

        // Compact grid info and editable map name in the toolbar
        if (_grid != null)
        {
            // Show WxH compact label
            GUILayout.Label($"{_grid.GridWidth}x{_grid.GridHeight}", GUILayout.Width(60));

            // Editable map name field with label
            GUILayout.Label("Map Name:", GUILayout.Width(72));
            string newName = EditorGUILayout.TextField(
                _grid.MapName ?? string.Empty,
                GUILayout.Width(180)
            );
            if (newName != _grid.MapName)
            {
                Undo.RecordObject(_grid, "Edit Map Name");
                _grid.MapName = newName;
                EditorUtility.SetDirty(_grid);
                var _scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(_scene);
            }
        }

        if (GUILayout.Button("Refresh"))
        {
            _terrainAsset = TerrainTypes.LoadDefault();
            Repaint();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        // Ensure the window repaints on mouse move so hover coordinates update live.
        if (Event.current != null && Event.current.type == EventType.MouseMove)
        {
            Repaint();
        }

        // Keyboard shortcuts (global within this window). Do not intercept when editing text fields.
        Event keyEvent = Event.current;
        if (keyEvent != null)
        {
            if (keyEvent.type == EventType.KeyDown && !EditorGUIUtility.editingTextField)
            {
                // Mode switch
                if (keyEvent.keyCode == KeyCode.P)
                {
                    _mode = Mode.Paint;
                    // Untoggle any second-layer tool selection
                    _selectedSecondTool = -1;
                    _selectedSecondToolName = string.Empty;
                    keyEvent.Use();
                }

                // Tool hotkey lookup: use the pre-initialized map to avoid repetitive branches
                if (
                    _toolHotkeys != null
                    && _toolHotkeys.TryGetValue(keyEvent.keyCode, out var toolId)
                )
                {
                    _selectedSecondTool = System.Array.IndexOf(_secondToolIds, toolId);
                    _mode = Mode.Paint;
                    keyEvent.Use();
                }

                // Zoom shortcuts (Ctrl/Cmd + / - , include keypad variants)
                bool ctrlCmd = keyEvent.control || keyEvent.command;
                if (
                    ctrlCmd
                    && (
                        keyEvent.keyCode == KeyCode.Equals
                        || keyEvent.keyCode == KeyCode.Plus
                        || keyEvent.keyCode == KeyCode.KeypadPlus
                    )
                )
                {
                    _zoom = Mathf.Min(3f, _zoom + 0.1f);
                    keyEvent.Use();
                    Repaint();
                }
                else if (
                    ctrlCmd
                    && (
                        keyEvent.keyCode == KeyCode.Minus || keyEvent.keyCode == KeyCode.KeypadMinus
                    )
                )
                {
                    _zoom = Mathf.Max(0.25f, _zoom - 0.1f);
                    keyEvent.Use();
                    Repaint();
                }

                // Space: begin panning mode (we will watch KeyUp to clear)
                if (keyEvent.keyCode == KeyCode.Space)
                {
                    _spacePan = true;
                    keyEvent.Use();
                }
            }
            else if (keyEvent.type == EventType.KeyUp)
            {
                if (keyEvent.keyCode == KeyCode.Space)
                {
                    _spacePan = false;
                    keyEvent.Use();
                }
            }
        }

        // Auto-ensure grid points if the assigned MapGrid has an empty index.
        if (_grid != null && _grid.GetGridPoint(0, 0) == null)
        {
            _grid.EnsureGridPoints();
            EditorUtility.SetDirty(_grid);
            EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene()
            );
            SceneView.RepaintAll();
        }

        if (_grid == null)
        {
            EditorGUILayout.HelpBox("Assign a MapGrid to edit.", MessageType.Info);
            return;
        }

        // If the grid's internal index appears empty, offer to ensure points exist.
        if (_grid.GetGridPoint(0, 0) == null)
        {
            EditorGUILayout.HelpBox(
                "No labels, no colors. Grid points appear missing.",
                MessageType.Warning
            );
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Ensure Grid Points"))
            {
                _grid.EnsureGridPoints();
                EditorUtility.SetDirty(_grid);
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("Rebuild Index"))
            {
                _grid.RebuildGridDictionary();
                EditorUtility.SetDirty(_grid);
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();
        }

        if (_terrainAsset == null)
        {
            EditorGUILayout.HelpBox("No TerrainTypes asset found.", MessageType.Warning);
        }

        _mode = (Mode)
            GUILayout.Toolbar((int)_mode, new string[] { "Paint Terrain Types", "Test Movement" });

        // Split layout: left vertical toolbars (two columns) + main area on right
        EditorGUILayout.BeginHorizontal();

        // Left toolbars column (two vertical toolbars side-by-side)
        EditorGUILayout.BeginVertical(GUILayout.Width(120));
        EditorGUILayout.BeginHorizontal();
        // Palette column
        EditorGUILayout.BeginVertical(GUILayout.Width(60));
        if (_mode == Mode.Paint)
        {
            DrawTerrainPaletteVertical();
        }
        EditorGUILayout.EndVertical();

        // Second toolbar column (empty placeholder for now)
        EditorGUILayout.BeginVertical(GUILayout.Width(60));
        DrawLeftSecondToolbar();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        // Main area (right) - split into grid (left) and details (right)
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();

        // Left: grid and controls (expandable)
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

        // Test Movement controls
        if (_mode == Mode.TestMovement)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Movement:", GUILayout.Width(70));
            _testMovementValue = EditorGUILayout.IntSlider(
                _testMovementValue,
                1,
                10,
                GUILayout.Width(300)
            );
            if (GUILayout.Button("Clear Test", GUILayout.Width(90)))
            {
                _testMovementStart = null;
                _testMovementResults = null;
                Repaint();
            }
            EditorGUILayout.EndHorizontal();

            // Movement type toggles (same as A* tab) so TestMovement uses the same cost flags
            EditorGUILayout.BeginHorizontal();
            _asWalk = GUILayout.Toggle(_asWalk, "Walk", "Button");
            _asFly = GUILayout.Toggle(_asFly, "Fly", "Button");
            _asRide = GUILayout.Toggle(_asRide, "Ride", "Button");
            _asMagic = GUILayout.Toggle(_asMagic, "Magic", "Button");
            _asArmor = GUILayout.Toggle(_asArmor, "Armor", "Button");
            GUILayout.Label("Same-dir:", GUILayout.Width(70));
            _sameDirectionMultiplier = EditorGUILayout.Slider(
                _sameDirectionMultiplier,
                0.5f,
                1.1f,
                GUILayout.Width(150)
            );
            EditorGUILayout.EndHorizontal();
        }

        // Zoom controls
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Zoom:", GUILayout.Width(40));
        _zoom = EditorGUILayout.Slider(_zoom, 0.25f, 3f);
        EditorGUILayout.EndHorizontal();

        float statusBarH = 24f;
        // calculate a left area width that leaves room for left toolbars (120) and right details
        float rightPanelW = 220f;
        float leftAreaW = Mathf.Max(200f, position.width - 120f - rightPanelW);
        Rect area = GUILayoutUtility.GetRect(leftAreaW, position.height - 120 - statusBarH);
        DrawGridArea(area);

        EditorGUILayout.EndVertical(); // end grid column

        // Right: details panel for second-layer tools
        EditorGUILayout.BeginVertical(GUILayout.Width(220));

        // Use cached wrapped label styles for the right panel
        // (Grid dimensions moved to the compact toolbar display)

        if (_selectedSecondTool >= 0 && _selectedSecondTool < _secondToolNames.Length)
        {
            string toolId = _secondToolIds[_selectedSecondTool];
            DrawFeatureDetails(toolId, _guiStyleBoldWrap, _guiStyleWrap);
        }
        else
        {
            DrawWrappedLabel(new GUIContent("Feature details"), _guiStyleWrap, 196f);
        }
        EditorGUILayout.EndVertical(); // end details column

        EditorGUILayout.EndHorizontal();

        // Close main area and left toolbar horizontal
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        // Status bar showing hovered tile (row/col) - placed below both toolbars and main area
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        string hoverText =
            _hoveredCell.x >= 0
                ? $"Hovered: Row {_hoveredCell.x}, Col {_hoveredCell.y}"
                : "Hovered: (none)";
        // Expand width so text doesn't get clipped on the right
        GUILayout.Label(hoverText, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();
    }

    // DrawTerrainSelector removed - selection/palette moved to left toolbar (cleanup)

    // Vertical palette for the left toolbar
    private void DrawTerrainPaletteVertical()
    {
        if (_terrainAsset == null || _terrainAsset.Types == null)
            return;

        GUILayout.Label("Palette", EditorStyles.boldLabel);
        for (int i = 0; i < _terrainAsset.Types.Length; i++)
        {
            var t = _terrainAsset.Types[i];
            if (t == null)
                continue;

            Texture2D icon = GetTerrainIcon(t);
            GUIContent content =
                icon != null ? new GUIContent(icon, t.Name) : new GUIContent(t.Name);

            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.fixedWidth = 40;
            style.fixedHeight = 40;

            bool isSelected = _selectedTerrainIndex == i;
            bool newState = GUILayout.Toggle(isSelected, content, style);
            if (newState && !isSelected)
            {
                _selectedTerrainIndex = i;
            }
        }

        // Color preview square (same size as buttons). Click to open terrain dropdown.
        if (
            _terrainAsset != null
            && _terrainAsset.Types != null
            && _selectedTerrainIndex >= 0
            && _selectedTerrainIndex < _terrainAsset.Types.Length
        )
        {
            var col = _terrainAsset.Types[_selectedTerrainIndex].EditorColor;
            // Use an explicitly fixed 40x40 rect and don't expand width so it matches the buttons.
            Rect cRect = GUILayoutUtility.GetRect(
                40,
                40,
                GUILayout.Width(40),
                GUILayout.Height(40)
            );
            EditorGUI.DrawRect(cRect, col);
            // Make the color preview clickable to show a popup menu of terrain types
            if (GUI.Button(cRect, GUIContent.none, GUIStyle.none))
            {
                ShowTerrainPopup(cRect);
            }
        }
    }

    // Placeholder for the second left toolbar (empty for now)
    private void DrawLeftSecondToolbar()
    {
        GUILayout.Label("Tools", EditorStyles.boldLabel);

        // Second-layer tools: show a vertical list of toggle buttons matching palette size
        for (int i = 0; i < _secondToolIds.Length; i++)
        {
            string id = _secondToolIds[i];
            string label = _secondToolNames[i];

            // Try to load an icon from Resources/FeatureIcons/<id>
            Texture2D icon = GetSecondToolIcon(id);
            GUIContent content;
            if (icon != null)
                content = new GUIContent(icon, label);
            else
                content = new GUIContent(label.Substring(0, 1));

            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.fixedWidth = 40;
            style.fixedHeight = 40;

            bool isSelected = _selectedSecondTool == i;
            bool newState = GUILayout.Toggle(isSelected, content, style);
            if (newState && !isSelected)
            {
                _selectedSecondTool = i;
                // initialize the name field to empty or existing default
                _selectedSecondToolName = string.Empty;
            }
            else if (!newState && isSelected)
            {
                // Clicking again will deselect
                _selectedSecondTool = -1;
            }
        }

        GUILayout.FlexibleSpace();
    }

    // Show a terrain selection popup at the given rect
    private void ShowTerrainPopup(Rect rect)
    {
        if (_terrainAsset == null || _terrainAsset.Types == null)
            return;

        GenericMenu menu = new();
        for (int i = 0; i < _terrainAsset.Types.Length; i++)
        {
            var t = _terrainAsset.Types[i];
            string name = t != null ? t.Name ?? i.ToString() : i.ToString();
            int idx = i;
            bool on = idx == _selectedTerrainIndex;
            menu.AddItem(
                new GUIContent(name),
                on,
                () =>
                {
                    _selectedTerrainIndex = idx;
                    Repaint();
                }
            );
        }

        menu.DropDown(rect);
    }

    private Texture2D GetTerrainIcon(TerrainType t)
    {
        if (t == null)
        {
            return null;
        }
        // Use a stable cache key (prefer Id, otherwise Name)
        string key = !string.IsNullOrEmpty(t.Id) ? t.Id : t.Name;
        if (_terrainIcons.TryGetValue(key, out var cached))
        {
            return cached;
        }
        // Build candidate paths (id and name variants)
        string nameNoSpaces = (t.Name ?? string.Empty).Replace(" ", "").ToLower();
        string[] candidates = new string[]
        {
            PATH_TERRAIN_ICONS + t.Id,
            PATH_TERRAIN_ICONS + nameNoSpaces,
            PATH_EDITOR_ICONS + t.Id,
            PATH_EDITOR_ICONS + nameNoSpaces,
        };

        var tex = LoadIconFromPaths(key, candidates);
        return tex;
    }

    // Load icons for second-layer feature tools from Resources/FeatureIcons/<id>
    private Texture2D GetSecondToolIcon(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;
        string key = "feature_" + id;
        if (_terrainIcons.TryGetValue(key, out var cached))
            return cached;
        string nameNoSpaces = id.Replace(" ", "").ToLower();
        string[] candidates = new string[]
        {
            PATH_FEATURE_ICONS + id,
            PATH_FEATURE_ICONS + nameNoSpaces,
            PATH_EDITOR_ICONS + id,
            PATH_EDITOR_ICONS + nameNoSpaces,
        };

        var tex = LoadIconFromPaths(key, candidates);
        return tex;
    }

    // Helper: try loading a Texture2D from a set of Resources paths and cache the result.
    private Texture2D LoadIconFromPaths(string cacheKey, string[] paths)
    {
        if (_terrainIcons.TryGetValue(cacheKey, out var cached))
            return cached;

        Texture2D tex = null;
        var tried = new List<string>();
        foreach (var p in paths)
        {
            if (string.IsNullOrEmpty(p))
                continue;
            tried.Add(p);
            tex = Resources.Load<Texture2D>(p);
            if (tex != null)
                break;
        }

        if (tex == null)
        {
            // Keep the cache entry (null) to avoid repeated lookups and log debug info once.
            Debug.LogWarning(
                $"MapGridEditorWindow: icon not found for '{cacheKey}'. Tried: {string.Join(", ", tried)}"
            );
        }

        _terrainIcons[cacheKey] = tex;
        return tex;
    }

    private void DrawGridArea(Rect area)
    {
        // Editor window renders cell colors and overlays directly.

        float cellSize = _baseCellSize * _zoom;
        int width = _grid.GridWidth;
        int height = _grid.GridHeight;

        // Scrollable view
        _scroll = GUI.BeginScrollView(
            area,
            _scroll,
            new Rect(0, 0, width * cellSize, height * cellSize)
        );

        // Background
        var contentRect = new Rect(0, 0, width * cellSize, height * cellSize);
        EditorGUI.DrawRect(contentRect, Color.grey * 0.2f);

        // Show pan cursor when space is held or actively panning (over the scroll content)
        if (_spacePan || _isPanning)
        {
            EditorGUIUtility.AddCursorRect(contentRect, MouseCursor.Pan);
        }

        Event e = Event.current;
        // After BeginScrollView the GUI coordinate space is already translated into
        // the scroll content. Use Event.mousePosition plus the scroll offset to
        // get content-local coordinates (do not subtract the window `area` position).
        Vector2 localMouse = e.mousePosition + _scroll;

        float contentW = width * cellSize;
        float contentH = height * cellSize;
        if (
            localMouse.x >= 0
            && localMouse.y >= 0
            && localMouse.x <= contentW
            && localMouse.y <= contentH
        )
        {
            Vector2Int cell = MouseToCell(localMouse, cellSize);
            _hoveredCell = ClampCell(cell, width, height);
        }
        else
        {
            _hoveredCell = new Vector2Int(-1, -1);
        }

        if (e.type == EventType.MouseMove)
        {
            Repaint();
        }

        HandleMouse(e, localMouse, cellSize, width, height);

        // Determine farthest reachable cost for TestMovement visualization
        float _testMaxCost = float.NegativeInfinity;
        if (_mode == Mode.TestMovement && _testMovementResults != null)
        {
            foreach (var kv in _testMovementResults)
            {
                if (kv.Value > _testMaxCost)
                {
                    _testMaxCost = kv.Value;
                }
            }
            if (float.IsNegativeInfinity(_testMaxCost))
            {
                _testMaxCost = float.MinValue;
            }
        }

        // Draw cells
        for (int r = 0; r < width; r++)
        {
            for (int c = 0; c < height; c++)
            {
                Rect cellRect = new(r * cellSize, c * cellSize, cellSize, cellSize);
                // Terrain color
                var point = _grid.GetGridPoint(r, c);
                Color fill = Color.white;
                if (point != null)
                {
                    TerrainType tt = null;
                    if (_terrainAsset != null)
                    {
                        tt = _terrainAsset.GetTypeById(point.TerrainTypeId);
                    }
                    tt ??= point.SelectedTerrainType;

                    if (tt != null)
                    {
                        fill = tt.EditorColor;
                    }
                }
                EditorGUI.DrawRect(cellRect, fill);

                // Selection overlay (paint mode)
                if (_mode == Mode.Paint && _isDragging)
                {
                    int minR = Mathf.Min(_dragStart.x, _dragEnd.x);
                    int maxR = Mathf.Max(_dragStart.x, _dragEnd.x);
                    int minC = Mathf.Min(_dragStart.y, _dragEnd.y);
                    int maxC = Mathf.Max(_dragStart.y, _dragEnd.y);
                    if (r >= minR && r <= maxR && c >= minC && c <= maxC)
                    {
                        EditorGUI.DrawRect(cellRect, new Color(0, 0, 0, 0.25f));
                    }
                }

                // Test Movement overlays (drawn when TestMovement mode is active)
                if (_mode == Mode.TestMovement && _testMovementResults != null)
                {
                    if (point != null && _testMovementResults.TryGetValue(point, out var cost))
                    {
                        EditorGUI.DrawRect(cellRect, new Color(0.4f, 0f, 0.4f, 1f));

                        // Draw cost overlay centered on the tile for debugging/inspection.
                        var costText = cost.ToString("0");
                        var txtStyle = new GUIStyle(EditorStyles.boldLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontSize = Mathf.Max(10, Mathf.FloorToInt(cellSize / 3f)),
                        };
                        txtStyle.normal.textColor = Color.white;
                        EditorGUI.LabelField(cellRect, costText, txtStyle);
                    }
                }

                // Feature overlay (second-layer): draw large bold letter if the point has a feature
                if (point != null)
                {
                    string letter = MapGridPointFeature.GetFeatureLetter(point.FeatureTypeId);
                    if (!string.IsNullOrEmpty(letter))
                    {
                        var txtStyleFeature = new GUIStyle(EditorStyles.boldLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontSize = Mathf.Max(10, Mathf.FloorToInt(cellSize * 0.8f)),
                            fontStyle = FontStyle.Bold,
                        };

                        // Choose contrasting text color based on underlying terrain fill
                        float luminance = 0.299f * fill.r + 0.587f * fill.g + 0.114f * fill.b;
                        if (luminance < 0.5f)
                            txtStyleFeature.normal.textColor = Color.white;
                        else
                            txtStyleFeature.normal.textColor = Color.black;

                        EditorGUI.LabelField(cellRect, letter, txtStyleFeature);
                    }
                }

                // 1px black lines
                EditorGUI.DrawRect(
                    new Rect(cellRect.x, cellRect.y, cellRect.width, 1f),
                    Color.black
                );
                EditorGUI.DrawRect(
                    new Rect(cellRect.x, cellRect.y + cellRect.height - 1f, cellRect.width, 1f),
                    Color.black
                );
                EditorGUI.DrawRect(
                    new Rect(cellRect.x, cellRect.y, 1f, cellRect.height),
                    Color.black
                );
                EditorGUI.DrawRect(
                    new Rect(cellRect.x + cellRect.width - 1f, cellRect.y, 1f, cellRect.height),
                    Color.black
                );
            }
        }

        GUI.EndScrollView();

        if (GUI.changed)
        {
            Repaint();
        }
    }

    private void HandleMouse(Event e, Vector2 localMouse, float cellSize, int width, int height)
    {
        float contentW = width * cellSize;
        float contentH = height * cellSize;
        bool inside =
            localMouse.x >= 0
            && localMouse.y >= 0
            && localMouse.x <= contentW
            && localMouse.y <= contentH;

        // Space + drag pans the view. Use Event.delta for smooth panning inside the scroll view.
        if (_spacePan)
        {
            if (e.type == EventType.MouseDown && e.button == 0 && inside)
            {
                GUI.FocusControl(null);
                _isPanning = true;
                e.Use();
                return;
            }
            else if (e.type == EventType.MouseDrag && _isPanning)
            {
                // e.delta is in GUI coordinates; subtract it to move the content opposite to mouse drag
                _scroll -= e.delta;
                // Prevent negative scroll which can cause jitter; clamp to zero minimum
                _scroll.x = Mathf.Max(0f, _scroll.x);
                _scroll.y = Mathf.Max(0f, _scroll.y);
                Repaint();
                e.Use();
                return;
            }
            else if (e.type == EventType.MouseUp && e.button == 0 && _isPanning)
            {
                _isPanning = false;
                e.Use();
                return;
            }
        }

        if (_mode == Mode.Paint)
        {
            if (e.type == EventType.MouseDown && e.button == 0 && inside)
            {
                GUI.FocusControl(null);
                Vector2Int cell = MouseToCell(localMouse, cellSize);
                _dragStart = ClampCell(cell, width, height);
                _dragEnd = _dragStart;
                _isDragging = true;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && _isDragging)
            {
                Vector2Int cell = MouseToCell(localMouse, cellSize);
                _dragEnd = ClampCell(cell, width, height);
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0 && _isDragging)
            {
                _isDragging = false;
                ApplyTerrainToSelection();
                e.Use();
            }
        }
        else if (_mode == Mode.TestMovement)
        {
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (!inside)
                {
                    e.Use();
                    return;
                }
                Vector2Int cell = MouseToCell(localMouse, cellSize);
                Vector2Int cellClamped = ClampCell(cell, width, height);
                var clicked = _grid.GetGridPoint(cellClamped.x, cellClamped.y);
                if (clicked != null)
                {
                    _testMovementStart = clicked;
                    var aStar = new AStarModified();
                    _testMovementResults = aStar.GetReachable(
                        _grid,
                        _testMovementStart,
                        _testMovementValue,
                        _asWalk,
                        _asFly,
                        _asRide,
                        _asMagic,
                        _asArmor,
                        _sameDirectionMultiplier
                    );
                    Repaint();
                }
                e.Use();
            }
            else if (e.type == EventType.MouseDown && e.button == 1)
            {
                _testMovementStart = null;
                _testMovementResults = null;
                e.Use();
            }
        }
    }

    private Vector2Int MouseToCell(Vector2 localMouse, float cellSize)
    {
        int r = Mathf.FloorToInt(localMouse.x / cellSize);
        int c = Mathf.FloorToInt(localMouse.y / cellSize);
        return new Vector2Int(r, c);
    }

    private Vector2Int ClampCell(Vector2Int cell, int width, int height)
    {
        cell.x = Mathf.Clamp(cell.x, 0, Mathf.Max(0, width - 1));
        cell.y = Mathf.Clamp(cell.y, 0, Mathf.Max(0, height - 1));
        return cell;
    }

    private void ApplyTerrainToSelection()
    {
        if (_grid == null || _terrainAsset == null || _terrainAsset.Types == null)
        {
            return;
        }

        int minR = Mathf.Min(_dragStart.x, _dragEnd.x);
        int maxR = Mathf.Max(_dragStart.x, _dragEnd.x);
        int minC = Mathf.Min(_dragStart.y, _dragEnd.y);
        int maxC = Mathf.Max(_dragStart.y, _dragEnd.y);
        var chosen = _terrainAsset.Types[_selectedTerrainIndex];
#if UNITY_EDITOR
        // Batch apply: record undo per-point and mark each point dirty, then mark scene dirty once.
        int painted = 0;
        for (int r = minR; r <= maxR; r++)
        {
            for (int c = minC; c <= maxC; c++)
            {
                var p = _grid.GetGridPoint(r, c);
                if (p != null)
                {
                    Undo.RecordObject(p, "MapGrid Edit");
                    // If a second-layer tool is selected, only modify the feature layer.
                    if (_selectedSecondTool >= 0 && _selectedSecondTool < _secondToolIds.Length)
                    {
                        string selId = _secondToolIds[_selectedSecondTool];
                        bool singleCell = (minR == maxR && minC == maxC);
                        ApplySecondToolToPoint(p, selId, singleCell);
                    }
                    else
                    {
                        // No second-layer tool selected: apply terrain painting as before
                        p.SetTerrainTypeId(chosen.Id);
                    }

                    EditorUtility.SetDirty(p);
                    painted++;
                }
            }
        }
        EditorUtility.SetDirty(_grid);
        // Persist the feature layer to the MapGrid serialized records
        _grid.SaveFeatureLayer();
        // Mark the active scene dirty so changes persist
        var _scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(_scene);
        SceneView.RepaintAll();
#endif
    }

    // Helper to apply (or toggle) a second-layer tool on a single point
    private void ApplySecondToolToPoint(MapGridPoint p, string selId, bool singleCell)
    {
        if (p == null || string.IsNullOrEmpty(selId))
            return;

        // Delegate feature application logic to MapGridPoint to keep editor code thin.
        p.ApplyFeature(selId, _selectedSecondToolName ?? string.Empty, singleCell);
    }

    // Draw the right-panel UI for the selected feature tool
    private void DrawFeatureDetails(string toolId, GUIStyle boldWrap, GUIStyle wrapStyle)
    {
        // Use safe fallbacks in case cached styles are not initialized yet.
        // Prefer the provided cached styles; otherwise create fresh styles based on EditorStyles
        GUIStyle safeBold = null;
        GUIStyle safeWrap = null;
        if (boldWrap != null)
            safeBold = boldWrap;
        else
            safeBold = new GUIStyle(EditorStyles.boldLabel) { wordWrap = true };

        if (wrapStyle != null)
            safeWrap = wrapStyle;
        else
            safeWrap = new GUIStyle(EditorStyles.label) { wordWrap = true };

        // Ensure text colors match the current theme (dark/light)
        safeWrap.normal.textColor = EditorStyles.label.normal.textColor;
        safeBold.normal.textColor = EditorStyles.boldLabel.normal.textColor;

        // Take a snapshot during the Layout event so Layout and Repaint produce
        // the same number of controls even if hover changes mid-pass.
        if (Event.current != null && Event.current.type == EventType.Layout)
        {
            _detailSnapshotHovered = _hoveredCell;
            _detailSnapshotProps = null;
            if (_detailSnapshotHovered.x >= 0 && _detailSnapshotHovered.y >= 0 && _grid != null)
            {
                var hp = _grid.GetGridPoint(_detailSnapshotHovered.x, _detailSnapshotHovered.y);
                if (hp != null && hp.FeatureTypeId == toolId)
                {
                    var live = hp.GetAllFeatureProperties();
                    _detailSnapshotProps = new List<KeyValuePair<string, string>>();
                    if (live != null)
                    {
                        // Handle different possible return shapes for feature properties
                        foreach (var vv in live)
                        {
                            var kv = ExtractKeyValuePair(vv);
                            _detailSnapshotProps.Add(kv);
                        }
                    }
                }
            }
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        // Constrain inner contents to a fixed width slightly smaller than the
        // outer details column so word-wrap works reliably.
        EditorGUILayout.BeginVertical(GUILayout.Width(196));
        switch (toolId)
        {
            case "eraser":
                DrawWrappedLabel(new GUIContent("Feature: Eraser"), safeBold, 196f);
                DrawWrappedLabel(
                    new GUIContent("Eraser clears the feature layer on painted tiles."),
                    safeWrap,
                    196f
                );
                _selectedSecondToolName = string.Empty;
                break;
            default:
                // Find a friendly name if available
                string friendly = toolId;
                int idx = System.Array.IndexOf(_secondToolIds, toolId);
                if (idx >= 0 && idx < _secondToolNames.Length)
                    friendly = _secondToolNames[idx];

                DrawWrappedLabel(new GUIContent($"Feature: {friendly}"), safeBold, 196f);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Name:", GUILayout.Width(50));
                _selectedSecondToolName = EditorGUILayout.TextField(
                    _selectedSecondToolName,
                    GUILayout.ExpandWidth(true)
                );
                EditorGUILayout.EndHorizontal();

                // Show per-feature properties using a snapshot captured during Layout
                if (
                    _detailSnapshotHovered.x >= 0
                    && _detailSnapshotHovered.y >= 0
                    && _detailSnapshotProps != null
                    && _grid != null
                )
                {
                    var hoveredPoint = _grid.GetGridPoint(
                        _detailSnapshotHovered.x,
                        _detailSnapshotHovered.y
                    );
                    GUILayout.Space(6);
                    DrawWrappedLabel(new GUIContent("Properties (key / value)"), safeBold, 196f);

                    // Display editable list from the snapshot (stable control counts)
                    var props = _detailSnapshotProps;
                    var removeKeys = new List<string>();
                    var editOldKeys = new List<string>();
                    var editNewKeys = new List<string>();
                    var editNewVals = new List<string>();
                    bool addRequested = false;

                    for (int i = 0; i < props.Count; i++)
                    {
                        var p = props[i];
                        EditorGUILayout.BeginHorizontal();
                        string newKey = EditorGUILayout.TextField(p.Key, GUILayout.Width(120));
                        string newVal = EditorGUILayout.TextField(
                            p.Value,
                            GUILayout.ExpandWidth(true)
                        );
                        if (GUILayout.Button("Remove", GUILayout.Width(60)))
                        {
                            removeKeys.Add(p.Key);
                        }
                        EditorGUILayout.EndHorizontal();

                        if (newKey != p.Key || newVal != p.Value)
                        {
                            editOldKeys.Add(p.Key);
                            editNewKeys.Add(newKey ?? string.Empty);
                            editNewVals.Add(newVal ?? string.Empty);
                        }
                    }

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Add Property", GUILayout.Width(120)))
                    {
                        addRequested = true;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (removeKeys.Count > 0 || editOldKeys.Count > 0 || addRequested)
                    {
                        var capturedPoint = hoveredPoint;
                        var capturedGrid = _grid;
                        var rkList = new List<string>(removeKeys);
                        var oldKeys = new List<string>(editOldKeys);
                        var newKeys = new List<string>(editNewKeys);
                        var newVals = new List<string>(editNewVals);
                        bool willAdd = addRequested;

                        EditorApplication.delayCall += () =>
                        {
                            if (capturedPoint == null)
                                return;

                            Undo.RecordObject(capturedPoint, "Edit Feature Properties");

                            foreach (var rk in rkList)
                            {
                                capturedPoint.ClearFeatureProperty(rk);
                            }

                            for (int ei = 0; ei < oldKeys.Count; ei++)
                            {
                                var oldk = oldKeys[ei];
                                var newk = newKeys[ei];
                                var newv = newVals[ei];
                                capturedPoint.ClearFeatureProperty(oldk);
                                if (!string.IsNullOrEmpty(newk))
                                    capturedPoint.SetFeatureProperty(newk, newv);
                            }

                            if (willAdd)
                            {
                                capturedPoint.SetFeatureProperty("new_key", string.Empty);
                            }

                            EditorUtility.SetDirty(capturedPoint);
                            if (capturedGrid != null)
                                capturedGrid.SaveFeatureLayer();
#if UNITY_EDITOR
                            var _scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                            EditorSceneManager.MarkSceneDirty(_scene);
                            SceneView.RepaintAll();
#endif
                        };
                    }
                }
                else
                {
                    GUILayout.Space(6);
                    DrawWrappedLabel(
                        new GUIContent("Hover a tile with this feature to edit its properties."),
                        safeWrap,
                        196f
                    );
                }
                break;
        }
        // End inner fixed-width vertical
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
    }

    // Helper: extract key/value from various possible feature property shapes
    private KeyValuePair<string, string> ExtractKeyValuePair(object item)
    {
        if (item == null)
            return new KeyValuePair<string, string>(string.Empty, string.Empty);

        var type = item.GetType();

        // Try fields first (common for lightweight data structs)
        var fKey = type.GetField("key") ?? type.GetField("Key");
        var fVal = type.GetField("value") ?? type.GetField("Value");
        if (fKey != null && fVal != null)
        {
            var k = fKey.GetValue(item)?.ToString() ?? string.Empty;
            var v = fVal.GetValue(item)?.ToString() ?? string.Empty;
            return new KeyValuePair<string, string>(k, v);
        }

        // Try properties
        var pKey = type.GetProperty("key") ?? type.GetProperty("Key");
        var pVal = type.GetProperty("value") ?? type.GetProperty("Value");
        if (pKey != null && pVal != null)
        {
            var k = pKey.GetValue(item)?.ToString() ?? string.Empty;
            var v = pVal.GetValue(item)?.ToString() ?? string.Empty;
            return new KeyValuePair<string, string>(k, v);
        }

        // Last resort: try ToString splitting (not ideal but safe)
        var s = item.ToString() ?? string.Empty;
        return new KeyValuePair<string, string>(s, string.Empty);
    }

    // Helper: draw a wrapped label within an explicit fixed width using a rect
    private void DrawWrappedLabel(GUIContent content, GUIStyle style, float width)
    {
        if (content == null)
            content = new GUIContent(string.Empty);

        GUIStyle s = style != null ? new GUIStyle(style) : new GUIStyle(EditorStyles.label);
        s.wordWrap = true;

        float h = s.CalcHeight(content, width);
        Rect r = GUILayoutUtility.GetRect(width, h, GUILayout.Width(width));
        EditorGUI.LabelField(r, content, s);
    }
}
#endif

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ClickHelperPopup : EditorWindow
{
    private static readonly Type[] DetectableTypes = new Type[]
    {
        typeof(MeshRenderer),
        typeof(SkinnedMeshRenderer),
        typeof(SpriteRenderer),
        typeof(MeshFilter),
        typeof(Terrain),
        typeof(ParticleSystemRenderer),
        typeof(Collider),
        typeof(Collider2D),
        typeof(Canvas),
        typeof(CanvasRenderer),
        typeof(RectTransform),
        typeof(Image),
        typeof(RawImage),
        typeof(Text),
        typeof(Button),
        typeof(Toggle),
        typeof(Slider),
        typeof(Scrollbar),
        typeof(Dropdown),
        typeof(InputField),
        typeof(ScrollRect),
        typeof(Mask),
        typeof(RectMask2D),
        typeof(LayoutGroup),
        typeof(Graphic),
    };

    private class Entry
    {
        public GameObject gameObject;
        public List<Component> components = new List<Component>();
    }

    private List<Entry> _entries;

    // State
    private int _selectedLeftIndex = -1; // Index currently active in Left Panel
    private int _hoveredRightIndex = -1; // Current mouse hover index in Right Panel
    
    private Vector2 _scrollPosLeft;
    private Vector2 _scrollPosRight;

    private const float RowHeight = 22f;
    private const float LeftPanelWidth = 220f;
    private const float RightPanelWidth = 250f;
    private const float WindowWidth = LeftPanelWidth + RightPanelWidth;
    private const float MaxVisibleRows = 20f;
    private const float IconSize = 16f;

    private static GUIStyle _rowStyle;
    private static GUIStyle _rowHoverStyle;
    private static GUIStyle _rowSelectedStyle;
    private static GUIStyle _labelStyle;
    private static GUIStyle _componentLabelStyle;
    private static GUIStyle _separatorStyle;

    public static event Action<GameObject> OnHighlightRequested;
    public static event Action OnHighlightCleared;

    public static ClickHelperPopup Show(Vector2 screenPos, List<GameObject> objects)
    {
        var window = CreateInstance<ClickHelperPopup>();
        window.BuildEntries(objects);

        int rowCount = Mathf.Max(objects.Count, 1);
        if (rowCount < 8) rowCount = 8; // Min height

        float height = Mathf.Min(rowCount, MaxVisibleRows) * RowHeight;

        // Position slightly offset to not cover the mouse immediately if possible
        window.ShowAsDropDown(new Rect(screenPos, Vector2.zero), new Vector2(WindowWidth, height));
        window.Focus();

        // Highlight first item by default if available
        if (window._entries.Count > 0)
        {
            window._selectedLeftIndex = 0;
            window.PreviewSelection(window._entries[0].gameObject);
        }

        return window;
    }

    private void BuildEntries(List<GameObject> objects)
    {
        _entries = new List<Entry>();
        foreach (var go in objects)
        {
            if (go == null) continue;
            var entry = new Entry { gameObject = go };

            foreach (var type in DetectableTypes)
            {
                if (typeof(Component).IsAssignableFrom(type))
                {
                    var comps = go.GetComponents(type);
                    foreach (var c in comps)
                    {
                        if (c != null && !entry.components.Contains(c))
                            entry.components.Add(c);
                    }
                }
            }
            _entries.Add(entry);
        }
    }

    private void OnEnable()
    {
        wantsMouseMove = true;
    }

    private void OnDisable()
    {
        OnHighlightCleared?.Invoke();
    }

    private void InitStyles()
    {
        if (_rowStyle != null) return;

        _rowStyle = new GUIStyle(GUIStyle.none)
        {
            padding = new RectOffset(6, 6, 2, 2),
            fixedHeight = RowHeight,
            alignment = TextAnchor.MiddleLeft
        };

        _rowHoverStyle = new GUIStyle(_rowStyle);
        var hoverTex = new Texture2D(1, 1);
        hoverTex.SetPixel(0, 0, new Color(0.24f, 0.48f, 0.9f, 0.5f));
        hoverTex.Apply();
        _rowHoverStyle.normal.background = hoverTex;
        _rowHoverStyle.normal.textColor = Color.white;

        _rowSelectedStyle = new GUIStyle(_rowStyle);
        var selectedTex = new Texture2D(1, 1);
        selectedTex.SetPixel(0, 0, new Color(0.24f, 0.48f, 0.9f, 0.8f));
        selectedTex.Apply();
        _rowSelectedStyle.normal.background = selectedTex;
        _rowSelectedStyle.normal.textColor = Color.white;

        _labelStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleLeft,
            richText = true,
            clipping = TextClipping.Clip
        };

        _componentLabelStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 11,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(4, 0, 0, 0),
            normal = { textColor = new Color(0.8f, 0.8f, 0.8f) }
        };

        _separatorStyle = new GUIStyle(GUIStyle.none);
        var sepTex = new Texture2D(1, 1);
        sepTex.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f, 0.5f));
        sepTex.Apply();
        _separatorStyle.normal.background = sepTex;
    }

    private void OnGUI()
    {
        if (_entries == null || _entries.Count == 0)
        {
            Close();
            return;
        }

        InitStyles();
        HandleKeyboard();

        GUILayout.BeginHorizontal();

        // --- Left Panel: GameObjects ---
        GUILayout.BeginVertical(GUILayout.Width(LeftPanelWidth));
        
        _scrollPosLeft = GUILayout.BeginScrollView(_scrollPosLeft, GUIStyle.none, GUI.skin.verticalScrollbar);

        DrawSelectAllRow();

        for (int i = 0; i < _entries.Count; i++)
        {
            DrawLeftPanelRow(i);
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        // Separator Line
        GUILayout.Box("", _separatorStyle, GUILayout.Width(1), GUILayout.ExpandHeight(true));

        // --- Right Panel: Components ---
        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        
        _scrollPosRight = GUILayout.BeginScrollView(_scrollPosRight, GUIStyle.none, GUI.skin.verticalScrollbar);

        if (_selectedLeftIndex >= 0 && _selectedLeftIndex < _entries.Count)
        {
            var activeEntry = _entries[_selectedLeftIndex];
            if (activeEntry.components.Count > 0)
            {
                for (int i = 0; i < activeEntry.components.Count; i++)
                {
                    DrawRightPanelRow(activeEntry.components[i], activeEntry.gameObject, i);
                }
            }
            else
            {
                GUILayout.Space(20);
                GUILayout.Label("No Components", EditorStyles.centeredGreyMiniLabel);
            }
        }
        else if (_selectedLeftIndex == -1) // Select All
        {
            GUILayout.Space(20);
            GUILayout.Label("Multiple Selection", EditorStyles.centeredGreyMiniLabel);
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        if (Event.current.type == EventType.MouseMove)
            Repaint();
    }

    private void DrawSelectAllRow()
    {
        bool isSelected = _selectedLeftIndex == -1;
        
         Rect rowRect = EditorGUILayout.BeginHorizontal(
            isSelected ? _rowSelectedStyle : _rowStyle,
            GUILayout.Height(RowHeight));

        GUILayout.Label(" Select All (" + _entries.Count + ")", _labelStyle);

        EditorGUILayout.EndHorizontal();

        HandleRowEvents(rowRect, -1, true);
    }

    private void DrawLeftPanelRow(int index)
    {
        Entry entry = _entries[index];
        bool isSelected = index == _selectedLeftIndex;

        Rect rowRect = EditorGUILayout.BeginHorizontal(
            isSelected ? _rowSelectedStyle : _rowStyle,
            GUILayout.Height(RowHeight));

        // Icon
        var icon = EditorGUIUtility.ObjectContent(entry.gameObject, typeof(GameObject)).image;
        if (icon != null)
        {
            Rect iconRect = GUILayoutUtility.GetRect(IconSize, IconSize,
                GUILayout.Width(IconSize), GUILayout.Height(IconSize));
            iconRect.y += (RowHeight - IconSize) * 0.5f;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
        }

        // Name
        GUILayout.Label(entry.gameObject.name, _labelStyle);

        // Arrow indicator if selected
        if (isSelected)
        {
            Rect arrowRect = new Rect(rowRect.xMax - 20, rowRect.y + (RowHeight - 12) * 0.5f, 12, 12);
            GUI.Label(arrowRect, "\u25B6", EditorStyles.miniLabel);
        }

        EditorGUILayout.EndHorizontal();

        HandleRowEvents(rowRect, index, true);
    }
    
    private void HandleRowEvents(Rect rowRect, int index, bool isLeftPanel)
    {
        Event e = Event.current;
        
        // Hover Logic
        if (e.type == EventType.MouseMove && rowRect.Contains(e.mousePosition))
        {
            if (isLeftPanel)
            {
                 if (_selectedLeftIndex != index)
                {
                    _selectedLeftIndex = index;
                    _hoveredRightIndex = -1;
                    
                    if (index >= 0)
                        PreviewSelection(_entries[index].gameObject);
                    else
                        OnHighlightCleared?.Invoke(); // Clear custom highlight
                         
                    Repaint();
                }
            }
            else
            {
                if (_hoveredRightIndex != index)
                {
                    _hoveredRightIndex = index;
                    Repaint();
                }
            }
        }
        
        // Click Logic
        if (e.type == EventType.MouseDown && rowRect.Contains(e.mousePosition))
        {
            e.Use();
            if (isLeftPanel)
            {
                if (index == -1) ConfirmSelectAll();
                else if (index >= 0) ConfirmSelection(_entries[index].gameObject);
            }
        }
    }

    private void DrawRightPanelRow(Component component, GameObject parent, int index)
    {
        if (component == null) return;

        bool isHovered = index == _hoveredRightIndex;
        
        Rect rowRect = EditorGUILayout.BeginHorizontal(
            isHovered ? _rowHoverStyle : _rowStyle,
            GUILayout.Height(RowHeight));

        var icon = EditorGUIUtility.ObjectContent(component, component.GetType()).image;
        if (icon != null)
        {
            Rect iconRect = GUILayoutUtility.GetRect(IconSize, IconSize,
                GUILayout.Width(IconSize), GUILayout.Height(IconSize));
            iconRect.y += (RowHeight - IconSize) * 0.5f;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
        }

        GUILayout.Label(ObjectNames.NicifyVariableName(component.GetType().Name), _componentLabelStyle);

        EditorGUILayout.EndHorizontal();

        Event e = Event.current;
        if (e.type == EventType.MouseMove && rowRect.Contains(e.mousePosition))
        {
            if (_hoveredRightIndex != index)
            {
                _hoveredRightIndex = index;
                Repaint();
            }
        }
        else if (e.type == EventType.MouseDown && rowRect.Contains(e.mousePosition))
        {
            e.Use();
            ConfirmSelection(parent);
            EditorApplication.delayCall += () =>
            {
                // Just use PingObject to flash the object in Hierarchy
                EditorGUIUtility.PingObject(component);
            };
        }
    }

    private void HandleKeyboard()
    {
        Event e = Event.current;
        if (e.type != EventType.KeyDown) return;

        bool isRightFocus = _hoveredRightIndex != -1;

        switch (e.keyCode)
        {
            case KeyCode.DownArrow:
                if (isRightFocus)
                {
                    if (_selectedLeftIndex >= 0)
                    {
                        var comps = _entries[_selectedLeftIndex].components;
                        _hoveredRightIndex = Mathf.Min(_hoveredRightIndex + 1, comps.Count - 1);
                        ScrollToRightIndex(_hoveredRightIndex);
                    }
                }
                else
                {
                    int next = Mathf.Min(_selectedLeftIndex + 1, _entries.Count - 1);
                    if (next != _selectedLeftIndex)
                    {
                        _selectedLeftIndex = next;
                        _hoveredRightIndex = -1;
                        ScrollToLeftIndex(_selectedLeftIndex);
                        if (_selectedLeftIndex >= 0)
                            PreviewSelection(_entries[_selectedLeftIndex].gameObject);
                    }
                }
                e.Use();
                Repaint();
                break;

            case KeyCode.UpArrow:
                if (isRightFocus)
                {
                    _hoveredRightIndex = Mathf.Max(_hoveredRightIndex - 1, 0);
                    ScrollToRightIndex(_hoveredRightIndex);
                }
                else
                {
                    int prev = Mathf.Max(_selectedLeftIndex - 1, -1);
                    if (prev != _selectedLeftIndex)
                    {
                        _selectedLeftIndex = prev;
                        _hoveredRightIndex = -1;
                        ScrollToLeftIndex(_selectedLeftIndex);
                        if (_selectedLeftIndex >= 0)
                            PreviewSelection(_entries[_selectedLeftIndex].gameObject);
                        else
                            OnHighlightCleared?.Invoke();
                    }
                }
                e.Use();
                Repaint();
                break;
                
            case KeyCode.RightArrow:
                if (!isRightFocus && _selectedLeftIndex >= 0 && _entries[_selectedLeftIndex].components.Count > 0)
                {
                    _hoveredRightIndex = 0;
                    Repaint();
                }
                e.Use();
                break;
                
            case KeyCode.LeftArrow:
                if (isRightFocus)
                {
                    _hoveredRightIndex = -1;
                    Repaint();
                }
                e.Use();
                break;

            case KeyCode.Return:
            case KeyCode.KeypadEnter:
                if (isRightFocus && _selectedLeftIndex >= 0 && _hoveredRightIndex >= 0)
                {
                    var entry = _entries[_selectedLeftIndex];
                     ConfirmSelection(entry.gameObject);
                }
                else
                {
                    if (_selectedLeftIndex == -1) ConfirmSelectAll();
                    else if (_selectedLeftIndex >= 0) ConfirmSelection(_entries[_selectedLeftIndex].gameObject);
                }
                e.Use();
                break;

            case KeyCode.Escape:
                e.Use();
                Close();
                break;
        }
    }

    private void PreviewSelection(GameObject go)
    {
        // Only trigger visual highlight in Scene, do NOT change Selection.activeGameObject
        OnHighlightRequested?.Invoke(go);
    }

    private void ConfirmSelection(GameObject go)
    {
        Selection.activeGameObject = go;
        EditorGUIUtility.PingObject(go);
        
        if (ClickHelperSettings.Behavior == SelectionBehavior.SelectAndFocus)
            SceneView.lastActiveSceneView?.FrameSelected();
            
        Close();
    }

    private void ConfirmSelectAll()
    {
        List<UnityEngine.Object> all = new List<UnityEngine.Object>();
        foreach (var entry in _entries)
        {
            if (entry.gameObject != null)
                all.Add(entry.gameObject);
        }
        Selection.objects = all.ToArray();
        Close();
    }

    private void ScrollToLeftIndex(int index)
    {
        float targetY = (index + 1) * RowHeight;
        float viewHeight = position.height;
        if (targetY < _scrollPosLeft.y + RowHeight) _scrollPosLeft.y = Mathf.Max(0, targetY - RowHeight);
        else if (targetY > _scrollPosLeft.y + viewHeight - RowHeight) _scrollPosLeft.y = targetY - viewHeight + RowHeight;
    }
    
    private void ScrollToRightIndex(int index)
    {
         float targetY = index * RowHeight;
         float viewHeight = position.height;
         if (targetY < _scrollPosRight.y) _scrollPosRight.y = targetY;
         else if (targetY > _scrollPosRight.y + viewHeight - RowHeight) _scrollPosRight.y = targetY - viewHeight + RowHeight;
    }
}

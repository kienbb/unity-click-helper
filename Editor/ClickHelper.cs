using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class ClickHelper
{
    private static GameObject _highlightedObject;

    static ClickHelper()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
        ClickHelperPopup.OnHighlightRequested -= SetHighlight;
        ClickHelperPopup.OnHighlightRequested += SetHighlight;
        ClickHelperPopup.OnHighlightCleared -= ClearHighlight;
        ClickHelperPopup.OnHighlightCleared += ClearHighlight;
    }

    private static void SetHighlight(GameObject go)
    {
        _highlightedObject = go;
        SceneView.RepaintAll();
    }

    private static void ClearHighlight()
    {
        _highlightedObject = null;
        SceneView.RepaintAll();
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!ClickHelperSettings.Enabled)
            return;

        DrawHighlight();

        Event e = Event.current;

        if (e.type != EventType.MouseDown)
            return;

        if (!MatchesActivation(e))
            return;

        e.Use();

        Vector2 mousePos = e.mousePosition;
        List<GameObject> hits = PickAllAtPosition(mousePos);

        if (hits.Count == 0)
            return;

        if (hits.Count == 1)
        {
            Selection.activeGameObject = hits[0];
            EditorGUIUtility.PingObject(hits[0]);
            if (ClickHelperSettings.Behavior == SelectionBehavior.SelectAndFocus)
                SceneView.lastActiveSceneView?.FrameSelected();
            return;
        }

        Vector2 screenPos = GUIUtility.GUIToScreenPoint(mousePos);
        ClickHelperPopup.Show(screenPos, hits);
    }

    private static void DrawHighlight()
    {
        if (_highlightedObject == null)
            return;

        Color color = ClickHelperSettings.HighlightColor;
        Bounds bounds = GetObjectBounds(_highlightedObject);

        Handles.color = color;
        Handles.DrawWireCube(bounds.center, bounds.size);

        Color fillColor = color;
        fillColor.a *= 0.12f;
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        DrawFilledBox(bounds, fillColor);
    }

    private static void DrawFilledBox(Bounds bounds, Color color)
    {
        Vector3 c = bounds.center;
        Vector3 e = bounds.extents;

        Vector3[] face = new Vector3[4];
        Handles.color = color;

        face[0] = c + new Vector3(-e.x, -e.y, -e.z);
        face[1] = c + new Vector3(-e.x, e.y, -e.z);
        face[2] = c + new Vector3(e.x, e.y, -e.z);
        face[3] = c + new Vector3(e.x, -e.y, -e.z);
        Handles.DrawSolidRectangleWithOutline(face, color, Color.clear);

        face[0] = c + new Vector3(-e.x, -e.y, e.z);
        face[1] = c + new Vector3(-e.x, e.y, e.z);
        face[2] = c + new Vector3(e.x, e.y, e.z);
        face[3] = c + new Vector3(e.x, -e.y, e.z);
        Handles.DrawSolidRectangleWithOutline(face, color, Color.clear);

        face[0] = c + new Vector3(-e.x, e.y, -e.z);
        face[1] = c + new Vector3(-e.x, e.y, e.z);
        face[2] = c + new Vector3(e.x, e.y, e.z);
        face[3] = c + new Vector3(e.x, e.y, -e.z);
        Handles.DrawSolidRectangleWithOutline(face, color, Color.clear);

        face[0] = c + new Vector3(-e.x, -e.y, -e.z);
        face[1] = c + new Vector3(-e.x, -e.y, e.z);
        face[2] = c + new Vector3(e.x, -e.y, e.z);
        face[3] = c + new Vector3(e.x, -e.y, -e.z);
        Handles.DrawSolidRectangleWithOutline(face, color, Color.clear);

        face[0] = c + new Vector3(-e.x, -e.y, -e.z);
        face[1] = c + new Vector3(-e.x, -e.y, e.z);
        face[2] = c + new Vector3(-e.x, e.y, e.z);
        face[3] = c + new Vector3(-e.x, e.y, -e.z);
        Handles.DrawSolidRectangleWithOutline(face, color, Color.clear);

        face[0] = c + new Vector3(e.x, -e.y, -e.z);
        face[1] = c + new Vector3(e.x, -e.y, e.z);
        face[2] = c + new Vector3(e.x, e.y, e.z);
        face[3] = c + new Vector3(e.x, e.y, -e.z);
        Handles.DrawSolidRectangleWithOutline(face, color, Color.clear);
    }

    private static Bounds GetObjectBounds(GameObject go)
    {
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
            return renderer.bounds;

        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            Bounds b = new Bounds(corners[0], Vector3.zero);
            for (int i = 1; i < 4; i++)
                b.Encapsulate(corners[i]);
            return b;
        }

        Collider col = go.GetComponent<Collider>();
        if (col != null)
            return col.bounds;

        Collider2D col2D = go.GetComponent<Collider2D>();
        if (col2D != null)
            return col2D.bounds;

        return new Bounds(go.transform.position, Vector3.one * 0.5f);
    }

    private static List<GameObject> PickAllAtPosition(Vector2 position)
    {
        HashSet<int> addedIds = new HashSet<int>();
        List<GameObject> results = new List<GameObject>();
        int excludedLayers = ClickHelperSettings.ExcludedLayers;

        List<GameObject> ignore = new List<GameObject>();
        int maxIterations = 100;
        for (int i = 0; i < maxIterations; i++)
        {
            GameObject picked = HandleUtility.PickGameObject(
                position, true, ignore.ToArray());

            if (picked == null)
                break;

            if (addedIds.Contains(picked.GetInstanceID()))
                break;

            ignore.Add(picked);

            if (IsLayerExcluded(picked.layer, excludedLayers))
                continue;

            addedIds.Add(picked.GetInstanceID());
            results.Add(picked);
        }

        PickUIElementsAtPosition(position, results, addedIds, excludedLayers);

        return results;
    }

    private static void PickUIElementsAtPosition(Vector2 guiPosition,
        List<GameObject> results, HashSet<int> addedIds, int excludedLayers)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(guiPosition);

        Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
        List<(GameObject go, float dist)> uiHits = new List<(GameObject, float)>();

        foreach (Canvas canvas in canvases)
        {
            if (canvas == null || !canvas.gameObject.activeInHierarchy)
                continue;

            Graphic[] graphics = canvas.GetComponentsInChildren<Graphic>(false);
            foreach (Graphic graphic in graphics)
            {
                if (graphic == null || !graphic.gameObject.activeInHierarchy)
                    continue;

                GameObject go = graphic.gameObject;
                if (addedIds.Contains(go.GetInstanceID()))
                    continue;

                if (IsLayerExcluded(go.layer, excludedLayers))
                    continue;

                RectTransform rt = graphic.rectTransform;
                if (RectTransformIntersectsRay(rt, ray, out float distance))
                {
                    uiHits.Add((go, distance));
                    addedIds.Add(go.GetInstanceID());
                }
            }
        }

        uiHits.Sort((a, b) => a.dist.CompareTo(b.dist));
        foreach (var hit in uiHits)
            results.Add(hit.go);
    }

    private static bool RectTransformIntersectsRay(RectTransform rt, Ray ray, out float distance)
    {
        distance = float.MaxValue;
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        Vector3 normal = Vector3.Cross(corners[1] - corners[0], corners[3] - corners[0]).normalized;
        if (normal.sqrMagnitude < 0.0001f)
            return false;

        Plane plane = new Plane(normal, corners[0]);
        if (!plane.Raycast(ray, out float enter))
            return false;

        Vector3 hitPoint = ray.GetPoint(enter);

        Vector3 edge0 = corners[1] - corners[0];
        Vector3 edge1 = corners[3] - corners[0];
        Vector3 local = hitPoint - corners[0];

        float dot00 = Vector3.Dot(edge0, edge0);
        float dot01 = Vector3.Dot(edge0, edge1);
        float dot0L = Vector3.Dot(edge0, local);
        float dot11 = Vector3.Dot(edge1, edge1);
        float dot1L = Vector3.Dot(edge1, local);

        float invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot0L - dot01 * dot1L) * invDenom;
        float v = (dot00 * dot1L - dot01 * dot0L) * invDenom;

        if (u >= 0f && u <= 1f && v >= 0f && v <= 1f)
        {
            distance = enter;
            return true;
        }

        return false;
    }

    private static bool IsLayerExcluded(int objectLayer, int excludedMask)
    {
        if (excludedMask == 0)
            return false;

        string[] layerNames = UnityEditorInternal.InternalEditorUtility.layers;
        for (int i = 0; i < layerNames.Length; i++)
        {
            if ((excludedMask & (1 << i)) != 0)
            {
                int unityLayer = LayerMask.NameToLayer(layerNames[i]);
                if (unityLayer == objectLayer)
                    return true;
            }
        }

        return false;
    }

    private static bool MatchesActivation(Event e)
    {
        switch (ClickHelperSettings.Activation)
        {
            case ActivationModifier.ShiftRightClick:
                return e.button == 1 && e.shift && !e.alt && !e.control;
            case ActivationModifier.CtrlRightClick:
                return e.button == 1 && (e.control || e.command) && !e.alt;
            case ActivationModifier.RightClick:
                return e.button == 1 && !e.shift && !e.alt && !e.control;
            case ActivationModifier.MiddleClick:
                return e.button == 2 && !e.alt;
            default:
                return false;
        }
    }
}

using UnityEditor;
using UnityEngine;

public enum SelectionBehavior
{
    SelectOnly,
    SelectAndFocus
}

public enum ActivationModifier
{
    ShiftRightClick,
    CtrlRightClick,
    RightClick,
    MiddleClick
}

public static class ClickHelperSettings
{
    private const string PrefPrefix = "ClickHelper_";

    public static bool Enabled
    {
        get => EditorPrefs.GetBool(PrefPrefix + "Enabled", true);
        set => EditorPrefs.SetBool(PrefPrefix + "Enabled", value);
    }

    public static SelectionBehavior Behavior
    {
        get => (SelectionBehavior)EditorPrefs.GetInt(PrefPrefix + "Behavior", 0);
        set => EditorPrefs.SetInt(PrefPrefix + "Behavior", (int)value);
    }

    public static Color HighlightColor
    {
        get
        {
            float r = EditorPrefs.GetFloat(PrefPrefix + "HighlightR", 0f);
            float g = EditorPrefs.GetFloat(PrefPrefix + "HighlightG", 1f);
            float b = EditorPrefs.GetFloat(PrefPrefix + "HighlightB", 1f);
            float a = EditorPrefs.GetFloat(PrefPrefix + "HighlightA", 1f);
            return new Color(r, g, b, a);
        }
        set
        {
            EditorPrefs.SetFloat(PrefPrefix + "HighlightR", value.r);
            EditorPrefs.SetFloat(PrefPrefix + "HighlightG", value.g);
            EditorPrefs.SetFloat(PrefPrefix + "HighlightB", value.b);
            EditorPrefs.SetFloat(PrefPrefix + "HighlightA", value.a);
        }
    }

    public static int ExcludedLayers
    {
        get => EditorPrefs.GetInt(PrefPrefix + "ExcludedLayers", 0);
        set => EditorPrefs.SetInt(PrefPrefix + "ExcludedLayers", value);
    }

    public static ActivationModifier Activation
    {
        get => (ActivationModifier)EditorPrefs.GetInt(PrefPrefix + "Activation", 0);
        set => EditorPrefs.SetInt(PrefPrefix + "Activation", (int)value);
    }

    public static bool ShowComponents
    {
        get => EditorPrefs.GetBool(PrefPrefix + "ShowComponents", true);
        set => EditorPrefs.SetBool(PrefPrefix + "ShowComponents", value);
    }
}

using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), "Click Helper", true)]
[Icon("d_Linked")]
public class ClickHelperOverlay : Overlay, ITransientOverlay
{
    public bool visible => true;

    public override VisualElement CreatePanelContent()
    {
        var root = new VisualElement();
        root.style.flexDirection = FlexDirection.Row;
        root.style.alignItems = Align.Center;

        var toggle = new Button(() =>
        {
            ClickHelperSettings.Enabled = !ClickHelperSettings.Enabled;
        });
        toggle.text = "Click Helper";
        toggle.style.unityFontStyleAndWeight = FontStyle.Bold;
        toggle.style.fontSize = 11;
        toggle.RegisterCallback<AttachToPanelEvent>(_ => UpdateToggleStyle(toggle));
        toggle.schedule.Execute(() => UpdateToggleStyle(toggle)).Every(200);

        var settingsBtn = new Button(() =>
        {
            var menu = new GenericMenu();
            BuildSettingsMenu(menu);
            menu.ShowAsContext();
        });
        settingsBtn.text = "\u25BC";
        settingsBtn.style.fontSize = 9;
        settingsBtn.style.width = 20;
        settingsBtn.style.paddingLeft = 2;
        settingsBtn.style.paddingRight = 2;

        root.Add(toggle);
        root.Add(settingsBtn);
        return root;
    }

    private void UpdateToggleStyle(Button toggle)
    {
        bool enabled = ClickHelperSettings.Enabled;
        toggle.style.opacity = enabled ? 1f : 0.5f;
        toggle.text = enabled ? "\u2714 Click Helper" : "Click Helper";
    }

    private void BuildSettingsMenu(GenericMenu menu)
    {
        bool enabled = ClickHelperSettings.Enabled;
        menu.AddItem(new GUIContent("Enabled"), enabled, () =>
        {
            ClickHelperSettings.Enabled = !enabled;
        });

        menu.AddSeparator("");

        foreach (ActivationModifier mod in System.Enum.GetValues(typeof(ActivationModifier)))
        {
            var m = mod;
            string label = "Activation/" + FormatModifierName(m);
            menu.AddItem(new GUIContent(label),
                ClickHelperSettings.Activation == m,
                () => ClickHelperSettings.Activation = m);
        }

        menu.AddSeparator("");

        foreach (SelectionBehavior beh in System.Enum.GetValues(typeof(SelectionBehavior)))
        {
            var b = beh;
            string label = "Selection Behavior/" + ObjectNames.NicifyVariableName(b.ToString());
            menu.AddItem(new GUIContent(label),
                ClickHelperSettings.Behavior == b,
                () => ClickHelperSettings.Behavior = b);
        }

        menu.AddSeparator("");

        menu.AddItem(new GUIContent("Show Components"), ClickHelperSettings.ShowComponents, () =>
        {
            ClickHelperSettings.ShowComponents = !ClickHelperSettings.ShowComponents;
        });

        menu.AddSeparator("");

        string[] layerNames = UnityEditorInternal.InternalEditorUtility.layers;
        int excluded = ClickHelperSettings.ExcludedLayers;
        for (int i = 0; i < layerNames.Length; i++)
        {
            int bit = 1 << i;
            bool isExcluded = (excluded & bit) != 0;
            int idx = i;
            menu.AddItem(new GUIContent("Excluded Layers/" + layerNames[idx]), isExcluded, () =>
            {
                ClickHelperSettings.ExcludedLayers = excluded ^ (1 << idx);
            });
        }
    }

    private string FormatModifierName(ActivationModifier mod)
    {
        switch (mod)
        {
            case ActivationModifier.ShiftRightClick: return "Shift + Right Click";
            case ActivationModifier.CtrlRightClick: return "Ctrl + Right Click";
            case ActivationModifier.RightClick: return "Right Click";
            case ActivationModifier.MiddleClick: return "Middle Click";
            default: return mod.ToString();
        }
    }
}

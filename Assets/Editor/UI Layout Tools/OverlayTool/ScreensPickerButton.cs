// Assets/Editor/UIOverlay/ScreensPickerButton.cs

using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace Editor.RedesignTools.OverlayTool
{
    [EditorToolbarElement(ID, typeof(SceneView))]
    public class ScreensPickerButton : EditorToolbarButton
    {
        public const string ID = "UIOverlay/ScreensPicker";

        public ScreensPickerButton()
        {
            text = CurrentLabel();
            tooltip = "Pick overlay image (with preview)";
            clicked += OnClick;
        }

        private void OnClick()
        {
            var rect = new Rect(Event.current.mousePosition, Vector2.zero);
            PopupWindow.Show(rect, new ScreensImagesPopup(this));
        }

        internal void RefreshLabel() => text = CurrentLabel();

        private static string CurrentLabel()
        {
            int i = UIOverlayImageState.CurrentIndex;
            return (i >= 0 && i < UIOverlayImageState.Screens.Count)
                ? $"Screens ({i + 1})"
                : "Screens";
        }
    }
}
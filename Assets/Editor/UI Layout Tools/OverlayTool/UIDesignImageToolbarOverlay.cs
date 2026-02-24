// Assets/Editor/UIOverlay/UIDesignImageToolbarOverlay.cs

using UnityEditor;
using UnityEditor.Overlays;

namespace Editor.RedesignTools.OverlayTool
{
    [Overlay(typeof(SceneView), "UI Design Overlay (Images)")]
    public class UIDesignImageToolbarOverlay : ToolbarOverlay
    {
        public UIDesignImageToolbarOverlay()
            : base(
                ScreensPickerButton.ID,  // попап с превью
                FullButton.ID,
                HalfButton.ID,
                HideButton.ID,
                RefreshButton.ID,
                AttachButton.ID,
                DetachButton.ID
            )
        {
            _ = typeof(UIOverlayImageState); // ensure static init
            _ = typeof(UIOverlayImageDrawer); // ensure drawer is alive
        }
    }
}
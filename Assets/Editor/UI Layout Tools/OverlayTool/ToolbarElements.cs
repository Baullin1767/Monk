// Assets/Editor/UIOverlay/ToolbarElements.cs

using UnityEditor;
using UnityEditor.Toolbars;

namespace Editor.RedesignTools.OverlayTool
{
    [EditorToolbarElement(ID, typeof(SceneView))]
    public class FullButton : EditorToolbarButton
    {
        public const string ID = "UIOverlay/Full";
        public FullButton()
        {
            text = "Full";
            tooltip = "Opaque overlay";
            clicked += () =>
            {
                UIOverlayImageState.Alpha = 1f;
                UIOverlayImageState.Visible = true;
                UIOverlayImageState.RepaintAllSceneViews();
            };
        }
    }

    [EditorToolbarElement(ID, typeof(SceneView))]
    public class HalfButton : EditorToolbarButton
    {
        public const string ID = "UIOverlay/Half";
        public HalfButton()
        {
            text = "Half";
            tooltip = "Semi-transparent overlay";
            clicked += () =>
            {
                UIOverlayImageState.Alpha = 0.5f;
                UIOverlayImageState.Visible = true;
                UIOverlayImageState.RepaintAllSceneViews();
            };
        }
    }

    [EditorToolbarElement(ID, typeof(SceneView))]
    public class HideButton : EditorToolbarButton
    {
        public const string ID = "UIOverlay/Hide";
        public HideButton()
        {
            text = "Hide";
            tooltip = "Hide overlay image";
            clicked += () =>
            {
                UIOverlayImageState.Visible = false;
                UIOverlayImageState.RepaintAllSceneViews();
            };
        }
    }

    [EditorToolbarElement(ID, typeof(SceneView))]
    public class RefreshButton : EditorToolbarButton
    {
        public const string ID = "UIOverlay/Refresh";
        public RefreshButton()
        {
            text = "↻";
            tooltip = "Reload images from folder";
            clicked += UIOverlayImageState.RefreshAssets;
        }
    }

    [EditorToolbarElement(ID, typeof(SceneView))]
    public class AttachButton : EditorToolbarButton
    {
        public const string ID = "UIOverlay/Attach";
        public AttachButton()
        {
            text = "Attach";
            tooltip = "Attach overlay to selected RectTransform/Canvas";
            clicked += UIOverlayImageState.AttachToSelection;
        }
    }

    [EditorToolbarElement(ID, typeof(SceneView))]
    public class DetachButton : EditorToolbarButton
    {
        public const string ID = "UIOverlay/Detach";
        public DetachButton()
        {
            text = "Detach";
            tooltip = "Detach overlay from target";
            clicked += UIOverlayImageState.Detach;
        }
    }
}

// Assets/Editor/UIOverlay/UIOverlayImageDrawer.cs

using UnityEditor;
using UnityEngine;

namespace Editor.RedesignTools.OverlayTool
{
    [InitializeOnLoad]
    internal static class UIOverlayImageDrawer
    {
        static UIOverlayImageDrawer()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sv)
        {
            if (!UIOverlayImageState.Visible) return;
            int idx = UIOverlayImageState.CurrentIndex;
            if (idx < 0 || idx >= UIOverlayImageState.Screens.Count) return;

            var tex = UIOverlayImageState.Screens[idx];
            if (!tex) return;

            Handles.BeginGUI();
            var prev = GUI.color;
            GUI.color = new Color(1, 1, 1, UIOverlayImageState.Alpha);

            if (UIOverlayImageState.Target != null)
            {
                Vector3[] wc = new Vector3[4];
                UIOverlayImageState.Target.GetWorldCorners(wc);

                var p0 = HandleUtility.WorldToGUIPoint(wc[0]);
                var p1 = HandleUtility.WorldToGUIPoint(wc[1]);
                var p2 = HandleUtility.WorldToGUIPoint(wc[2]);
                var p3 = HandleUtility.WorldToGUIPoint(wc[3]);

                float xMin = Mathf.Min(p0.x, p1.x, p2.x, p3.x);
                float xMax = Mathf.Max(p0.x, p1.x, p2.x, p3.x);
                float yMin = Mathf.Min(p0.y, p1.y, p2.y, p3.y);
                float yMax = Mathf.Max(p0.y, p1.y, p2.y, p3.y);

                var rect = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
                var imgRect = FitRectKeepAspect(new Rect(0, 0, tex.width, tex.height), rect);
                GUI.DrawTexture(imgRect, tex, ScaleMode.ScaleToFit, true);
            }
            else
            {
                var viewRect = sv.position;
                var drawRect = new Rect(0, 0, viewRect.width, viewRect.height);
                var imgRect = FitRectKeepAspect(new Rect(0, 0, tex.width, tex.height), drawRect);
                GUI.DrawTexture(imgRect, tex, ScaleMode.ScaleToFit, true);
            }

            GUI.color = prev;
            Handles.EndGUI();
        }

        private static Rect FitRectKeepAspect(Rect src, Rect dst)
        {
            float srcRatio = src.width / src.height;
            float dstRatio = dst.width / dst.height;

            Rect r = dst;
            if (srcRatio > dstRatio)
            {
                float h = dst.width / srcRatio;
                r.y += (dst.height - h) * 0.5f;
                r.height = h;
            }
            else
            {
                float w = dst.height * srcRatio;
                r.x += (dst.width - w) * 0.5f;
                r.width = w;
            }
            return r;
        }
    }
}

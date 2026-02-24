// Assets/Editor/UIOverlay/ScreensImagesPopup.cs

using UnityEditor;
using UnityEngine;

namespace Editor.RedesignTools.OverlayTool
{
    public class ScreensImagesPopup : PopupWindowContent
    {
        private Vector2 _scroll;
        private readonly ScreensPickerButton _host;

        const float kThumb = 64f;
        const float kRowH = 72f;

        public ScreensImagesPopup(ScreensPickerButton host) => _host = host;

        public override Vector2 GetWindowSize() => new Vector2(360, 460);

        public override void OnGUI(Rect rect)
        {
            var list = UIOverlayImageState.Screens;

            if (list.Count == 0)
            {
                GUILayout.Label("No images in " + UIOverlayImageState.ImagesFolder,
                    EditorStyles.centeredGreyMiniLabel, GUILayout.Height(24));

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Open folder…", GUILayout.Height(24)))
                        EditorUtility.RevealInFinder(UIOverlayImageState.ImagesFolder);

                    if (GUILayout.Button("Set folder…", GUILayout.Height(24)))
                        UIOverlayImageState.PickImagesFolderViaDialog();
                }
                return;
            }

            using (var scroll = new GUILayout.ScrollViewScope(_scroll))
            {
                _scroll = scroll.scrollPosition;

                for (int i = 0; i < list.Count; i++)
                {
                    var tex = list[i];
                    if (!tex) continue;

                    var rowRect = GUILayoutUtility.GetRect(1, kRowH, GUILayout.ExpandWidth(true));
                    DrawRow(rowRect, tex, i, i == UIOverlayImageState.CurrentIndex);
                }
            }
        }

        private void DrawRow(Rect r, Texture2D tex, int index, bool isCurrent)
        {
            if (isCurrent)
            {
                var sel = new Color(0.24f, 0.48f, 0.90f, 0.2f);
                EditorGUI.DrawRect(r, sel);
            }

            var thumbRect = new Rect(r.x + 6, r.y + (r.height - kThumb) * 0.5f, kThumb, kThumb);
            GUI.DrawTexture(thumbRect, tex, ScaleMode.ScaleToFit, true);

            var labelRect = new Rect(thumbRect.xMax + 8, r.y + 6, r.width - thumbRect.width - 90, 20);
            EditorGUI.LabelField(labelRect, tex.name, EditorStyles.boldLabel);

            var subRect = new Rect(labelRect.x, labelRect.yMax + 2, labelRect.width, 16);
            EditorGUI.LabelField(subRect, $"Screen {index}", EditorStyles.miniLabel);

            var btnRect = new Rect(r.xMax - 70, r.y + (r.height - 24) * 0.5f, 64, 24);
            if (GUI.Button(btnRect, isCurrent ? "Selected" : "Choose"))
            {
                Select(index);
            }

            if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
            {
                Select(index);
                Event.current.Use();
            }
        }

        private void Select(int idx)
        {
            UIOverlayImageState.CurrentIndex = idx;
            UIOverlayImageState.RepaintAllSceneViews();
            _host.RefreshLabel();
            editorWindow.Close();
        }
    }
}

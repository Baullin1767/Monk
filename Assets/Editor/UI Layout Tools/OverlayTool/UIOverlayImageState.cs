// Assets/Editor/UIOverlay/UIOverlayImageState.cs

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor.RedesignTools.OverlayTool
{
    internal static class UIOverlayImageState
    {
        public static readonly System.Collections.Generic.List<Texture2D> Screens = new();

        public static int CurrentIndex
        {
            get => UIOverlayPrefs.instance.currentIndex;
            set { UIOverlayPrefs.instance.currentIndex = value; UIOverlayPrefs.instance.SaveNow(); RepaintAllSceneViews(); }
        }

        public static float Alpha
        {
            get => UIOverlayPrefs.instance.alpha;
            set { UIOverlayPrefs.instance.alpha = value; UIOverlayPrefs.instance.SaveNow(); RepaintAllSceneViews(); }
        }

        public static bool Visible
        {
            get => UIOverlayPrefs.instance.visible;
            set { UIOverlayPrefs.instance.visible = value; UIOverlayPrefs.instance.SaveNow(); RepaintAllSceneViews(); }
        }

        public static RectTransform Target { get; private set; }

        public static string ImagesFolder
        {
            get => UIOverlayPrefs.instance.imagesFolder;
            set { UIOverlayPrefs.instance.imagesFolder = value; UIOverlayPrefs.instance.SaveNow(); }
        }

        static UIOverlayImageState()
        {
            // первичная загрузка ассетов
            RefreshAssets();

            // события проекта/сцен/префаб стейджа
            EditorApplication.projectChanged += RefreshAssets;
            EditorSceneManager.sceneOpened += (_, __) => TryRestoreTarget();
            UnityEditor.SceneManagement.PrefabStage.prefabStageOpened += _ => TryRestoreTarget();
            UnityEditor.SceneManagement.PrefabStage.prefabStageClosing += _ => { Target = null; };

            // после перезагрузки домена тоже попробуем восстановить
            TryRestoreTarget();
        }

        public static void RefreshAssets()
        {
            Screens.Clear();

            if (!AssetDatabase.IsValidFolder(ImagesFolder))
                ImagesFolder = UIOverlayPrefs.instance.imagesFolder; // оставим как есть, если путь невалиден

            if (AssetDatabase.IsValidFolder(ImagesFolder))
            {
                var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { ImagesFolder });
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    if (tex != null) Screens.Add(tex);
                }
            }

            if (Screens.Count > 0 && (CurrentIndex < 0 || CurrentIndex >= Screens.Count))
                CurrentIndex = 0;

            RepaintAllSceneViews();
        }

        public static void RepaintAllSceneViews()
        {
            foreach (SceneView sv in SceneView.sceneViews)
                sv.Repaint();
        }

        // -------------------- Привязка --------------------

        public static void AttachToSelection()
        {
            var tr = Selection.activeTransform;
            if (!tr)
            {
                EditorUtility.DisplayDialog("Attach overlay", "Select a RectTransform or Canvas in Hierarchy.", "OK");
                return;
            }

            var rt = tr.GetComponentInParent<RectTransform>(true);
            if (!rt)
            {
                var canvas = tr.GetComponentInParent<Canvas>(true);
                if (canvas) rt = canvas.GetComponent<RectTransform>();
            }

            if (!rt)
            {
                EditorUtility.DisplayDialog("Attach overlay", "No RectTransform/Canvas found on selection.", "OK");
                return;
            }

            // Определяем: это сцена или prefab stage?
            var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.prefabContentsRoot && rt.transform.IsChildOf(stage.prefabContentsRoot.transform))
            {
                // Prefab Stage: запомним путь и ассет
                UIOverlayPrefs.instance.attachMode = UIOverlayPrefs.AttachMode.PrefabObject;
                UIOverlayPrefs.instance.prefabAssetPath = stage.assetPath;
                UIOverlayPrefs.instance.prefabTransformPath = GetTransformPath(rt.transform, stage.prefabContentsRoot.transform);
                UIOverlayPrefs.instance.sceneTargetGlobalId = string.Empty;
                UIOverlayPrefs.instance.SaveNow();
                Target = rt;
            }
            else
            {
                // Scene Object: запомним GlobalObjectId (переживает доменные reload'ы)
                UIOverlayPrefs.instance.attachMode = UIOverlayPrefs.AttachMode.SceneObject;
                var gid = GlobalObjectId.GetGlobalObjectIdSlow(rt);
                UIOverlayPrefs.instance.sceneTargetGlobalId = gid.ToString();
                UIOverlayPrefs.instance.prefabAssetPath = string.Empty;
                UIOverlayPrefs.instance.prefabTransformPath = string.Empty;
                UIOverlayPrefs.instance.SaveNow();
                Target = rt;
            }

            RepaintAllSceneViews();
        }

        public static void Detach()
        {
            Target = null;
            UIOverlayPrefs.instance.attachMode = UIOverlayPrefs.AttachMode.None;
            UIOverlayPrefs.instance.sceneTargetGlobalId = string.Empty;
            UIOverlayPrefs.instance.prefabAssetPath = string.Empty;
            UIOverlayPrefs.instance.prefabTransformPath = string.Empty;
            UIOverlayPrefs.instance.SaveNow();
            RepaintAllSceneViews();
        }

        public static void TryRestoreTarget()
        {
            Target = null;

            switch (UIOverlayPrefs.instance.attachMode)
            {
                case UIOverlayPrefs.AttachMode.SceneObject:
                {
                    if (!string.IsNullOrEmpty(UIOverlayPrefs.instance.sceneTargetGlobalId))
                    {
                        if (GlobalObjectId.TryParse(UIOverlayPrefs.instance.sceneTargetGlobalId, out var gid))
                        {
                            var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(gid) as RectTransform;
                            if (obj != null) Target = obj;
                        }
                    }
                    break;
                }

                case UIOverlayPrefs.AttachMode.PrefabObject:
                {
                    var assetPath = UIOverlayPrefs.instance.prefabAssetPath;
                    var tPath = UIOverlayPrefs.instance.prefabTransformPath;
                    if (!string.IsNullOrEmpty(assetPath) && !string.IsNullOrEmpty(tPath))
                    {
                        var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                        if (stage != null && stage.assetPath == assetPath && stage.prefabContentsRoot)
                        {
                            var found = FindByPath(stage.prefabContentsRoot.transform, tPath);
                            if (found) Target = found.GetComponent<RectTransform>();
                        }
                    }
                    break;
                }
            }

            RepaintAllSceneViews();
        }

        // helpers
        private static string GetTransformPath(Transform t, Transform root)
        {
            var stack = new System.Collections.Generic.List<string>();
            var cur = t;
            while (cur != null && cur != root)
            {
                stack.Add(cur.name);
                cur = cur.parent;
            }
            stack.Reverse();
            return string.Join("/", stack);
        }

        private static Transform FindByPath(Transform root, string path)
        {
            var parts = path.Split('/');
            var cur = root;
            foreach (var p in parts)
            {
                var child = cur.Find(p);
                if (!child) return null;
                cur = child;
            }
            return cur;
        }
        public static void PickImagesFolderViaDialog()
        {
            var abs = EditorUtility.OpenFolderPanel("Select overlay images folder", Application.dataPath, "");
            if (string.IsNullOrEmpty(abs)) return;

            // переведём абсолютный путь в вид "Assets/..."
            var dataPath = Application.dataPath.Replace('\\', '/');
            abs = abs.Replace('\\', '/');
            if (!abs.StartsWith(dataPath))
            {
                EditorUtility.DisplayDialog("Wrong folder",
                    "Please select a folder inside the project (under Assets).", "OK");
                return;
            }

            string assetsRel = "Assets" + abs.Substring(dataPath.Length);
            ImagesFolder = assetsRel;
            RefreshAssets();
        }
    }
}

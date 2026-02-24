using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Editor
{
    public class LayoutComponentsAdderWindow : EditorWindow
    {
        [MenuItem("Tools/Layout Components Adder")]
        public static void ShowWindow()
        {
            GetWindow<LayoutComponentsAdderWindow>("Add Layout Components");
        }

        private void OnGUI()
        {
            GameObject[] gos = Selection.gameObjects;

            if (gos is {Length: 0})
            {
                EditorGUILayout.HelpBox("Выберите объект в иерархии", MessageType.Info);
                return;
            }

            if(gos.Length == 1)
                EditorGUILayout.LabelField("объект: " + gos[0].name, EditorStyles.boldLabel);
            else
              EditorGUILayout.LabelField("Много объектов.", EditorStyles.boldLabel);
            
            
            
            EditorGUILayout.Space();
            DrawButton("Create empty GO", () =>
            {
                var v = new GameObject("Obj");
                v.transform.parent = gos[0].transform;
                v.AddComponent<RectTransform>();
                v.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                v.transform.localScale = Vector3.one;
                Selection.activeGameObject = v;
            });
            DrawButton("Create Image", () =>
            {
                var v = new GameObject("Image");
                v.transform.parent = gos[0].transform;
                v.AddComponent<RectTransform>();
                v.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                v.transform.localScale = Vector3.one;
                v.AddComponent<Image>();
                Selection.activeGameObject = v;
            });
            DrawButton("Create text", () =>
            {
                var v = new GameObject("Text");
                v.transform.parent = gos[0].transform;
                v.AddComponent<RectTransform>();
                v.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                v.transform.localScale = Vector3.one;
                v.AddComponent<TextMeshProUGUI>();
                v.GetComponent<TextMeshProUGUI>().enableAutoSizing = true;
                Selection.activeGameObject = v;
            });
            
            DrawButton("Create button", () =>
            {
                var v = new GameObject("Text");
                v.transform.parent = gos[0].transform;
                v.AddComponent<RectTransform>();
                v.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                v.transform.localScale = Vector3.one;
                v.AddComponent<Button>();
                Selection.activeGameObject = v;
            });
            
            DrawButton("Create toggle", () =>
            {
                var v = new GameObject("Text");
                v.transform.parent = gos[0].transform;
                v.AddComponent<RectTransform>();
                v.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                v.transform.localScale = Vector3.one;
                v.AddComponent<Toggle>();
                Selection.activeGameObject = v;
            });

            // EditorGUILayout.BeginHorizontal();

            // Левая колонка
            // EditorGUILayout.BeginVertical();
            
            EditorGUILayout.LabelField("Layout", EditorStyles.boldLabel);
            DrawButton("GridLG", () => AddComponent<GridLayoutGroup>(gos));
            DrawButton("LayoutElement", () => AddComponent<LayoutElement>(gos));
            DrawButton("Left-RightLG", () => AddComponent<HorizontalLayoutGroup>(gos));
            DrawButton("Up-DownLG", () => AddComponent<VerticalLayoutGroup>(gos));
            DrawButton("ContentSizeFitter", () => AddComponent<ContentSizeFitter>(gos));
            EditorGUILayout.LabelField("UI - visual", EditorStyles.boldLabel);
            DrawButton("Image", () => AddComponent<Image>(gos));
            DrawButton("Mask", () => AddComponent<Mask>(gos));
            DrawButton("Shadow", () => { 
                AddComponent<Shadow>(gos);
                gos[0].GetComponent<Shadow>().effectDistance = new Vector2(-2f, -5f); 
            });
            EditorGUILayout.LabelField("UI - actions", EditorStyles.boldLabel);
            DrawButton("Button", () => AddComponent<Button>(gos));
            DrawButton("Toggle", () => AddComponent<Toggle>(gos));
            DrawButton("ScrollRect", () => AddComponent<ScrollRect>(gos));
            EditorGUILayout.LabelField("Дополнительные компоненты", EditorStyles.boldLabel);
            
            // EditorGUILayout.EndVertical();
            //
            // Правая колонка
            // EditorGUILayout.BeginVertical();
            // DrawButton("Add Image", () => AddComponent<Image>(gos));
            // DrawButton("Add Mask", () => AddComponent<Mask>(gos));
            // DrawButton("Add ScrollRect", () => AddComponent<ScrollRect>(gos));
            // DrawButton("Add ContentSizeFitter", () => AddComponent<ContentSizeFitter>(gos));
            // DrawButton("Add UITextSmoothPulse", () => AddComponent<UITextSmoothPulse>(gos));
            // EditorGUILayout.EndVertical();
            //
            // EditorGUILayout.EndHorizontal();
        }

        private void DrawButton(string label, System.Action onClick)
        {
            if (GUILayout.Button(label))
                onClick?.Invoke();
        }

        private void AddComponent<T>(GameObject[] gos) where T : Component
        {
            foreach (var go in gos)
            {
                if (go.GetComponent<T>() == null)
                    go.AddComponent<T>();
                else
                    Debug.Log(typeof(T).Name + " уже добавлен");
            }
        }
    }
}

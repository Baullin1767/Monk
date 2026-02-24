// Assets/Editor/RedesignTools/OverlayTool/UIOverlayPrefs.cs
using UnityEditor;

namespace Editor.RedesignTools.OverlayTool
{
    // Файл настроек будет лежать: ProjectSettings/UIOverlayPrefs.asset
    [FilePath("ProjectSettings/UIOverlayPrefs.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class UIOverlayPrefs : ScriptableSingleton<UIOverlayPrefs>
    {
        // базовые настройки
        public string imagesFolder = "Assets/2D/Template";
        public int currentIndex = -1;
        public float alpha = 0.5f;
        public bool visible = true;

        // режим привязки
        public enum AttachMode { None, SceneObject, PrefabObject }
        public AttachMode attachMode = AttachMode.None;

        // SceneObject — глобальный идентификатор
        public string sceneTargetGlobalId = string.Empty;

        // PrefabObject — путь к ассету и путь трансформа внутри него
        public string prefabAssetPath = string.Empty;
        public string prefabTransformPath = string.Empty;

        public void SaveNow() => Save(true);
    }
}
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Monk.Presentation.Editor
{
    public static class LevelTilemapImportEditor
    {
        [MenuItem("Tools/Monk/Setup/Import 2dAssetPack Tilemap Level")]
        public static void Import2dAssetPackLevel()
        {
            if (!AssetDatabase.IsValidFolder("Assets/2dAssetPack"))
            {
                Debug.LogError("Cannot import 2dAssetPack level because Assets/2dAssetPack is missing.");
                return;
            }

            TwoDAssetPackMigrationEditor.MigratePresetA();
            BaseTilemapLevelBuilderEditor.BuildBaseLevel();

            Debug.Log("Imported 2dAssetPack tilemap visuals into Level 1 while preserving gameplay layout.");
        }
    }
}
#endif

#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Monk.Presentation.Editor
{
public static class TufeiCopyPrefabMigrationEditor
    {
        private const string SourcePrefabPath = "Assets/_Project/Prefabs/Enemies/Tufei.prefab";
        private const string CopyPrefabPath = "Assets/_Project/Prefabs/Enemies/Tufei_Copy.prefab";
        private const string CopyObjectName = "Tufei_Copy";
        private const string ScenesRoot = "Assets/_Project/Scenes";

        [MenuItem("Tools/Monk/Setup/Create Tufei_Copy Prefab And Reconnect Scene Copies")]
        public static void CreateCopyAndReconnectSceneCopies()
        {
            var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePrefabPath);
            if (sourcePrefab == null)
            {
                Debug.LogError($"Cannot create copy prefab because source is missing: {SourcePrefabPath}");
                return;
            }

            if (!EnsureCopyPrefabExists())
            {
                Debug.LogError($"Cannot create copy prefab at path: {CopyPrefabPath}");
                return;
            }

            var copyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CopyPrefabPath);
            if (copyPrefab == null)
            {
                Debug.LogError($"Copy prefab could not be loaded: {CopyPrefabPath}");
                return;
            }

            var originalScenePath = SceneManager.GetActiveScene().path;
            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { ScenesRoot });

            var scannedCandidates = 0;
            var reconnectedCandidates = 0;
            var fallbackReconnects = 0;
            var modifiedScenes = 0;

            foreach (var sceneGuid in sceneGuids)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var sceneChanged = false;

                var roots = scene.GetRootGameObjects();
                for (var i = 0; i < roots.Length; i++)
                {
                    sceneChanged |= ReconnectSceneHierarchy(
                        roots[i].transform,
                        copyPrefab,
                        ref scannedCandidates,
                        ref reconnectedCandidates,
                        ref fallbackReconnects);
                }

                if (!sceneChanged)
                {
                    continue;
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                modifiedScenes++;
            }

            if (!string.IsNullOrWhiteSpace(originalScenePath) && File.Exists(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"Tufei copy migration complete. Scenes modified: {modifiedScenes}, " +
                $"candidates checked: {scannedCandidates}, reconnected: {reconnectedCandidates}, " +
                $"fallback reconnects: {fallbackReconnects}.");
        }

        private static bool EnsureCopyPrefabExists()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(CopyPrefabPath) != null)
            {
                return true;
            }

            return AssetDatabase.CopyAsset(SourcePrefabPath, CopyPrefabPath);
        }

        private static bool ReconnectSceneHierarchy(
            Transform current,
            GameObject copyPrefab,
            ref int scannedCandidates,
            ref int reconnectedCandidates,
            ref int fallbackReconnects)
        {
            var changed = false;

            for (var i = 0; i < current.childCount; i++)
            {
                changed |= ReconnectSceneHierarchy(
                    current.GetChild(i),
                    copyPrefab,
                    ref scannedCandidates,
                    ref reconnectedCandidates,
                    ref fallbackReconnects);
            }

            var candidate = current.gameObject;
            if (!string.Equals(candidate.name, CopyObjectName, StringComparison.Ordinal))
            {
                return changed;
            }

            if (PrefabUtility.IsPartOfPrefabInstance(candidate) &&
                PrefabUtility.GetNearestPrefabInstanceRoot(candidate) != candidate)
            {
                return changed;
            }

            scannedCandidates++;

            if (IsAlreadyConnectedToCopyPrefab(candidate))
            {
                return changed;
            }

            if (!TryReconnectObjectToCopyPrefab(candidate, copyPrefab, out var connectedRoot, out var usedFallback))
            {
                return changed;
            }

            connectedRoot.name = CopyObjectName;
            reconnectedCandidates++;

            if (usedFallback)
            {
                fallbackReconnects++;
            }

            return true;
        }

        private static bool IsAlreadyConnectedToCopyPrefab(GameObject instanceRoot)
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(instanceRoot))
            {
                return false;
            }

            var source = PrefabUtility.GetCorrespondingObjectFromSource(instanceRoot);
            var sourcePath = source != null ? AssetDatabase.GetAssetPath(source) : string.Empty;
            return string.Equals(sourcePath, CopyPrefabPath, StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryReconnectObjectToCopyPrefab(
            GameObject instanceRoot,
            GameObject copyPrefab,
            out GameObject connectedRoot,
            out bool usedFallback)
        {
            connectedRoot = instanceRoot;
            usedFallback = false;

            if (PrefabUtility.IsPartOfPrefabInstance(instanceRoot))
            {
                if (TryReplacePrefabAsset(instanceRoot, copyPrefab))
                {
                    return true;
                }
            }
            else
            {
                if (TryConvertToPrefabInstance(instanceRoot, copyPrefab))
                {
                    return true;
                }
            }

            connectedRoot = ReplaceByInstantiate(instanceRoot, copyPrefab);
            usedFallback = connectedRoot != null;
            return connectedRoot != null;
        }

        private static bool TryReplacePrefabAsset(GameObject instanceRoot, GameObject copyPrefab)
        {
            return InvokePrefabUtilityMethod(
                "ReplacePrefabAssetOfPrefabInstance",
                instanceRoot,
                copyPrefab,
                "objectMatchMode",
                "ByHierarchy",
                "prefabOverridesOptions",
                "KeepAllPossibleOverrides");
        }

        private static bool TryConvertToPrefabInstance(GameObject sceneObject, GameObject copyPrefab)
        {
            return InvokePrefabUtilityMethod(
                "ConvertToPrefabInstance",
                sceneObject,
                copyPrefab,
                "objectMatchMode",
                "ByHierarchy",
                "prefabOverridesOptions",
                "KeepAllPossibleOverrides");
        }

        private static bool InvokePrefabUtilityMethod(
            string methodName,
            GameObject targetObject,
            GameObject prefabAsset,
            string firstEnumMemberName,
            string firstEnumValueName,
            string secondEnumMemberName,
            string secondEnumValueName)
        {
            var candidateMethods = typeof(PrefabUtility)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => method.Name == methodName)
                .OrderByDescending(method => method.GetParameters().Length)
                .ToArray();

            for (var i = 0; i < candidateMethods.Length; i++)
            {
                var method = candidateMethods[i];
                var parameters = method.GetParameters();
                if (parameters.Length < 3 ||
                    parameters[0].ParameterType != typeof(GameObject) ||
                    parameters[1].ParameterType != typeof(GameObject))
                {
                    continue;
                }

                try
                {
                    if (parameters.Length == 3 && parameters[2].ParameterType == typeof(InteractionMode))
                    {
                        method.Invoke(null, new object[] { targetObject, prefabAsset, InteractionMode.AutomatedAction });
                        return true;
                    }

                    if (parameters.Length == 4 && parameters[3].ParameterType == typeof(InteractionMode))
                    {
                        var settings = Activator.CreateInstance(parameters[2].ParameterType);
                        SetEnumMemberIfPresent(settings, firstEnumMemberName, firstEnumValueName);
                        SetEnumMemberIfPresent(settings, secondEnumMemberName, secondEnumValueName);

                        method.Invoke(
                            null,
                            new object[] { targetObject, prefabAsset, settings, InteractionMode.AutomatedAction });
                        return true;
                    }
                }
                catch
                {
                    // Try next overload. Fallback replacement will run if all overloads fail.
                }
            }

            return false;
        }

        private static void SetEnumMemberIfPresent(object target, string memberName, string valueName)
        {
            if (target == null)
            {
                return;
            }

            var targetType = target.GetType();
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

            var property = targetType.GetProperty(memberName, flags);
            if (property != null && property.CanWrite && property.PropertyType.IsEnum)
            {
                var value = Enum.Parse(property.PropertyType, valueName, true);
                property.SetValue(target, value);
                return;
            }

            var field = targetType.GetField(memberName, flags);
            if (field != null && field.FieldType.IsEnum)
            {
                var value = Enum.Parse(field.FieldType, valueName, true);
                field.SetValue(target, value);
            }
        }

        private static GameObject ReplaceByInstantiate(GameObject oldRoot, GameObject copyPrefab)
        {
            var oldTransform = oldRoot.transform;
            var parent = oldTransform.parent;
            var siblingIndex = oldTransform.GetSiblingIndex();

            var localPosition = oldTransform.localPosition;
            var localRotation = oldTransform.localRotation;
            var localScale = oldTransform.localScale;
            var worldPosition = oldTransform.position;
            var worldRotation = oldTransform.rotation;
            var wasActive = oldRoot.activeSelf;
            var scene = oldRoot.scene;

            var replacement = PrefabUtility.InstantiatePrefab(copyPrefab, scene) as GameObject;
            if (replacement == null)
            {
                return null;
            }

            var replacementTransform = replacement.transform;
            if (parent != null)
            {
                replacementTransform.SetParent(parent, false);
                replacementTransform.localPosition = localPosition;
                replacementTransform.localRotation = localRotation;
                replacementTransform.localScale = localScale;
            }
            else
            {
                replacementTransform.SetPositionAndRotation(worldPosition, worldRotation);
                replacementTransform.localScale = localScale;
            }

            replacementTransform.SetSiblingIndex(siblingIndex);
            replacement.SetActive(wasActive);
            replacement.name = oldRoot.name;

            UnityEngine.Object.DestroyImmediate(oldRoot);
            return replacement;
        }
    }
}
#endif

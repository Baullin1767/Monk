#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Monk.Presentation.Editor
{
    public static class EnemyAnimationSetupEditor
    {
        private const string SpriteSheetPath = "Assets/_Project/Sprites/Enemy/tufei.png";
        private const string AnimationsFolder = "Assets/_Project/Animations/Enemy";
        private const string ControllerPath = AnimationsFolder + "/Enemy.controller";
        private const string SpritePrefix = "tufei_";
        private const float ClipFps = 12f;

        private static readonly int[] IdleFrames = { 0, 1, 2, 3 };
        private static readonly int[] WalkFrames = { 4, 5, 6, 7, 8 };
        private static readonly int[] AttackFrames = { 9, 10, 11, 12, 13, 14, 15, 16 };
        private static readonly int[] HurtFrames = { 17, 18, 19 };
        private static readonly int[] DeathFrames = { 20, 21, 22, 23 };

        [MenuItem("Tools/Monk/Setup/Generate Enemy Animations")]
        public static void GenerateEnemyAnimations()
        {
            EnsureFolder("Assets/_Project");
            EnsureFolder("Assets/_Project/Animations");
            EnsureFolder(AnimationsFolder);

            var sprites = LoadSheetSprites();
            var hasSpriteFrames = sprites.Count > 0;

            var idleClip = hasSpriteFrames
                ? CreateSpriteClip("EnemyIdle", IdleFrames, true, sprites)
                : LoadExistingClip("EnemyIdle");
            var walkClip = hasSpriteFrames
                ? CreateSpriteClip("EnemyWalk", WalkFrames, true, sprites)
                : LoadExistingClip("EnemyWalk");
            var attackClip = hasSpriteFrames
                ? CreateSpriteClip("EnemyAttack", AttackFrames, false, sprites)
                : LoadExistingClip("EnemyAttack");
            var hurtClip = hasSpriteFrames
                ? CreateSpriteClip("EnemyHurt", HurtFrames, false, sprites)
                : LoadExistingClip("EnemyHurt");
            var deathClip = hasSpriteFrames
                ? CreateSpriteClip("EnemyDeath", DeathFrames, false, sprites)
                : LoadExistingClip("EnemyDeath");

            if (idleClip == null || walkClip == null || attackClip == null || hurtClip == null || deathClip == null)
            {
                Debug.LogError($"Unable to build controller. Missing sprite frames and/or animation clips from '{AnimationsFolder}'.");
                return;
            }

            var controller = LoadOrCreateController();
            ConfigureController(controller, idleClip, walkClip, attackClip, hurtClip, deathClip);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Enemy animations and controller were generated successfully.");
        }

        private static AnimationClip LoadExistingClip(string clipName)
        {
            return AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimationsFolder}/{clipName}.anim");
        }

        private static Dictionary<int, Sprite> LoadSheetSprites()
        {
            var result = new Dictionary<int, Sprite>();
            var candidatePaths = new List<string> { SpriteSheetPath };

            var textureGuids = AssetDatabase.FindAssets("tufei t:Texture2D");
            foreach (var guid in textureGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!assetPath.EndsWith("/tufei.png"))
                {
                    continue;
                }

                if (!candidatePaths.Contains(assetPath))
                {
                    candidatePaths.Add(assetPath);
                }
            }

            foreach (var path in candidatePaths)
            {
                var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
                foreach (var asset in subAssets)
                {
                    TryAddSprite(asset, result);
                }

                var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var asset in allAssets)
                {
                    TryAddSprite(asset, result);
                }

                if (result.Count > 0)
                {
                    return result;
                }
            }

            return result;
        }

        private static void TryAddSprite(Object asset, IDictionary<int, Sprite> sprites)
        {
            if (asset is not Sprite sprite)
            {
                return;
            }

            var suffix = sprite.name.Replace(SpritePrefix, string.Empty);
            if (int.TryParse(suffix, out var frame))
            {
                sprites[frame] = sprite;
            }
        }

        private static AnimationClip CreateSpriteClip(string clipName, int[] frameIndices, bool loop, IReadOnlyDictionary<int, Sprite> sprites)
        {
            var clipPath = $"{AnimationsFolder}/{clipName}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            clip.frameRate = ClipFps;

            var binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = string.Empty,
                propertyName = "m_Sprite"
            };

            var keyframes = new List<ObjectReferenceKeyframe>(frameIndices.Length);
            for (var i = 0; i < frameIndices.Length; i++)
            {
                if (!sprites.TryGetValue(frameIndices[i], out var s))
                {
                    continue;
                }

                keyframes.Add(new ObjectReferenceKeyframe
                {
                    time = i / ClipFps,
                    value = s
                });
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes.ToArray());

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            settings.stopTime = frameIndices.Length / ClipFps;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimatorController LoadOrCreateController()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller != null)
            {
                return controller;
            }

            return AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        }

        private static void ConfigureController(
            AnimatorController controller,
            AnimationClip idleClip,
            AnimationClip walkClip,
            AnimationClip attackClip,
            AnimationClip hurtClip,
            AnimationClip deathClip)
        {
            EnsureParameter(controller, "Speed", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "Attack", AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, "Hurt", AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, "Death", AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, "IsDead", AnimatorControllerParameterType.Bool);

            var stateMachine = controller.layers[0].stateMachine;
            var locomotionState = EnsureState(stateMachine, "Locomotion", null, true);
            var attackState = EnsureState(stateMachine, "Attack", attackClip, false);
            var hurtState = EnsureState(stateMachine, "Hurt", hurtClip, false);
            var deadState = EnsureState(stateMachine, "Dead", deathClip, false);

            RemoveStateIfExists(stateMachine, "Idle");
            RemoveStateIfExists(stateMachine, "Walk");

            var locomotionTree = GetOrCreateBlendTree(controller, "EnemyLocomotionBlendTree");
            ConfigureLocomotionBlendTree(locomotionTree, idleClip, walkClip);
            locomotionState.motion = locomotionTree;

            ResetTransitions(stateMachine);

            AddTransition(attackState, locomotionState, true, 0.05f);
            AddTransition(hurtState, locomotionState, true, 0.05f);

            AddAnyStateTransition(stateMachine, attackState, false, 0.05f,
                ("Attack", AnimatorConditionMode.If, 0f),
                ("IsDead", AnimatorConditionMode.IfNot, 0f));

            AddAnyStateTransition(stateMachine, hurtState, false, 0.05f,
                ("Hurt", AnimatorConditionMode.If, 0f),
                ("IsDead", AnimatorConditionMode.IfNot, 0f));

            AddAnyStateTransition(stateMachine, deadState, false, 0.05f,
                ("IsDead", AnimatorConditionMode.If, 0f));

            EditorUtility.SetDirty(controller);
        }

        private static void ConfigureLocomotionBlendTree(BlendTree tree, Motion idleMotion, Motion walkMotion)
        {
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = "Speed";
            tree.blendParameterY = string.Empty;
            tree.useAutomaticThresholds = false;
            tree.minThreshold = 0f;
            tree.maxThreshold = 1f;

            var children = new ChildMotion[2];
            children[0] = new ChildMotion { motion = idleMotion, threshold = 0f, timeScale = 1f };
            children[1] = new ChildMotion { motion = walkMotion, threshold = 1f, timeScale = 1f };
            tree.children = children;
        }

        private static BlendTree GetOrCreateBlendTree(AnimatorController controller, string blendTreeName)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(ControllerPath);
            foreach (var asset in assets)
            {
                if (asset is BlendTree tree && tree.name == blendTreeName)
                {
                    return tree;
                }
            }

            var createdTree = new BlendTree { name = blendTreeName };
            AssetDatabase.AddObjectToAsset(createdTree, controller);
            return createdTree;
        }

        private static void EnsureParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            if (controller.parameters.Any(parameter => parameter.name == name))
            {
                return;
            }

            controller.AddParameter(name, type);
        }

        private static AnimatorState EnsureState(AnimatorStateMachine machine, string stateName, Motion motion, bool isDefault)
        {
            foreach (var childState in machine.states)
            {
                if (childState.state.name != stateName)
                {
                    continue;
                }

                childState.state.motion = motion;
                if (isDefault)
                {
                    machine.defaultState = childState.state;
                }

                return childState.state;
            }

            var createdState = machine.AddState(stateName);
            createdState.motion = motion;

            if (isDefault)
            {
                machine.defaultState = createdState;
            }

            return createdState;
        }

        private static void RemoveStateIfExists(AnimatorStateMachine machine, string stateName)
        {
            foreach (var childState in machine.states)
            {
                if (childState.state.name != stateName)
                {
                    continue;
                }

                machine.RemoveState(childState.state);
                return;
            }
        }

        private static void ResetTransitions(AnimatorStateMachine stateMachine)
        {
            for (var i = stateMachine.anyStateTransitions.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveAnyStateTransition(stateMachine.anyStateTransitions[i]);
            }

            foreach (var childState in stateMachine.states)
            {
                var state = childState.state;
                for (var i = state.transitions.Length - 1; i >= 0; i--)
                {
                    state.RemoveTransition(state.transitions[i]);
                }
            }
        }

        private static void AddTransition(
            AnimatorState from,
            AnimatorState to,
            bool hasExitTime,
            float duration,
            params (string parameter, AnimatorConditionMode mode, float threshold)[] conditions)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = hasExitTime;
            transition.duration = duration;
            transition.exitTime = hasExitTime ? 0.95f : 0f;

            foreach (var condition in conditions)
            {
                transition.AddCondition(condition.mode, condition.threshold, condition.parameter);
            }
        }

        private static void AddAnyStateTransition(
            AnimatorStateMachine stateMachine,
            AnimatorState to,
            bool hasExitTime,
            float duration,
            params (string parameter, AnimatorConditionMode mode, float threshold)[] conditions)
        {
            var transition = stateMachine.AddAnyStateTransition(to);
            transition.hasExitTime = hasExitTime;
            transition.duration = duration;
            transition.exitTime = hasExitTime ? 0.95f : 0f;

            foreach (var condition in conditions)
            {
                transition.AddCondition(condition.mode, condition.threshold, condition.parameter);
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parent = folderPath[..folderPath.LastIndexOf('/')];
            var folderName = folderPath[(folderPath.LastIndexOf('/') + 1)..];

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
#endif

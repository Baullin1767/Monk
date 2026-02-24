#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Monk.Presentation.Editor
{
    public static class PlayerAnimationSetupEditor
    {
        private const string SpriteSheetPath = "Assets/nemaycojohn/player and enemy/PL.png";
        private const string AnimationsFolder = "Assets/_Project/Animations/Player";
        private const string ControllerPath = AnimationsFolder + "/Player.controller";
        private const float ClipFps = 12f;

        private static readonly int[] IdleFrames = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        private static readonly int[] RunFrames = { 12, 13, 14, 15, 16, 17, 18, 19 };
        private static readonly int[] Attack1Frames = { 20, 21, 22, 23, 24, 25, 26 };
        private static readonly int[] Attack2Frames = { 27, 28, 29, 30 };
        private static readonly int[] JumpStartFrames = { 31, 32, 33 };
        private static readonly int[] AirFrames = { 34, 35, 36, 37 };
        private static readonly int[] LandFrames = { 38, 39, 40 };
        private static readonly int[] DeadFrames = { 41, 42, 43, 44 };

        [MenuItem("Tools/Monk/Setup/Generate Player Animations")]
        public static void GeneratePlayerAnimations()
        {
            EnsureFolder("Assets/_Project");
            EnsureFolder("Assets/_Project/Animations");
            EnsureFolder(AnimationsFolder);

            var sprites = LoadSheetSprites();
            var hasSpriteFrames = sprites.Count > 0;

            var idleClip = hasSpriteFrames
                ? CreateSpriteClip("PlayerIdle", IdleFrames, true, sprites)
                : LoadExistingClip("PlayerIdle");
            var runClip = hasSpriteFrames
                ? CreateSpriteClip("PlayerRun", RunFrames, true, sprites)
                : LoadExistingClip("PlayerRun");
            var jumpStartClip = hasSpriteFrames
                ? CreateSpriteClip("PlayerJump", JumpStartFrames, false, sprites)
                : LoadExistingClip("PlayerJump");
            var airClip = hasSpriteFrames
                ? CreateSpriteClip("PlayerAir", AirFrames, true, sprites)
                : LoadExistingClip("PlayerAir");
            var landClip = hasSpriteFrames
                ? CreateSpriteClip("PlayerLand", LandFrames, false, sprites)
                : LoadExistingClip("PlayerLand");
            var attack1Clip = hasSpriteFrames
                ? CreateSpriteClip("PlayerAttack", Attack1Frames, false, sprites)
                : LoadExistingClip("PlayerAttack");
            var attack2Clip = hasSpriteFrames
                ? CreateSpriteClip("PlayerAttack2", Attack2Frames, false, sprites)
                : LoadExistingClip("PlayerAttack2");
            var deadClip = hasSpriteFrames
                ? CreateSpriteClip("PlayerDead", DeadFrames, false, sprites)
                : LoadExistingClip("PlayerDead");

            if (idleClip == null || runClip == null || jumpStartClip == null || airClip == null || landClip == null || attack1Clip == null || attack2Clip == null || deadClip == null)
            {
                Debug.LogError($"Unable to build controller. Missing sprite frames and/or animation clips from '{AnimationsFolder}'.");
                return;
            }

            var controller = LoadOrCreateController();
            ConfigureController(controller, idleClip, runClip, jumpStartClip, airClip, landClip, attack1Clip, attack2Clip, deadClip);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Player animations and controller were generated successfully.");
        }

        private static AnimationClip LoadExistingClip(string clipName)
        {
            return AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimationsFolder}/{clipName}.anim");
        }

        private static Dictionary<int, Sprite> LoadSheetSprites()
        {
            var result = new Dictionary<int, Sprite>();
            var candidatePaths = new List<string> { SpriteSheetPath };

            var textureGuids = AssetDatabase.FindAssets("PL t:Texture2D");
            foreach (var guid in textureGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!assetPath.EndsWith("/player and enemy/PL.png"))
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

            var suffix = sprite.name.Replace("PL_", string.Empty);
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
                if (!sprites.TryGetValue(frameIndices[i], out var sprite))
                {
                    continue;
                }

                keyframes.Add(new ObjectReferenceKeyframe
                {
                    time = i / ClipFps,
                    value = sprite
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
            AnimationClip runClip,
            AnimationClip jumpStartClip,
            AnimationClip airClip,
            AnimationClip landClip,
            AnimationClip attack1Clip,
            AnimationClip attack2Clip,
            AnimationClip deadClip)
        {
            EnsureParameter(controller, "Speed", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "Grounded", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "VerticalVelocity", AnimatorControllerParameterType.Float);
            EnsureParameter(controller, "Attack", AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, "Attack2", AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, "Death", AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, "IsDead", AnimatorControllerParameterType.Bool);

            var stateMachine = controller.layers[0].stateMachine;
            var locomotionState = EnsureState(stateMachine, "Locomotion", null, true);
            var jumpStartState = EnsureState(stateMachine, "JumpStart", jumpStartClip, false);
            var airState = EnsureState(stateMachine, "Air", airClip, false);
            var landState = EnsureState(stateMachine, "Land", landClip, false);
            var attack1State = EnsureState(stateMachine, "Attack1", attack1Clip, false);
            var attack2State = EnsureState(stateMachine, "Attack2", attack2Clip, false);
            var deadState = EnsureState(stateMachine, "Dead", deadClip, false);

            RemoveStateIfExists(stateMachine, "Idle");
            RemoveStateIfExists(stateMachine, "Run");
            RemoveStateIfExists(stateMachine, "Jump");
            RemoveStateIfExists(stateMachine, "Attack");

            var locomotionTree = GetOrCreateBlendTree(controller, "PlayerLocomotionBlendTree");
            ConfigureLocomotionBlendTree(locomotionTree, idleClip, runClip);
            locomotionState.motion = locomotionTree;

            ResetTransitions(stateMachine);

            AddTransition(locomotionState, jumpStartState, false, 0.05f, ("Grounded", AnimatorConditionMode.IfNot, 0f));
            AddTransition(jumpStartState, airState, true, 0.05f);
            AddTransition(airState, landState, false, 0.05f, ("Grounded", AnimatorConditionMode.If, 0f));
            AddTransition(landState, locomotionState, true, 0.05f);

            AddTransition(attack1State, attack2State, false, 0.02f, ("Attack2", AnimatorConditionMode.If, 0f));
            AddTransition(attack1State, locomotionState, true, 0.08f);
            AddTransition(attack2State, locomotionState, true, 0.08f);

            AddAnyStateTransition(stateMachine, attack1State, false, 0.05f,
                ("Attack", AnimatorConditionMode.If, 0f),
                ("IsDead", AnimatorConditionMode.IfNot, 0f));

            AddAnyStateTransition(stateMachine, deadState, false, 0.05f, ("IsDead", AnimatorConditionMode.If, 0f));

            EditorUtility.SetDirty(controller);
        }

        private static void ConfigureLocomotionBlendTree(BlendTree tree, Motion idleMotion, Motion runMotion)
        {
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = "Speed";
            tree.blendParameterY = string.Empty;
            tree.useAutomaticThresholds = false;
            tree.minThreshold = 0f;
            tree.maxThreshold = 1f;

            var children = new ChildMotion[2];
            children[0] = new ChildMotion { motion = idleMotion, threshold = 0f, timeScale = 1f };
            children[1] = new ChildMotion { motion = runMotion, threshold = 1f, timeScale = 1f };
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

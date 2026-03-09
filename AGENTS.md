# Monk - Project Rules

## Overview
2D physics-based platformer for mobile (Unity 6, URP).

## Architecture
Clean Architecture with strict layer boundaries enforced by assembly definitions:

- **Core** (pure C#, no Unity references) — interfaces, models, enums, pure services
- **Common** — utilities, extensions, ServiceLocator
- **Configs** — ScriptableObjects for all tuning data
- **Infrastructure** — persistence, audio, scene loading
- **Input** — platform-aware input abstraction
- **Presentation** — MonoBehaviours, UI, camera, visuals

Dependency flow: Core ← Common ← Configs ← Infrastructure ← Input ← Presentation

## Namespaces
- `Monk.Core`, `Monk.Common`, `Monk.Configs`, `Monk.Infrastructure`, `Monk.Input`, `Monk.Presentation`

## Folder Structure
All game code lives under `Assets/_Project/`. Third-party packages stay outside.

## Key Conventions
- Use **Rigidbody2D** for all movement — no transform.position manipulation for physics objects
- Use **DOTween** for animations/tweens
- Use **Joystick Pack** for mobile input
- Use **Cinemachine 3.x** for camera management
- **ScriptableObjects** for all config/tuning data
- **ServiceLocator** pattern — no heavy DI frameworks, no singletons except through ServiceLocator
- **C# events** over UnityEvents for inter-system communication
- Assembly definitions enforce layer boundaries; Core has `noEngineReferences: true`

## Performance (Mobile-First)
- Pool frequently instantiated objects (enemies, projectiles, VFX)
- Avoid allocations in Update/FixedUpdate (no LINQ, no foreach on collections that allocate)
- Use `CompareTag()` instead of `tag ==`
- Cache component references in Awake/Start

## Code Style
- One class per file, file name matches class name
- Interfaces prefixed with `I` (e.g., `IPlayerModel`)
- Config ScriptableObjects suffixed with `Config`
- MonoBehaviour views/controllers named descriptively (e.g., `PlayerMovement`, `HUDView`)

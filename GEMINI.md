# Project02: Unity Hybrid Strategy Game

## Project Overview
Project02 is a Hybrid Strategy Game built with Unity 6.0. It combines Building, Tower Defense, and Wave Attack mechanics into a multi-dimensional game loop. The project focuses on modularity, loose coupling, and high performance through modern Unity techniques.

## Core Gameplay Systems
- **Building System**: Grid-based map for resource gathering and base construction.
- **Tower Defense System**: Management of defensive towers with weight-based AI.
- **Wave Attack System**: Deployment and coordination of offensive units.
- **Smart Wave Converter**: Interpolates static wave configurations into dynamic battlefield data.

## Architecture & Conventions
- **PBR (Preset - Behaviour - Runtime)**: A core architectural pattern (inspired by ECS) that separates data configuration from execution logic.
    - **Presets**: `ScriptableObject` classes (e.g., `BaseObjectPresetSo`) containing static data and strategy installers.
    - **Runtime**: `MonoBehaviour` classes (e.g., `BaseObjectRuntime`) that handle the execution lifecycle and component management.
- **Event-Driven**: Loose coupling between Data (Backend) and UI (Frontend) using events.
- **Multi-Scene Management**: The game is split into independent contexts (Bootstrapper, Global Gameplay, Building, etc.).
- **Assembly Definitions**: Logic is organized into assemblies like `Core.Gameplay.asmdef` to improve compilation times and enforce dependency rules.

## Tech Stack
- **Unity 6.0+**: Primary engine.
- **UniTask**: For zero-allocation asynchronous operations.
- **LitMotion / PrimeTween**: For high-performance tweening and animations.
- **Reflex**: Dependency Injection framework for managing object lifetimes and dependencies.
- **Addressables**: For dynamic asset loading and memory management.
- **KBCore.Refs**: For automatic attribute-based component injection (e.g., `[Self]`, `[Child]`).
- **UI Toolkit**: Likely used for modern, performant UI.

## Building and Running
- **Requirements**: Unity Editor 6.0 or higher.
- **Entry Point**: Always start from the `Bootstrapper.unity` scene located in `Assets/_Project/Scenes/`. This ensures all managers and controllers are initialized correctly.
- **Packages**: Ensure all dependencies from `manifest.json` are resolved via the Unity Package Manager.

## Directory Structure (Core)
- `Assets/_Project/`: Main project folder.
    - `Scripts/Gameplay/`: Modular gameplay logic.
        - `BaseGameplay/`: Core entity logic and PBR base classes.
        - `BuildingGameplay/`, `TowerDefense/`, `WaveAttack/`: Feature-specific modules.
        - `Global/`: Shared managers, data systems, and UI.
    - `Data/`: ScriptableObject instances (Presets).
    - `Scenes/`: Multi-scene environment files.
    - `Prefabs/`: Reusable game objects and entities.

## Development Guidelines
- **Modularity**: Keep features isolated. Use the existing module structure in `Scripts/Gameplay/`.
- **PBR Pattern**: When adding new entities, separate their data into a `PresetSo` and their logic into a `Runtime` class.
- **Async First**: Prefer `UniTask` over Coroutines for asynchronous logic.
- **Reference Injection**: Use `KBCore.Refs` attributes for internal component references to reduce boilerplate in `Awake`/`Start`.
- **Optimization**: Use Object Pooling (via Addressables) for frequently spawned entities like projectiles or units.

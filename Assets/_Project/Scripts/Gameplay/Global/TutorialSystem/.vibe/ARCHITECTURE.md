# Tutorial System Architecture

## System Purpose
To provide a data-driven, graph-based tutorial system that cleanly separates tutorial sequence definition (data), execution logic, and visual rendering (UI/Spatial). It allows for creating complex tutorial flows without hardcoding scene dependencies.

## Core Components
- **`TutorialData`** (`ScriptableObject`): Container holding a sequence of tutorial steps (`TutorialId`, `StartStep`).
- **`TutorialStepData`** (`ScriptableObject`): Represents a single step, containing the message, `TutorialDisplayType`, and a reference to the `NextStep`.
- **`TutorialManager`** (`Singleton MonoBehaviour`): Owns the runtime state of the active tutorial. Coordinates step transitions and persists completion status to `PlayerPrefs`.
- **`TutorialUI`** (`MonoBehaviour`): Responsible for rendering the tutorial text and spatial hints (arrows/highlights) based on step data and anchor positions.
- **`TutorialAnchor`** (`MonoBehaviour`): Attached to interactable GameObjects in the scene. Holds a reference to its corresponding `TutorialStepData` and automatically registers itself upon activation.
- **`TutorialAnchorRegistry`** (`static class`): Acts as an in-memory spatial lookup, mapping `TutorialStepData` to the active `TutorialAnchor` instance in the scene.

## Runtime Flow Summary
The system operates on an event-driven pull/push model:
1. `TutorialManager` initiates a sequence and *pulls* the required spatial position by querying the `TutorialAnchorRegistry` using the current `TutorialStepData`.
2. It pushes the display command to `TutorialUI`.
3. When the user interacts with the scene object, the `TutorialAnchor` *pushes* a trigger event back to the `TutorialManager` to advance the step.

## Dependencies
- Unity `PlayerPrefs` (for progress persistence).
- Unity UI / Custom UI Framework (for `TutorialUI`).
- `TutorialGraphWindow` / `TutorialStepNode` (Editor-only dependencies for visual scripting).

## Extension Points
- `TutorialDisplayType` enum can be expanded for new hint styles.
- `TutorialUI` can be replaced or extended to support different visual presentations without altering the core logic.
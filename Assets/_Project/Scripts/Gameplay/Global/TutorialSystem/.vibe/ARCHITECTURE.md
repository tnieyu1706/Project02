# Tutorial System Architecture

## System Purpose
To provide a data-driven, graph-based tutorial system that cleanly separates tutorial sequence definition (data), execution logic, and visual rendering (UI/Spatial). It allows for creating complex tutorial flows without hardcoding scene dependencies.

## Core Components
- **`ITutorialManager`** (`Interface`): Defines the contract for tutorial lifecycle and progression management.
- **`TutorialData`** (`ScriptableObject`): Container holding a sequential list of tutorial steps (`TutorialId`, `Steps`).
- **`TutorialStepData`** (`ScriptableObject`): Represents a single step, containing the message and `TutorialDisplayType`.
- **`TutorialManager`** (`Singleton MonoBehaviour`): Owns the runtime state and `currentIndex`. Coordinates transitions and broadcasts state via events (`OnStepStarted`, `OnTutorialEnded`).
- **`TutorialUI`** (`MonoBehaviour`): Frontend View. Subscribes to manager events. Renders text, handles World-to-Screen conversion for hints, and advances text-only steps via message panel click.
- **`TutorialAnchor`** (`MonoBehaviour`): Attached to GameObjects. Holds a reference to its corresponding `TutorialStepData` and triggers the Manager.
- **`TutorialAnchorRegistry`** (`static class`): Mapping `TutorialStepData` to the active `TutorialAnchor`.
- **`TutorialGraphWindow`** (`EditorWindow`): Visual editor for `TutorialData`. Manages sequential list reconstruction, automatic node naming (`<DataName>_<Index>`), and drag-to-create node UX.

## Runtime Flow Summary
The system follows an event-driven MVC pattern:
1. `TutorialManager` increments `currentIndex` and broadcasts `OnStepStarted` with the step data and the target anchor's `Transform`.
2. `TutorialUI` receives the event and updates its visual state.
3. `TutorialUI` continuously tracks the anchor's world position and converts it to screen space using `Registry<Camera>.GetFirst()`.
4. User interaction via `TutorialAnchor` triggers `TutorialManager.Next(stepData)`, validating the sequence before advancing.

## Dependencies
- `TnieYuPackage.DesignPatterns.Registry` (for Camera lookup).
- Unity UI (Screen Space Overlay Canvas).
- `PlayerPrefs` (for progress persistence).

## Constraints
- TutorialManager is a Singleton but its lifecycle is managed by the Scene/Bootstrapper (no `DontDestroyOnLoad`).
- TutorialManager must NOT reference TutorialUI.
- PlayerPrefs key must use a constant prefix (`Tutorial_Complete_`).
- TutorialStepData assets use persistent random IDs (`Step_XXXX`) to protect external references.
- Graph Editor uses visual prefixes (e.g., `[0]`, `[1]`) for sequence order without renaming assets.
- Tutorial progression MUST be index-based sequentially, not linked-list.

## Extension Points
- `TutorialDisplayType` enum can be expanded for new hint styles.
- `TutorialUI` can be replaced or extended to support different visual presentations without altering the core logic.
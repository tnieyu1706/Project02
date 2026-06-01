# [Turn 1] — Tutorial System Refactor (MVC & Screen Space)
...
[rest of history content]
...
## [Turn 6] — TutorialSystem Refactoring & Standardized Documentation

**Files changed:**
- `Assets/_Project/Scripts/Gameplay/Global/TutorialSystem/TutorialStepData.cs` — modified: added XML documentation and standardized naming.
- `Assets/_Project/Scripts/Gameplay/Global/TutorialSystem/TutorialData.cs` — modified: added XML documentation.
- `Assets/_Project/Scripts/Gameplay/Global/TutorialSystem/TutorialAnchor.cs` — modified: added XML documentation.
- `Assets/_Project/Scripts/Gameplay/Global/TutorialSystem/TutorialAnchorRegistry.cs` — modified: added XML documentation and standardized field naming.
- `Assets/_Project/Scripts/Gameplay/Global/TutorialSystem/TutorialManager.cs` — modified: introduced `ITutorialManager` interface, added XML documentation, and standardized field naming (`_` prefix).
- `Assets/_Project/Scripts/Gameplay/Global/TutorialSystem/TutorialUI.cs` — modified: added XML documentation and standardized field naming.

**Docs updated:**
- `.vibe/ARCHITECTURE.md` — updated component list with `ITutorialManager`.
- `.vibe/CHANGE.md` — updated with turn 6 changes.
- `.vibe/HISTORY.md` — appended turn 6 history.

**Flagged:**
- None

## [Turn 7] — Reverting to Persistent IDs in TutorialGraph

**Files changed:**
- `Assets/_Project/Scripts/Gameplay/Global/TutorialSystem/Editor/TutorialGraphWindow.cs` — modified: reverted to persistent random IDs (`Step_<RandomID>`) for assets. Refactored naming logic to `UpdateNodeVisuals`, which adds a sequential prefix (`[Index]`) to node titles without renaming the underlying asset. Updated `LoadData` to reconstruct connections from the sequential list.

**Docs updated:**
- `.vibe/ARCHITECTURE.md` — clarified node naming and persistence constraints.
- `.vibe/CHANGE.md` — updated with turn 7 changes.
- `.vibe/HISTORY.md` — appended turn 7 history.

**Flagged:**
- None

# [Turn 7] — Reverting to Persistent IDs in TutorialGraph

**Files changed:**
- `Assets/_Project/Scripts/Gameplay/Global/TutorialSystem/Editor/TutorialGraphWindow.cs` — modified: reverted to persistent random IDs (`Step_<RandomID>`) for assets. Refactored naming logic to `UpdateNodeVisuals`, which adds a sequential prefix (`[Index]`) to node titles without renaming the underlying asset. Updated `LoadData` to reconstruct connections from the sequential list.

**Docs updated:**
- `.vibe/ARCHITECTURE.md` — clarified node naming and persistence constraints.
- `.vibe/CHANGE.md` — updated with turn 7 changes.
- `.vibe/HISTORY.md` — appended turn 7 history.

**Flagged:**
- None

# [Turn 1] — Tutorial System Refactor (MVC & Screen Space)

**Files changed:**
- `Assets/_Project/Scripts/Gameplay/Global/TutorialSystem/TutorialStepData.cs` — modified: removed `NextStep` property.
- `Assets/_Project/Scripts/Gameplay/Global/TutorialSystem/TutorialData.cs` — modified: removed `StartStep`, now relies on sequential `Steps` list.
- `Assets/_Project/Scripts/Gameplay/Global/TutorialSystem/TutorialManager.cs` — modified: decoupled from `TutorialUI` using events, implemented index-based progression.
- `Assets/_Project/Scripts/Gameplay/Global/TutorialSystem/TutorialUI.cs` — modified: subscribed to `TutorialManager` events, implemented Screen Space Overlay support with camera registry tracking.

**Docs updated:**
- `.vibe/ARCHITECTURE.md` — updated component responsibilities and dependencies.
- `.vibe/FLOW.md` — updated runtime flow to reflect event-driven and index-based logic.
- `.vibe/SUMMARY.md` — updated system status.
- `.vibe/CHANGE.md` — updated with latest changes.
- `.vibe/HISTORY.md` — appended turn history.

**Flagged:**
- None

## [Turn 2] — TutorialGraphWindow Refactor (Sequential Support)

**Files changed:**
- `Assets/_Project/Scripts/Gameplay/Global/TutorialSystem/Editor/TutorialGraphWindow.cs` — modified: refactored `SaveData` and `LoadData` to support index-based sequential logic; updated `TutorialStepNode` to enforce linear flow.

**Docs updated:**
- `.vibe/ARCHITECTURE.md` — updated responsibilities for `TutorialGraphWindow`.
- `.vibe/CHANGE.md` — updated with turn 2 changes.
- `.vibe/HISTORY.md` — appended turn 2 history.

**Flagged:**
- None

## [Turn 3] — TutorialSystem Refinement (Lifecycle & Testing)

**Files changed:**
- `Assets/_Project/Scripts/Gameplay/Global/TutorialSystem/TutorialManager.cs` — modified: removed `DontDestroyOnLoad`, changed `playerPrefsPrefix` to `const`, and added debug logs.
- `Assets/_Project/Scripts/Gameplay/Global/TutorialSystem/Test/TutorialStarter.cs` — modified: added `_forceRestart` toggle and `ResetProgress` button for easier testing.

**Docs updated:**
- `.vibe/ARCHITECTURE.md` — updated lifecycle constraints.
- `.vibe/CHANGE.md` — updated with turn 3 changes.
- `.vibe/HISTORY.md` — appended turn 3 history.

**Flagged:**
- None

## [Turn 4] — Tutorial Interaction Refinement (Click-to-Continue)

**Files changed:**
- `Assets/_Project/Scripts/Gameplay/Global/TutorialSystem/TutorialUI.cs` — modified: added `messageButton` reference and click logic to advance text-only steps.

**Docs updated:**
- `.vibe/ARCHITECTURE.md` — updated `TutorialUI` responsibilities.
- `.vibe/CHANGE.md` — updated with turn 4 changes.
- `.vibe/HISTORY.md` — appended turn 4 history.

**Flagged:**
- None

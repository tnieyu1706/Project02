# Tutorial System Runtime Flow

## Step-by-Step Execution

1. **Initialization:**
   - External caller invokes `TutorialManager.Instance.StartTutorial(TutorialData)`.
   - `TutorialManager` checks `PlayerPrefs` to ensure it hasn't been completed.
   - `currentTutorialSequence` and `currentStepData` are set.
   - `isTutorialActive` is set to `true`.
   - Calls `ShowCurrentStep()`.

2. **Displaying a Step:**
   - `TutorialManager.ShowCurrentStep()` checks the `TutorialDisplayType` of `currentStepData`.
   - If a spatial hint is required (`Hint` or `TextHint`), it queries `TutorialAnchorRegistry.GetAnchor(currentStepData)`.
   - The Registry returns the active `TutorialAnchor` (which registered itself during `OnEnable`).
   - `TutorialManager` extracts the `transform.position` of the Anchor.
   - `TutorialUI.ShowStep(currentStepData, targetPosition)` is called to render the UI.

3. **User Interaction & Triggering:**
   - User clicks or interacts with the specific GameObject holding the `TutorialAnchor` in the scene.
   - The interaction event calls `TutorialAnchor.TriggerNextStep()`.
   - The Anchor calls `TutorialManager.Instance.Next(TargetStep)`, passing its associated `TutorialStepData`.

4. **Validation and Transition:**
   - `TutorialManager.Next(triggeredStep)` validates that the `triggeredStep` exactly matches `currentStepData`.
   - If valid, calls `AdvanceToNextStep()`.
   - `currentStepData` is updated to `currentStepData.NextStep`.
   - If `NextStep` is not null, the flow loops back to **Phase 2 (Displaying a Step)**.

5. **Completion:**
   - If `currentStepData.NextStep` is null, `AdvanceToNextStep()` calls `CompleteTutorial()`.
   - Progress is saved: `PlayerPrefs.SetInt("Tutorial_Complete_" + currentTutorialSequence.TutorialId, 1)`.
   - `EndTutorial()` is called: state is cleared, and `TutorialUI.Hide()` is invoked.

## Data Flow
`ScriptableObject` definitions act as the primary keys traversing the system:
`TutorialData` -> extracts -> `TutorialStepData` -> queries -> `TutorialAnchorRegistry` -> resolves -> `TutorialAnchor` position -> passes to -> `TutorialUI`.
# Tutorial System Runtime Flow

## Step-by-Step Execution

1. **Initialization:**
   - External caller invokes `TutorialManager.Instance.StartTutorial(TutorialData)`.
   - `TutorialManager` sets `_currentIndex = 0` and broadcasts `OnStepStarted`.

2. **Displaying a Step (View):**
   - `TutorialUI` receives `OnStepStarted(stepData, targetAnchorTransform)`.
   - It sets the message text and activates the hint if a target anchor is provided.
   - **Continuous Tracking:** In `Update()`, `TutorialUI` converts `targetAnchor.position` to screen space using `Registry<Camera>.GetFirst().WorldToScreenPoint()` and updates the hint's `RectTransform.position`.

3. **User Interaction & Triggering:**
   - User interacts with the `TutorialAnchor` in the scene.
   - `TutorialAnchor` calls `TutorialManager.Instance.Next(TargetStep)`.

4. **Validation and Transition:**
   - `TutorialManager` validates that the `triggeredStep` matches `Steps[_currentIndex]`.
   - If valid, `_currentIndex` is incremented.
   - If more steps exist, the cycle repeats from step 1 (broadcasting `OnStepStarted`).

5. **Completion:**
   - If `_currentIndex` exceeds the list size, `CompleteTutorial()` is called.
   - `OnTutorialEnded` is broadcasted.
   - `TutorialUI` hides all elements.

## Data Flow
`ScriptableObject` definitions act as the primary keys traversing the system:
`TutorialData` -> extracts -> `TutorialStepData` -> queries -> `TutorialAnchorRegistry` -> resolves -> `TutorialAnchor` position -> passes to -> `TutorialUI`.
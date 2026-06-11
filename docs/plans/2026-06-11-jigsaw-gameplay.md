# Real Jigsaw Puzzle Gameplay Implementation Plan

> **For Antigravity:** REQUIRED WORKFLOW: Use `.agent/workflows/execute-plan.md` to execute this plan in single-flow mode.

**Goal:** Replace the Cheat Win placeholder in Gameplay with a real jigsaw puzzle board and tray, procedural piece generation by clipping the selected picture, EventSystem drag & drop interaction, snapping, timer, hints (with last-interacted piece priority), preview toggle, back button, and win detection.

**Architecture:** 
1. Use `PuzzleSession` to represent the runtime puzzle state (elapsed time, piece states, hint logic prioritizing the last interacted piece, and completion check).
2. Procedurally generate puzzle pieces at runtime by dividing the picture texture using `Sprite.Create` and dynamic cell size configuration from the static-data service.
3. Manage drag and drop using uGUI EventSystem handler interfaces on `PuzzlePieceView`, computing drag offsets within the drag container space to prevent coordinate jump.
4. Support displacement-based gesture mode classification in `PuzzlePieceView` to allow scrolling the tray on vertical drag, and piece dragging on horizontal drag once past a 10px threshold.
5. Regenerate scene hierarchies in `ThinVerticalSliceSceneSetup.cs` idempotently by checking if a version marker GameObject (`SetupVersionMarker_v2`) exists in the active scene. If already updated, skip regeneration. Configures landscape canvas (`1920x1080`, `matchWidthOrHeight = 0.5f`).
   > [!NOTE]
   > The SetupVersionMarker_v2 handles layout migration idempotency, meaning it prevents regenerating the scene if version 2 is already present. This ensures that scene SHA256 is unchanged on subsequent runs. Note that this version marker does not replace manual or automated scene wiring validations (such as verifying fields aren't accidentally cleared in the editor).
6. Enforce a decoupled completion lifecycle: stop timer $\rightarrow$ disable all top-bar buttons and pieces input $\rightarrow$ fade-in completed full image (1s UniTask transition) $\rightarrow$ delay transition $\rightarrow$ display reward summary $\rightarrow$ cleanup pieces only on leaving scene. Suppress OperationCanceledException when cancellation occurs during scene unload.
7. Open test assembly configuration in `JigsawVina.Tests.asmdef` by changing `includePlatforms` to empty (`[]`) so NUnit PlayMode tests compile and run correctly under player loop assemblies.
8. Enforce single-reward credit execution per session via `IsRewardProcessed` flag tracked on `GameSessionService`.

**Tech Stack:** Unity 6000.3.11f1, uGUI, TMPro, UniTask, VContainer, NUnit (Unity Test Framework).

---

## User Review Required

> [!IMPORTANT]
> The procedural piece generation divides the picture texture into rectangular sub-sprites at runtime.
> We need real landscape pictures (4:3 aspect ratio) to load. I will generate two high-quality watercolor images:
> 1. `ho_guom.png` (Hoan Kiem lake watercolor painting)
> 2. `ha_long.png` (Ha Long Bay watercolor painting)
> and save them to `Assets/Resources/Textures/` to allow `Resources.Load<Texture2D>` to load them.

---

## Proposed Changes

### [Gameplay Core Logic]

#### [MODIFY] [IStaticDataService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/IStaticDataService.cs)
Define `PictureDifficultyConfig` containing grid rows, columns, piece count, and star rewards. Add `GetPictureDifficulty(int pictureId, int difficultyId)` to `IStaticDataService`.

#### [MODIFY] [StaticDataService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/StaticDataService.cs)
Implement difficulty config array returning picture-specific difficulty settings dynamically. Fail fast (throw `KeyNotFoundException`) if configuration does not exist.

#### [MODIFY] [GameSessionService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/GameSessionService.cs)
Add `IsRewardProcessed` flag to avoid double coins/inventories updates if reward display is invoked repeatedly. Reset flag when starting a new picture session.

#### [NEW] [PuzzleSession.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PuzzleSession.cs)
Create the runtime session state class, containing the list of piece statuses, timer, and piece snapping/hint calculations prioritizing `LastInteractedPieceIndex`.

#### [NEW] [PuzzleSessionTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/PuzzleSessionTests.cs)
Write Unit Tests verifying the dynamic grid setup, timer progression, snap calculations, hint prioritization, return to tray, and completion condition.

---

### [Gameplay Presentation Layer]

#### [NEW] [PuzzlePieceView.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PuzzlePieceView.cs)
MonoBehaviour attached to each piece GameObject to receive drag events and forward them to the presenter. Accumulates dragging distance (threshold 10px) before classifying scroll vs drag piece modes.

#### [NEW] [PuzzleBoardView.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PuzzleBoardView.cs)
Board visual representation containing the preview overlay image, toggle preview opacity, board bounds, and locked pieces container. Supports async UniTask-based fade-in completed image animation.

#### [MODIFY] [PuzzlePlayingView.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PuzzlePlayingView.cs)
Modify to expose reference fields for the Board view, Tray content container, scroll view, timer label, back button, preview button, hint button, return to tray button, and dynamic canvas helper references. Add `DisableAllInput()` to freeze controls on win.

#### [MODIFY] [PuzzlePlayingPresenter.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PuzzlePlayingPresenter.cs)
Modify to initialize `PuzzleSession` with static data difficulty grid, load selected picture texture, slice texture into sub-sprites, instantiate `PuzzlePieceView`s (passing board cell dimensions), update timer loop, handle drag and drop snap logic, track last interacted piece, and support separate complete/disable input.

#### [MODIFY] [RewardSummaryPresenter.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/RewardSummaryPresenter.cs)
Modify to read star count dynamically from `IStaticDataService.GetPictureDifficulty` instead of using hard-coded values. Enforce a guard checking `IsRewardProcessed` to ensure rewards are processed only once per session.

#### [MODIFY] [GameplayLifetimeScope.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/GameplayLifetimeScope.cs)
Update flow controller and lifetime scope to bind and inject the new components, handling Back button loaded-scene transitions and async win animation cancellation tokens.

#### [MODIFY] [ThinVerticalSliceSceneSetup.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceSceneSetup.cs)
Modify `CreateGameplayScene()` and `CreateHomeScene()` to be fully idempotent: check if a marker GameObject `SetupVersionMarker_v2` exists in the scene. If present, skip regeneration. Configures landscape canvas (`1920x1080`, `matchWidthOrHeight = 0.5f`). Setup landscape uGUI layout containing:
* Top Bar: Back button, Title, Timer text, Preview toggle button, Hint button, Return to Tray button.
* Main Area: Left Board area (dimmed preview overlay) and Right Tray area (scrolling list).

#### [MODIFY] [TestRunnerHelper.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/TestRunnerHelper.cs)
Add MenuItem route `JigsawVina/Run PlayMode Tests` to execute PlayMode tests programmatically via the Unity TestRunner API.

#### [MODIFY] [JigsawVina.Tests.asmdef](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/JigsawVina.Tests.asmdef)
Modify assembly definition to empty the `includePlatforms` array. This registers the test assembly to compile for both Editor (EditMode) and Runtime Player (PlayMode) loops.

#### [MODIFY] [ProgressionTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/ProgressionTests.cs)
Update progression tests to pass the newly added `IStaticDataService` dependency parameter into `RewardSummaryPresenter` constructor calls to resolve compiler errors.

#### [NEW] [PuzzleGameplayPlayModeTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/PuzzleGameplayPlayModeTests.cs)
PlayMode integration tests validating piece initialization, return-to-tray, hint consumption, drag/snap lock assertions (position, IsLocked, sizeDelta, reparenting), and win state changes.

---

## Tasks

### Task 1: Puzzle Session Data Model & Tests

**Files:**
* Modify: `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/IStaticDataService.cs`
* Modify: `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/StaticDataService.cs`
* Create: `JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PuzzleSession.cs`
* Create: `JigsawVina/Assets/JigsawVina/Tests/PuzzleSessionTests.cs`

**Step 1: Implement `IStaticDataService.cs` and `StaticDataService.cs` adjustments**
(Identical code structure to previous iteration).

---

### Task 2: UI Visual Components & Asset Generation

**Files:**
* Create: `JigsawVina/Assets/Resources/Textures/ho_guom.png`
* Create: `JigsawVina/Assets/Resources/Textures/ha_long.png`
* Create: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PuzzlePieceView.cs`
* Create: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PuzzleBoardView.cs`

**Step 1: Generate Pictures via generate_image**
Generate Ho Guom and Vinh Ha Long textures matching MVP 4:3 landscape requirements.

**Step 2: Write PuzzlePieceView C# script**
Create `PuzzlePieceView.cs` (handling displacement-based horizontal drag modes and ScrollRect forwarding).

**Step 3: Write PuzzleBoardView C# script**
Create `PuzzleBoardView.cs` (handling preview opacity toggle and fade-in completed image animation).

---

### Task 3: Scene Layout & Editor Setup Update

**Files:**
* Modify: `JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceSceneSetup.cs`
* Modify: `JigsawVina/Assets/JigsawVina/Scripts/Editor/TestRunnerHelper.cs`
* Modify: `JigsawVina/Assets/JigsawVina/Tests/JigsawVina.Tests.asmdef`

**Step 1: Modify `ThinVerticalSliceSceneSetup.cs` to support idempotent layout regeneration via Version Marker**
(Same as previously updated).

---

### Task 4: Puzzle Playing Presenter & View wiring

**Files:**
* Modify: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PuzzlePlayingView.cs`
* Modify: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PuzzlePlayingPresenter.cs`
* Modify: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/RewardSummaryPresenter.cs`
* Modify: `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/GameSessionService.cs`
* Modify: `JigsawVina/Assets/JigsawVina/Tests/ProgressionTests.cs`

**Step 1: Fix compiler errors in `ProgressionTests.cs`**
(Same as previously updated).

**Step 2: Modify `GameSessionService.cs`**
Expose `IsRewardProcessed` field to prevent duplicate reward triggers in the same session:
```csharp
// Modify in JigsawVina/Assets/JigsawVina/Scripts/Core/Services/GameSessionService.cs:
```
```csharp
namespace JigsawVina.Core.Services
{
    public class GameSessionService
    {
        public int SelectedPictureId { get; private set; }
        public int SelectedDifficultyId { get; private set; }
        public float LastElapsedTimeSeconds { get; set; }
        public int LastStarCount { get; set; }
        public bool IsRewardProcessed { get; set; }

        public void SetSelectedPicture(int pictureId)
        {
            SelectedPictureId = pictureId;
            IsRewardProcessed = false; // Reset processed flag when beginning a new picture
        }

        public void SetSelectedDifficulty(int difficultyId)
        {
            SelectedDifficultyId = difficultyId;
            IsRewardProcessed = false; // Reset processed flag when beginning a new difficulty
        }
    }
}
```

**Step 3: Modify `RewardSummaryPresenter.cs` to use single reward execution**
Update `ProcessRewardsAndDisplay` to guard reward distribution:
```csharp
// Modify in JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/RewardSummaryPresenter.cs:
```
```csharp
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;

namespace JigsawVina.Presentation.Screens
{
    public class RewardSummaryPresenter
    {
        private readonly RewardSummaryView _view;
        private readonly GameSessionService _sessionService;
        private readonly ISaveDataService _saveDataService;
        private readonly IStaticDataService _staticDataService;

        public RewardSummaryPresenter(
            RewardSummaryView view, 
            GameSessionService sessionService, 
            ISaveDataService saveDataService,
            IStaticDataService staticDataService)
        {
            _view = view;
            _sessionService = sessionService;
            _saveDataService = saveDataService;
            _staticDataService = staticDataService;
        }

        public void ProcessRewardsAndDisplay(float elapsedTimeSeconds)
        {
            var config = _staticDataService.GetPictureDifficulty(_sessionService.SelectedPictureId, _sessionService.SelectedDifficultyId);
            int stars = config.StarReward;
            int coins = stars * 10;

            _sessionService.LastStarCount = stars;
            _sessionService.LastElapsedTimeSeconds = elapsedTimeSeconds;

            if (!_sessionService.IsRewardProcessed)
            {
                _sessionService.IsRewardProcessed = true;

                // Save progression with Upsert Logic
                var save = _saveDataService.Load();
                save.Coins += coins;

                var existing = save.CompletedPuzzles.Find(p => 
                    p.PictureId == _sessionService.SelectedPictureId && 
                    p.DifficultyId == _sessionService.SelectedDifficultyId);

                if (existing != null)
                {
                    // Update with best records
                    if (_sessionService.LastElapsedTimeSeconds < existing.BestTimeSeconds || existing.BestTimeSeconds <= 0)
                    {
                        existing.BestTimeSeconds = _sessionService.LastElapsedTimeSeconds;
                    }
                    if (stars > existing.BestStar)
                    {
                        existing.BestStar = stars;
                    }
                }
                else
                {
                    save.CompletedPuzzles.Add(new CompletedPuzzleData
                    {
                        PictureId = _sessionService.SelectedPictureId,
                        DifficultyId = _sessionService.SelectedDifficultyId,
                        BestTimeSeconds = _sessionService.LastElapsedTimeSeconds,
                        BestStar = stars
                    });
                }

                _saveDataService.Save(save);
            }

            if (_view != null)
            {
                _view.DisplayReward(stars, coins);
            }
        }
    }
}
```

---

### Task 5: Win and Reward Flow Integration

**Files:**
* Modify: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/GameplayLifetimeScope.cs`
* Create: `JigsawVina/Assets/JigsawVina/Tests/PuzzleGameplayPlayModeTests.cs`

**Step 1: Coordinate asynchronous completed-picture fade-in animation and cancelable scene transitions**
(Same as Task 5 Step 1 previously updated).

**Step 2: Write PlayMode tests in `PuzzleGameplayPlayModeTests.cs`**
Create `PuzzleGameplayPlayModeTests.cs` validating piece initialization, return-to-tray, hint consumption, drag/snap lock assertions, and win state changes (interactivity lock, timer stop, summary display, single reward duplicate checks).
```csharp
using System.Collections;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;
using JigsawVina.Presentation.Screens;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace JigsawVina.Tests
{
    public class PuzzleGameplayPlayModeTests
    {
        private GameObject _root;
        private Canvas _canvas;
        private PuzzlePlayingView _view;
        private PuzzlePlayingPresenter _presenter;
        private GameSessionService _sessionService;
        private MockSaveDataService _saveService;
        private StaticDataService _staticDataService;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("TestRoot");
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(_root.transform);
            _canvas = canvasGo.GetComponent<Canvas>();

            // Setup Board Container (Position at world zero, size 800x600)
            var boardContainer = new GameObject("BoardContainer", typeof(RectTransform));
            boardContainer.transform.SetParent(_canvas.transform, false);
            var boardContainerRect = (RectTransform)boardContainer.transform;
            boardContainerRect.sizeDelta = new Vector2(800f, 600f);

            var boardGo = new GameObject("Board", typeof(RectTransform));
            boardGo.transform.SetParent(boardContainer.transform, false);
            var boardRect = (RectTransform)boardGo.transform;
            boardRect.sizeDelta = new Vector2(800f, 600f);
            boardGo.AddComponent<Image>();
            
            var previewObj = new GameObject("PreviewOverlay", typeof(RectTransform));
            previewObj.transform.SetParent(boardGo.transform, false);
            previewObj.AddComponent<Image>();

            var lockedObj = new GameObject("LockedPieces", typeof(RectTransform));
            lockedObj.transform.SetParent(boardGo.transform, false);
            var lockedRect = (RectTransform)lockedObj.transform;
            lockedRect.sizeDelta = new Vector2(800f, 600f);

            var boardView = boardGo.AddComponent<PuzzleBoardView>();
            AssignField(boardView, "_previewImage", previewObj.GetComponent<Image>());
            AssignField(boardView, "_lockedPiecesContainer", lockedRect);

            // Setup View Screen (canvas group)
            var viewGo = new GameObject("View", typeof(PuzzlePlayingView), typeof(CanvasGroup));
            viewGo.transform.SetParent(_canvas.transform, false);
            _view = viewGo.GetComponent<PuzzlePlayingView>();

            var trayContent = new GameObject("TrayContent", typeof(RectTransform));
            trayContent.transform.SetParent(viewGo.transform, false);

            var dragContainer = new GameObject("DragContainer", typeof(RectTransform));
            dragContainer.transform.SetParent(viewGo.transform, false);

            AssignField(_view, "_boardView", boardView);
            AssignField(_view, "_trayContent", (RectTransform)trayContent.transform);
            AssignField(_view, "_dragContainer", (RectTransform)dragContainer.transform);
            AssignField(_view, "_canvas", _canvas);

            // Services
            _sessionService = new GameSessionService();
            _sessionService.SetSelectedPicture(1);
            _sessionService.SetSelectedDifficulty(0); // Easy: 6x4 = 24

            _saveService = new MockSaveDataService();
            _staticDataService = new StaticDataService();

            _presenter = new PuzzlePlayingPresenter(_view, _sessionService, _staticDataService, _saveService);
        }

        [TearDown]
        public void TearDown()
        {
            _presenter.Cleanup();
            Object.DestroyImmediate(_root);
        }

        [UnityTest]
        public IEnumerator PuzzlePlay_InitializesCorrectPieceCount()
        {
            _presenter.Initialize();
            yield return null;

            Assert.AreEqual(24, _view.TrayContent.childCount);
        }

        [UnityTest]
        public IEnumerator PuzzlePlay_ReturnToTray_RestoresFloatingPieces()
        {
            _presenter.Initialize();
            yield return null;

            var piece0 = _view.TrayContent.GetChild(0);
            piece0.SetParent(_view.DragContainer, false);

            Assert.AreEqual(23, _view.TrayContent.childCount);

            TriggerEvent(_view, "OnReturnToTrayClicked");
            yield return null;

            Assert.AreEqual(24, _view.TrayContent.childCount);
        }

        [UnityTest]
        public IEnumerator PuzzlePlay_HintConsumption_LocksCorrectPiece()
        {
            _saveService.SaveData.Hints = 5;
            _presenter.Initialize();
            yield return null;

            TriggerEvent(_view, "OnHintClicked");
            yield return null;

            Assert.AreEqual(4, _saveService.Load().Hints);
            Assert.AreEqual(1, _view.BoardView.LockedPiecesContainer.childCount);
            
            var piece = _view.BoardView.LockedPiecesContainer.GetChild(0).GetComponent<PuzzlePieceView>();
            Assert.IsTrue(piece.IsLocked);
        }

        [UnityTest]
        public IEnumerator PuzzlePlay_SnapClosePiece_LocksAssertsCorrectly()
        {
            _presenter.Initialize();
            yield return null;

            var pieceView = _view.TrayContent.GetChild(0).GetComponent<PuzzlePieceView>();
            
            // Move piece to Board local target (calculated from bottom-left corner offset in board space)
            pieceView.transform.SetParent(_view.DragContainer, false);
            // Piece 0 target local is (-333.33f, -225f)
            pieceView.transform.position = _view.BoardView.RectTransform.TransformPoint(new Vector3(-333.33f, -225f, 0f));

            // Target size cell size Easy (6x4) inside 800x600 = Vector2(133.33f, 150f)
            Vector2 expectedCellSize = new Vector2(800f / 6f, 600f / 4f);

            // Trigger OnPieceDragEnd directly to force snap logic
            TriggerPieceDragEnd(pieceView, Vector2.zero);
            yield return null;

            // Assertions checking size layout, locked state, locked container parenting, and position snap
            Assert.IsTrue(pieceView.IsLocked);
            Assert.AreEqual(_view.BoardView.LockedPiecesContainer, pieceView.transform.parent);
            
            var rect = pieceView.GetComponent<RectTransform>();
            Assert.AreEqual(expectedCellSize.x, rect.sizeDelta.x, 0.1f);
            Assert.AreEqual(expectedCellSize.y, rect.sizeDelta.y, 0.1f);
        }

        [UnityTest]
        public IEnumerator PuzzlePlay_CompleteLifecycle_LocksInputTimerAndPersistsSingleRecord()
        {
            var rewardGo = new GameObject("RewardView", typeof(RewardSummaryView));
            rewardGo.transform.SetParent(_canvas.transform, false);
            var rewardView = rewardGo.GetComponent<RewardSummaryView>();

            var starsTextObj = new GameObject("StarsText", typeof(RectTransform));
            starsTextObj.transform.SetParent(rewardGo.transform, false);
            var starsText = starsTextObj.AddComponent<TMPro.TextMeshProUGUI>();

            var coinsTextObj = new GameObject("CoinsText", typeof(RectTransform));
            coinsTextObj.transform.SetParent(rewardGo.transform, false);
            var coinsText = coinsTextObj.AddComponent<TMPro.TextMeshProUGUI>();

            AssignField(rewardView, "_starsText", starsText);
            AssignField(rewardView, "_coinsText", coinsText);

            var rewardPresenter = new RewardSummaryPresenter(rewardView, _sessionService, _saveService, _staticDataService);

            // Initialize flow
            var flowController = new GameplayFlowController(_view, rewardView, _presenter, rewardPresenter, new SceneLoader());
            flowController.Start();
            yield return null;

            // Check that Tick() increases elapsed time before completion (yield to let frame time advance)
            float initialTime = _presenter.GetElapsedTime();
            yield return null;
            _presenter.Tick();
            float tickTime = _presenter.GetElapsedTime();
            Assert.IsTrue(tickTime > initialTime, $"Expected timer to increase during Tick, but tickTime was {tickTime}");
            
            // Trigger win condition by marking all piece data Locked in the presenter's active session
            var session = GetPrivateField<PuzzleSession>(_presenter, "_puzzleSession");
            for (int i = 0; i < session.PieceCount; i++)
            {
                session.Pieces[i].State = PuzzleSession.PieceState.Locked;
            }

            // Trigger final check snap callback to trigger win sequence
            var firstPiece = _view.TrayContent.GetChild(0).GetComponent<PuzzlePieceView>();
            firstPiece.transform.position = _view.BoardView.RectTransform.TransformPoint(new Vector3(-333.33f, -225f, 0f));
            TriggerPieceDragEnd(firstPiece, Vector2.zero);
            yield return null;

            // Check if controls are disabled on win
            var group = _view.GetComponent<CanvasGroup>();
            Assert.IsFalse(group.interactable);

            // Assert timer is stopped (Tick calls do not increase elapsed time after completion)
            float elapsedAtWin = _presenter.GetElapsedTime();
            _presenter.Tick();
            Assert.AreEqual(elapsedAtWin, _presenter.GetElapsedTime());

            // Wait for 1.5s delay animation (simulate remaining transition time)
            float t = 0f;
            while (t < 1.5f)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            // Assert Reward summary is displayed and single progression record written
            Assert.IsTrue(rewardView.gameObject.activeSelf);
            var save = _saveService.Load();
            Assert.AreEqual(1, save.CompletedPuzzles.Count);

            int coinsBefore = save.Coins;
            // Assert duplicate protection: process reward again, verifying it does NOT award extra coins or records
            rewardPresenter.ProcessRewardsAndDisplay(elapsedAtWin);
            
            var saveAfter = _saveService.Load();
            Assert.AreEqual(1, saveAfter.CompletedPuzzles.Count);
            Assert.AreEqual(coinsBefore, saveAfter.Coins); // Verified coins didn't double!
        }

        private static void AssignField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        private static T GetPrivateField<T>(object target, string name)
        {
            var field = target.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)field?.GetValue(target);
        }

        private static void TriggerEvent(object target, string name)
        {
            var eventInfo = target.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var del = eventInfo?.GetValue(target) as System.MulticastDelegate;
            del?.DynamicInvoke();
        }

        private static void TriggerPieceDragEnd(PuzzlePieceView pieceView, Vector2 pos)
        {
            var eventInfo = typeof(PuzzlePieceView).GetField("OnPieceDragEnd", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var del = eventInfo?.GetValue(pieceView) as System.MulticastDelegate;
            del?.DynamicInvoke(pieceView, pos);
        }
    }
}
```

---

## Verification Plan

### Automated Tests
Run EditMode & PlayMode Tests:
* Run menu item `JigsawVina/Run EditMode Tests` inside Unity via Unity MCP or manually.
* Run menu item `JigsawVina/Run PlayMode Tests` inside Unity via Unity MCP or manually.
* Expected tests count: 14 EditMode tests + 5 PlayMode tests = 19 total tests.
* Expected result: All 19 tests PASS.

### Manual Verification
1. Open Unity. Run the editor script menu `JigsawVina/Setup Thin Vertical Slice Scenes` to regenerate both Home and Gameplay scene hierarchies idempotently. Verify running it twice leaves scene SHA256 completely identical.
2. Enter Play Mode. Select `Ho Guom` -> `Easy`.
3. Verify that Canvas matches landscape layout scale correctly without stretching.
4. Verify Scroll View Scroll Rect scrolls vertically when sliding finger/cursor up and down on the pieces. Drag pieces sideways past 10px threshold to pull them out.
5. Drag and release a piece close to its slot. Verify it snaps and scales to fit the board cell exactly.
6. Trigger the "Gợi Ý" (Hint) button: check that it snaps the last interacted piece, or a random piece if none, decrementing the Hint counter in player save.
7. Complete all 24 pieces. Verify completion freezes input, triggers complete animation, waits 1.5 seconds, then presents the Reward Summary page.
8. Click "Return Home" and verify pieces are cleaned up cleanly.

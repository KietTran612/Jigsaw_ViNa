# Thin Vertical Slice Implementation Plan

> **For Antigravity:** REQUIRED WORKFLOW: Use `.agent/workflows/execute-plan.md` to execute this plan in single-flow mode.

**Goal:** Build a complete 2-scene flow (Home and Gameplay) with VContainer multi-scene DI, PlayerPrefs-based save/load, and mock gameplay logic.

**Architecture:** Use a persistent `ProjectLifetimeScope` for global services (`StaticData`, `SaveData`, `GameSession`), and separate scene lifetime scopes (`HomeLifetimeScope`, `GameplayLifetimeScope`) for UI and presenter logic.

**Tech Stack:** Unity 6000.3.11f1, uGUI, UniTask, VContainer, NUnit (Unity Test Framework).

---

## Plan Assembly Definition Files

We need Assembly Definition (`.asmdef`) files to resolve VContainer, UniTask, TMPro, and uGUI references cleanly.

### Task 0: Configure Assembly Definitions

**Files:**
- Create: `JigsawVina/Assets/JigsawVina/JigsawVina.asmdef`
- Create: `JigsawVina/Assets/JigsawVina/Tests/JigsawVina.Tests.asmdef`

**Step 1: Write Main Assembly Definition**
Create `JigsawVina.asmdef` (adding references to TextMeshPro and `Unity.ugui` assemblies):
```json
{
    "name": "JigsawVina",
    "rootNamespace": "JigsawVina",
    "references": [
        "UniTask",
        "VContainer",
        "Unity.TextMeshPro",
        "Unity.ugui"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

**Step 2: Write Test Assembly Definition**
Create `JigsawVina.Tests.asmdef` (setting `overrideReferences` to `false` and removing redundant manual precompiled DLL references since `TestAssemblies` handles NUnit natively):
```json
{
    "name": "JigsawVina.Tests",
    "rootNamespace": "JigsawVina.Tests",
    "references": [
        "JigsawVina",
        "UniTask",
        "VContainer",
        "Unity.TextMeshPro",
        "Unity.ugui"
    ],
    "optionalUnityReferences": [
        "TestAssemblies"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

**Step 3: Checkpoint**
*Optional checkpoint commit only when explicitly requested by user.*

---

## Tasks

### Task 1: Core Data Models & Save System

**Files:**
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PictureConfig.cs`
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PlayerSave.cs`
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/ISaveDataService.cs`
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/SaveDataService.cs`
- Create: `JigsawVina/Assets/JigsawVina/Tests/SaveDataServiceTests.cs`

**Step 1: Write the failing test**
Create `SaveDataServiceTests.cs` with tests verifying:
- SaveDataService loads empty default profile if no PlayerPrefs key exists.
- SaveDataService serializes and deserializes completed puzzles correctly.

```csharp
using NUnit.Framework;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;
using UnityEngine;

namespace JigsawVina.Tests
{
    public class SaveDataServiceTests
    {
        [SetUp]
        public void Setup()
        {
            PlayerPrefs.DeleteKey(SaveDataService.SaveKey);
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(SaveDataService.SaveKey);
            PlayerPrefs.Save();
        }

        [Test]
        public void Load_WhenNoSaveExists_ReturnsDefaultSave()
        {
            var service = new SaveDataService();
            var save = service.Load();
            Assert.AreEqual(0, save.Coins);
            Assert.AreEqual(0, save.CompletedPuzzles.Count);
        }

        [Test]
        public void SaveAndLoad_SavesCorrectData()
        {
            var service = new SaveDataService();
            var save = service.Load();
            save.Coins = 100;
            save.CompletedPuzzles.Add(new CompletedPuzzleData { PictureId = 1, DifficultyId = 0, BestTimeSeconds = 45f, BestStar = 3 });
            service.Save(save);

            var loadedSave = service.Load();
            Assert.AreEqual(100, loadedSave.Coins);
            Assert.AreEqual(1, loadedSave.CompletedPuzzles.Count);
            Assert.AreEqual(45f, loadedSave.CompletedPuzzles[0].BestTimeSeconds);
        }
    }
}
```

**Step 2: Run test to verify it fails**
Expected: Compile error (SaveDataService and types do not exist yet).

**Step 3: Write minimal implementation**
Create `PictureConfig.cs`:
```csharp
namespace JigsawVina.Core.Data
{
    public readonly struct PictureConfig
    {
        public readonly int Id;
        public readonly string IdString;
        public readonly string DisplayName;
        public readonly string AssetPath;

        public PictureConfig(int id, string idString, string displayName, string assetPath)
        {
            Id = id;
            IdString = idString;
            DisplayName = displayName;
            AssetPath = assetPath;
        }
    }
}
```

Create `PlayerSave.cs`:
```csharp
using System;
using System.Collections.Generic;

namespace JigsawVina.Core.Data
{
    [Serializable]
    public class CompletedPuzzleData
    {
        public int PictureId;
        public int DifficultyId;
        public float BestTimeSeconds;
        public int BestStar;
    }

    [Serializable]
    public class PlayerSave
    {
        public int Coins;
        public int Hints;
        public List<CompletedPuzzleData> CompletedPuzzles = new();
    }
}
```

Create `ISaveDataService.cs`:
```csharp
using JigsawVina.Core.Data;

namespace JigsawVina.Core.Services
{
    public interface ISaveDataService
    {
        PlayerSave Load();
        void Save(PlayerSave save);
    }
}
```

Create `SaveDataService.cs` (exposing `SaveKey` publicly and avoiding DeleteAll):
```csharp
using JigsawVina.Core.Data;
using UnityEngine;

namespace JigsawVina.Core.Services
{
    public class SaveDataService : ISaveDataService
    {
        public const string SaveKey = "JigsawVina_PlayerSave";

        public PlayerSave Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                return new PlayerSave();
            }
            string json = PlayerPrefs.GetString(SaveKey);
            return JsonUtility.FromJson<PlayerSave>(json) ?? new PlayerSave();
        }

        public void Save(PlayerSave save)
        {
            string json = JsonUtility.ToJson(save);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }
    }
}
```

**Step 4: Run test to verify it passes**
Expected: PASS.

**Step 5: Checkpoint**
*Optional checkpoint commit only when explicitly requested by user.*

---

### Task 2: Global Services & Shared Session State

**Files:**
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/IStaticDataService.cs`
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/StaticDataService.cs`
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/GameSessionService.cs`
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/SceneLoader.cs`
- Create: `JigsawVina/Assets/JigsawVina/Tests/GameSessionServiceTests.cs`

**Step 1: Write the failing test**
Create `GameSessionServiceTests.cs` to test starting, completing, and checking if data can be carried over.
```csharp
using NUnit.Framework;
using JigsawVina.Core.Services;

namespace JigsawVina.Tests
{
    public class GameSessionServiceTests
    {
        [Test]
        public void Session_StoresCorrectly()
        {
            var session = new GameSessionService();
            session.SetSelectedPicture(5);
            session.SetSelectedDifficulty(1); // Normal

            Assert.AreEqual(5, session.SelectedPictureId);
            Assert.AreEqual(1, session.SelectedDifficultyId);
        }
    }
}
```

**Step 2: Run test to verify it fails**
Compile will fail as classes do not exist.

**Step 3: Write minimal implementation**
Create `IStaticDataService.cs`:
```csharp
using System.Collections.Generic;
using JigsawVina.Core.Data;

namespace JigsawVina.Core.Services
{
    public interface IStaticDataService
    {
        IReadOnlyList<PictureConfig> GetAllPictures();
        PictureConfig GetPictureById(int id);
    }
}
```

Create `StaticDataService.cs` (using clean Unicode escapes for non-ASCII characters):
```csharp
using System.Collections.Generic;
using System.Linq;
using JigsawVina.Core.Data;

namespace JigsawVina.Core.Services
{
    public class StaticDataService : IStaticDataService
    {
        private readonly List<PictureConfig> _pictures = new()
        {
            // Picture 1: Ho Guom, Picture 2: Vinh Ha Long
            new PictureConfig(1, "ho_guom", "H\u1ed3 G\u01b0\u01a1m", "Textures/ho_guom"),
            new PictureConfig(2, "ha_long", "V\u1ecbnh H\u1ea1 Long", "Textures/ha_long")
        };

        public IReadOnlyList<PictureConfig> GetAllPictures() => _pictures;

        public PictureConfig GetPictureById(int id)
        {
            return _pictures.FirstOrDefault(p => p.Id == id);
        }
    }
}
```

Create `GameSessionService.cs`:
```csharp
namespace JigsawVina.Core.Services
{
    public class GameSessionService
    {
        public int SelectedPictureId { get; private set; }
        public int SelectedDifficultyId { get; private set; }
        public float LastElapsedTimeSeconds { get; set; }
        public int LastStarCount { get; set; }

        public void SetSelectedPicture(int pictureId)
        {
            SelectedPictureId = pictureId;
        }

        public void SetSelectedDifficulty(int difficultyId)
        {
            SelectedDifficultyId = difficultyId;
        }
    }
}
```

Create `SceneLoader.cs` (handling async transitions with UniTask):
```csharp
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace JigsawVina.Core.Services
{
    public class SceneLoader
    {
        public async UniTask LoadSceneAsync(string sceneName)
        {
            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op == null) return;
            await op.ToUniTask();
        }
    }
}
```

**Step 4: Run test to verify it passes**
Expected: PASS.

**Step 5: Checkpoint**
*Optional checkpoint commit only when explicitly requested by user.*

---

### Task 3: VContainer Project Scope

**Files:**
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/App/ProjectLifetimeScope.cs`

**Step 1: Write ProjectLifetimeScope C# Script**
Create `ProjectLifetimeScope.cs`:
```csharp
using VContainer;
using VContainer.Unity;
using JigsawVina.Core.Services;

namespace JigsawVina.Presentation.App
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<SaveDataService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<StaticDataService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<GameSessionService>(Lifetime.Singleton);
            builder.Register<SceneLoader>(Lifetime.Singleton);
        }
    }
}
```

**Step 2: Checkpoint**
*Optional checkpoint commit only when explicitly requested by user.*

---

### Task 4: Home Scene UI & Presenters

**Files:**
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureSelectView.cs`
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureSelectPresenter.cs`
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/DifficultySelectView.cs`
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/DifficultySelectPresenter.cs`
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/HomeLifetimeScope.cs`

**Step 1: Write Views & Presenters**
Create `PictureSelectView.cs`:
```csharp
using System;
using UnityEngine;
using UnityEngine.UI;

namespace JigsawVina.Presentation.Screens
{
    public class PictureSelectView : MonoBehaviour
    {
        public event Action<int> OnPictureSelected;

        [SerializeField] private Button _pic1Button;
        [SerializeField] private Button _pic2Button;

        private void Start()
        {
            if (_pic1Button != null) _pic1Button.onClick.AddListener(() => OnPictureSelected?.Invoke(1));
            if (_pic2Button != null) _pic2Button.onClick.AddListener(() => OnPictureSelected?.Invoke(2));
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}
```

Create `PictureSelectPresenter.cs`:
```csharp
using JigsawVina.Core.Services;

namespace JigsawVina.Presentation.Screens
{
    public class PictureSelectPresenter
    {
        private readonly PictureSelectView _view;
        private readonly GameSessionService _sessionService;

        public PictureSelectPresenter(PictureSelectView view, GameSessionService sessionService)
        {
            _view = view;
            _sessionService = sessionService;
            _view.OnPictureSelected += HandlePictureSelected;
        }

        private void HandlePictureSelected(int pictureId)
        {
            _sessionService.SetSelectedPicture(pictureId);
        }
    }
}
```

Create `DifficultySelectView.cs`:
```csharp
using System;
using UnityEngine;
using UnityEngine.UI;

namespace JigsawVina.Presentation.Screens
{
    public class DifficultySelectView : MonoBehaviour
    {
        public event Action<int> OnDifficultySelected;

        [SerializeField] private Button _easyButton;
        [SerializeField] private Button _normalButton;
        [SerializeField] private Button _hardButton;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;

        private void Start()
        {
            if (_easyButton != null) _easyButton.onClick.AddListener(() => OnDifficultySelected?.Invoke(0)); // 0: Easy
            if (_normalButton != null) _normalButton.onClick.AddListener(() => OnDifficultySelected?.Invoke(1)); // 1: Normal
            if (_hardButton != null) _hardButton.onClick.AddListener(() => OnDifficultySelected?.Invoke(2)); // 2: Hard
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}
```

Create `DifficultySelectPresenter.cs` (including Cysharp.Threading.Tasks namespace import):
```csharp
using Cysharp.Threading.Tasks;
using JigsawVina.Core.Services;

namespace JigsawVina.Presentation.Screens
{
    public class DifficultySelectPresenter
    {
        private readonly DifficultySelectView _view;
        private readonly GameSessionService _sessionService;
        private readonly SceneLoader _sceneLoader;

        public DifficultySelectPresenter(DifficultySelectView view, GameSessionService sessionService, SceneLoader sceneLoader)
        {
            _view = view;
            _sessionService = sessionService;
            _sceneLoader = sceneLoader;
            _view.OnDifficultySelected += HandleDifficultySelected;
        }

        private void HandleDifficultySelected(int difficultyId)
        {
            _sessionService.SetSelectedDifficulty(difficultyId);
            _sceneLoader.LoadSceneAsync("Gameplay").Forget();
        }
    }
}
```

Create `HomeLifetimeScope.cs` (handling unused injected fields to suppress warnings):
```csharp
using VContainer;
using VContainer.Unity;

namespace JigsawVina.Presentation.Screens
{
    public class HomeLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<PictureSelectView>();
            builder.RegisterComponentInHierarchy<DifficultySelectView>();

            builder.Register<PictureSelectPresenter>(Lifetime.Singleton);
            builder.Register<DifficultySelectPresenter>(Lifetime.Singleton);
            
            builder.RegisterEntryPoint<HomeFlowController>();
        }
    }

    public class HomeFlowController : IStartable
    {
        private readonly PictureSelectView _picView;
        private readonly DifficultySelectView _diffView;

        public HomeFlowController(
            PictureSelectView picView, 
            DifficultySelectView diffView,
            PictureSelectPresenter picPresenter, // Injected to force instantiation via DI
            DifficultySelectPresenter diffPresenter) // Injected to force instantiation via DI
        {
            _picView = picView;
            _diffView = diffView;

            // Discard assignments to suppress IDE unused variable warnings
            _ = picPresenter;
            _ = diffPresenter;
        }

        public void Start()
        {
            _picView.SetActive(true);
            _diffView.SetActive(false);

            _picView.OnPictureSelected += _ =>
            {
                _picView.SetActive(false);
                _diffView.SetActive(true);
            };

            _diffView.BackButton.onClick.AddListener(() =>
            {
                _diffView.SetActive(false);
                _picView.SetActive(true);
            });
        }
    }
}
```

**Step 2: Checkpoint**
*Optional checkpoint commit only when explicitly requested by user.*

---

### Task 5: Gameplay Scene UI, Progression Logic & Tests

**Files:**
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PuzzlePlayingView.cs`
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PuzzlePlayingPresenter.cs`
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/RewardSummaryView.cs`
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/RewardSummaryPresenter.cs`
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/GameplayLifetimeScope.cs`
- Create: `JigsawVina/Assets/JigsawVina/Tests/ProgressionTests.cs`

**Step 1: Write progression logic failing tests**
Create `ProgressionTests.cs` verifying rewards and duplicate-prevention upsert logic (since this test class relies purely on an in-memory `MockSaveDataService`, PlayerPrefs setup and teardown are completely omitted here):
```csharp
using NUnit.Framework;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;
using JigsawVina.Presentation.Screens;

namespace JigsawVina.Tests
{
    public class MockSaveDataService : ISaveDataService
    {
        public PlayerSave SaveData = new();
        public PlayerSave Load() => SaveData;
        public void Save(PlayerSave save) => SaveData = save;
    }

    public class ProgressionTests
    {
        [Test]
        public void ProcessRewards_FirstClear_AddsRecord()
        {
            var saveService = new MockSaveDataService();
            var session = new GameSessionService();
            session.SetSelectedPicture(1);
            session.SetSelectedDifficulty(1); // Normal (2 stars)

            var presenter = new RewardSummaryPresenter(null, session, saveService);
            presenter.ProcessRewardsAndDisplay(12f); // Pass 12 seconds elapsed time

            var save = saveService.Load();
            Assert.AreEqual(1, save.CompletedPuzzles.Count);
            Assert.AreEqual(1, save.CompletedPuzzles[0].PictureId);
            Assert.AreEqual(1, save.CompletedPuzzles[0].DifficultyId);
            Assert.AreEqual(2, save.CompletedPuzzles[0].BestStar);
            Assert.AreEqual(12f, save.CompletedPuzzles[0].BestTimeSeconds);
        }

        [Test]
        public void ProcessRewards_ReplayWorseScore_DoesNotOverwriteBestRecord()
        {
            var saveService = new MockSaveDataService();
            saveService.SaveData.CompletedPuzzles.Add(new CompletedPuzzleData
            {
                PictureId = 1,
                DifficultyId = 1,
                BestTimeSeconds = 10f,
                BestStar = 2
            });

            var session = new GameSessionService();
            session.SetSelectedPicture(1);
            session.SetSelectedDifficulty(1);

            var presenter = new RewardSummaryPresenter(null, session, saveService);
            presenter.ProcessRewardsAndDisplay(20f); // 20s (slower than 10s)

            var save = saveService.Load();
            Assert.AreEqual(1, save.CompletedPuzzles.Count);
            Assert.AreEqual(10f, save.CompletedPuzzles[0].BestTimeSeconds); // keeps 10s
        }

        [Test]
        public void ProcessRewards_ReplayBetterScore_UpdatesBestRecord()
        {
            var saveService = new MockSaveDataService();
            saveService.SaveData.CompletedPuzzles.Add(new CompletedPuzzleData
            {
                PictureId = 1,
                DifficultyId = 1,
                BestTimeSeconds = 30f,
                BestStar = 1
            });

            var session = new GameSessionService();
            session.SetSelectedPicture(1);
            session.SetSelectedDifficulty(1); // Normal (2 stars)

            var presenter = new RewardSummaryPresenter(null, session, saveService);
            presenter.ProcessRewardsAndDisplay(15f); // 15s (faster than 30s) and 2 stars (higher than 1)

            var save = saveService.Load();
            Assert.AreEqual(1, save.CompletedPuzzles.Count);
            Assert.AreEqual(15f, save.CompletedPuzzles[0].BestTimeSeconds); // updated to 15s
            Assert.AreEqual(2, save.CompletedPuzzles[0].BestStar); // updated to 2 stars
        }
    }
}
```

**Step 2: Run test to verify it fails**
Compile will fail as Gameplay scripts are not written yet.

**Step 3: Write minimal implementation**
Create `PuzzlePlayingView.cs`:
```csharp
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace JigsawVina.Presentation.Screens
{
    public class PuzzlePlayingView : MonoBehaviour
    {
        public event Action OnCheatWinClicked;

        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private Button _cheatWinButton;

        private void Start()
        {
            if (_cheatWinButton != null) _cheatWinButton.onClick.AddListener(() => OnCheatWinClicked?.Invoke());
        }

        public void Setup(string pictureName, string difficultyName)
        {
            if (_titleText != null) _titleText.text = $"Playing: {pictureName} ({difficultyName})";
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}
```

Create `PuzzlePlayingPresenter.cs`:
```csharp
using JigsawVina.Core.Services;

namespace JigsawVina.Presentation.Screens
{
    public class PuzzlePlayingPresenter
    {
        private readonly PuzzlePlayingView _view;
        private readonly GameSessionService _sessionService;
        private readonly IStaticDataService _staticDataService;

        public PuzzlePlayingPresenter(PuzzlePlayingView view, GameSessionService sessionService, IStaticDataService staticDataService)
        {
            _view = view;
            _sessionService = sessionService;
            _staticDataService = staticDataService;
        }

        public void Initialize()
        {
            var picture = _staticDataService.GetPictureById(_sessionService.SelectedPictureId);
            string diffName = _sessionService.SelectedDifficultyId switch
            {
                0 => "Easy (24 pieces)",
                1 => "Normal (48 pieces)",
                2 => "Hard (96 pieces)",
                _ => "Debug"
            };
            _view.Setup(picture.DisplayName ?? "Unknown", diffName);
        }
    }
}
```

Create `RewardSummaryView.cs`:
```csharp
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace JigsawVina.Presentation.Screens
{
    public class RewardSummaryView : MonoBehaviour
    {
        public event Action OnReturnClicked;

        [SerializeField] private TMP_Text _starsText;
        [SerializeField] private TMP_Text _coinsText;
        [SerializeField] private Button _returnButton;

        private void Start()
        {
            if (_returnButton != null) _returnButton.onClick.AddListener(() => OnReturnClicked?.Invoke());
        }

        public void DisplayReward(int stars, int coins)
        {
            if (_starsText != null) _starsText.text = $"Stars: {stars}";
            if (_coinsText != null) _coinsText.text = $"Coins Earned: {coins}";
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}
```

Create `RewardSummaryPresenter.cs` (implementing progression upsert logic utilizing parameter-passed elapsed time):
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

        public RewardSummaryPresenter(RewardSummaryView view, GameSessionService sessionService, ISaveDataService saveDataService)
        {
            _view = view;
            _sessionService = sessionService;
            _saveDataService = saveDataService;
        }

        public void ProcessRewardsAndDisplay(float elapsedTimeSeconds)
        {
            int stars = _sessionService.SelectedDifficultyId switch
            {
                0 => 1,
                1 => 2,
                2 => 3,
                _ => 1
            };
            int coins = stars * 10;

            _sessionService.LastStarCount = stars;
            _sessionService.LastElapsedTimeSeconds = elapsedTimeSeconds;

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
            if (_view != null)
            {
                _view.DisplayReward(stars, coins);
            }
        }
    }
}
```

Create `GameplayLifetimeScope.cs` (including Cysharp.Threading.Tasks namespace import):
```csharp
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace JigsawVina.Presentation.Screens
{
    public class GameplayLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<PuzzlePlayingView>();
            builder.RegisterComponentInHierarchy<RewardSummaryView>();

            builder.Register<PuzzlePlayingPresenter>(Lifetime.Singleton);
            builder.Register<RewardSummaryPresenter>(Lifetime.Singleton);

            builder.RegisterEntryPoint<GameplayFlowController>();
        }
    }

    public class GameplayFlowController : IStartable
    {
        private readonly PuzzlePlayingView _playView;
        private readonly RewardSummaryView _rewardView;
        private readonly PuzzlePlayingPresenter _playPresenter;
        private readonly RewardSummaryPresenter _rewardPresenter;
        private readonly SceneLoader _sceneLoader;

        public GameplayFlowController(
            PuzzlePlayingView playView,
            RewardSummaryView rewardView,
            PuzzlePlayingPresenter playPresenter,
            RewardSummaryPresenter rewardPresenter,
            SceneLoader sceneLoader)
        {
            _playView = playView;
            _rewardView = rewardView;
            _playPresenter = playPresenter;
            _rewardPresenter = rewardPresenter;
            _sceneLoader = sceneLoader;
        }

        public void Start()
        {
            _playView.SetActive(true);
            _rewardView.SetActive(false);
            _playPresenter.Initialize();

            _playView.OnCheatWinClicked += () =>
            {
                _playView.SetActive(false);
                _rewardView.SetActive(true);
                _rewardPresenter.ProcessRewardsAndDisplay(15f); // Pass 15s mock time
            };

            _rewardView.OnReturnClicked += () =>
            {
                _sceneLoader.LoadSceneAsync("Home").Forget();
            };
        }
    }
}
```

**Step 4: Run tests to verify they pass**
Expected: PASS.

**Step 5: Checkpoint**
*Optional checkpoint commit only when explicitly requested by user.*

---

### Task 6: Unity Scene Wire Up & Manual Run

**Files:**
- Create: Unity scenes `Home.unity` and `Gameplay.unity` (configured inside `JigsawVina/Assets/Scenes/`)
- Modify: Project Settings (Assign `ProjectLifetimeScope` prefab under VContainer Settings; add `Home` and `Gameplay` scenes to Build Settings).

**Step 1: Configure VContainer & Build Settings**
1. Create a `ProjectLifetimeScope` Prefab by adding the component to a GameObject and saving it as a Prefab.
2. Open Unity's **Project Settings** -> **VContainer** and drag this Prefab into the **Project Connection** field. This maps VContainer to automatically parent all scene scopes to this Project Scope.
3. Open Unity's **Build Settings** (`File -> Build Settings`) and add the two scenes:
   - Index 0: `JigsawVina/Assets/Scenes/Home.unity`
   - Index 1: `JigsawVina/Assets/Scenes/Gameplay.unity`

**Step 2: Setup Home Scene Hierarchy**
Open `Home.unity`:
1. Create a VContainer `LifetimeScope` GameObject, attach `HomeLifetimeScope` component to it.
2. Create a Canvas with:
   - `PictureSelectScreen` GameObject (Attach `PictureSelectView` script, configure Button serializable fields).
   - `DifficultySelectScreen` GameObject (Attach `DifficultySelectView` script, configure Button serializable fields).
3. Ensure both View components exist directly in the hierarchy of the scene so that VContainer's `RegisterComponentInHierarchy` can resolve them.

**Step 3: Setup Gameplay Scene Hierarchy**
Open `Gameplay.unity`:
1. Create a VContainer `LifetimeScope` GameObject, attach `GameplayLifetimeScope` component to it.
2. Create a Canvas with:
   - `PuzzlePlayingScreen` GameObject (Attach `PuzzlePlayingView` script, configure fields).
   - `RewardSummaryScreen` GameObject (Attach `RewardSummaryView` script, configure fields).
3. Ensure both View components exist directly in the hierarchy of the scene so that VContainer's `RegisterComponentInHierarchy` can resolve them.

**Step 4: Manual Verification Flow**
1. Enter Play Mode starting from the `Home` scene.
2. Select picture, choose difficulty.
3. Verify scene loads `Gameplay`.
4. Click **Cheat Win** on the playing panel.
5. Verify rewards display correct star counts (Easy = 1, Normal = 2, Hard = 3).
6. Click **Return to Home**. Verify it goes back.

**Step 5: Checkpoint**
*Optional checkpoint commit only when explicitly requested by user.*
*(Ensure compilation is finished and all required .meta files are generated before requesting a commit).*

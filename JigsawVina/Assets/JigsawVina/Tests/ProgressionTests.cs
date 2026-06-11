using JigsawVina.Core.Data;
using JigsawVina.Core.Services;
using JigsawVina.Presentation.Screens;
using NUnit.Framework;

namespace JigsawVina.Tests
{
    public class MockSaveDataService : ISaveDataService
    {
        public PlayerSave SaveData = new();

        public PlayerSave Load()
        {
            return SaveData;
        }

        public void Save(PlayerSave save)
        {
            SaveData = save;
        }
    }

    public class ProgressionTests
    {
        [Test]
        public void ProcessRewards_FirstClear_AddsRecord()
        {
            var saveService = new MockSaveDataService();
            var session = new GameSessionService();
            session.SetSelectedPicture(1);
            session.SetSelectedDifficulty(1);

            var staticDataService = new StaticDataService();
            var presenter = new RewardSummaryPresenter(null, session, saveService, staticDataService);
            presenter.ProcessRewardsAndDisplay(12f);

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

            var staticDataService = new StaticDataService();
            var presenter = new RewardSummaryPresenter(null, session, saveService, staticDataService);
            presenter.ProcessRewardsAndDisplay(20f);

            var save = saveService.Load();
            Assert.AreEqual(1, save.CompletedPuzzles.Count);
            Assert.AreEqual(10f, save.CompletedPuzzles[0].BestTimeSeconds);
            Assert.AreEqual(2, save.CompletedPuzzles[0].BestStar);
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
            session.SetSelectedDifficulty(1);

            var staticDataService = new StaticDataService();
            var presenter = new RewardSummaryPresenter(null, session, saveService, staticDataService);
            presenter.ProcessRewardsAndDisplay(15f);

            var save = saveService.Load();
            Assert.AreEqual(1, save.CompletedPuzzles.Count);
            Assert.AreEqual(15f, save.CompletedPuzzles[0].BestTimeSeconds);
            Assert.AreEqual(2, save.CompletedPuzzles[0].BestStar);
        }
    }
}

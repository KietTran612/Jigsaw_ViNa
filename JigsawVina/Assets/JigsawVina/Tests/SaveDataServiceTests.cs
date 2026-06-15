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

        [Test]
        public void Load_DailyDropCounts_ResetsOnDateChange()
        {
            var mockDateProvider = new MockLocalDateProvider { DateString = "2026-06-15" };
            var service = new SaveDataService(mockDateProvider);

            // Create and save a state with daily drop counts
            var save = service.Load();
            save.DailyDropCounts.Add(new DailyDropCount { ItemId = 10, Count = 5 });
            service.Save(save);

            // Load again on the SAME date, verify counts are preserved
            var loadedSameDay = service.Load();
            Assert.AreEqual("2026-06-15", loadedSameDay.LastSaveDateString);
            Assert.AreEqual(1, loadedSameDay.DailyDropCounts.Count);
            Assert.AreEqual(10, loadedSameDay.DailyDropCounts[0].ItemId);
            Assert.AreEqual(5, loadedSameDay.DailyDropCounts[0].Count);

            // Load on a DIFFERENT date, verify counts are cleared
            mockDateProvider.DateString = "2026-06-16";
            var loadedNextDay = service.Load();
            Assert.AreEqual("2026-06-16", loadedNextDay.LastSaveDateString);
            Assert.AreEqual(0, loadedNextDay.DailyDropCounts.Count);
        }

        private class MockLocalDateProvider : ILocalDateProvider
        {
            public string DateString;
            public string GetCurrentLocalDateString() => DateString;
        }
    }
}

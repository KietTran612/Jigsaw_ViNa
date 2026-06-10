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

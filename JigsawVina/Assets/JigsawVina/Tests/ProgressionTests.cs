using System.Collections.Generic;
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

        [Test]
        public void ProcessRewards_FirstClear_AwardsFirstClearCoinsHintsAndItems()
        {
            var saveService = new MockSaveDataService();
            saveService.SaveData.Coins = 100;
            saveService.SaveData.Hints = 2;

            var session = new GameSessionService();
            session.SetSelectedPicture(1);
            session.SetSelectedDifficulty(1);

            var staticDataService = new StaticDataService(false);
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""categories"": [
                    { ""id"": 1, ""id_string"": ""vietnam_landscapes"", ""display_name"": ""Phong Cảnh Việt Nam"" }
                ],
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1, ""asset_path"": ""Textures/pic1"" }
                ],
                ""items"": [
                    { ""id"": 101, ""id_string"": ""item1"", ""display_name"": ""Item 1"", ""item_type"": ""key_item"", ""asset_path"": ""Items/item1"" }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 1, ""display_name"": ""Medium"", ""grid_columns"": 8, ""grid_rows"": 6, ""piece_count"": 48, ""star_reward"": 2, ""first_clear_coin"": 60, ""first_clear_hint"": 3, ""replay_coin"": 20, ""first_clear_reward_item_ids"": [101] }
                ]
            }";
            staticDataService.LoadFromText(json);

            var presenter = new RewardSummaryPresenter(null, session, saveService, staticDataService);
            presenter.ProcessRewardsAndDisplay(12f);

            var save = saveService.Load();
            Assert.AreEqual(1, save.CompletedPuzzles.Count);
            Assert.AreEqual(100 + 60, save.Coins); // 100 base + 60 first clear
            Assert.AreEqual(2 + 3, save.Hints);   // 2 base + 3 hints
            Assert.Contains(101, save.OwnedItemIds); // Rewarded item
            Assert.AreEqual(60, session.LastCoinEarned);
        }

        [Test]
        public void ProcessRewards_Replay_AwardsReplayCoinsOnly()
        {
            var saveService = new MockSaveDataService();
            saveService.SaveData.Coins = 100;
            saveService.SaveData.Hints = 2;
            saveService.SaveData.OwnedItemIds = new List<int> { 101 };
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

            var staticDataService = new StaticDataService(false);
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""categories"": [
                    { ""id"": 1, ""id_string"": ""vietnam_landscapes"", ""display_name"": ""Phong Cảnh Việt Nam"" }
                ],
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1, ""asset_path"": ""Textures/pic1"" }
                ],
                ""items"": [
                    { ""id"": 101, ""id_string"": ""item1"", ""display_name"": ""Item 1"", ""item_type"": ""key_item"", ""asset_path"": ""Items/item1"" }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 1, ""display_name"": ""Medium"", ""grid_columns"": 8, ""grid_rows"": 6, ""piece_count"": 48, ""star_reward"": 2, ""first_clear_coin"": 60, ""first_clear_hint"": 3, ""replay_coin"": 20, ""first_clear_reward_item_ids"": [101] }
                ]
            }";
            staticDataService.LoadFromText(json);

            var presenter = new RewardSummaryPresenter(null, session, saveService, staticDataService);
            presenter.ProcessRewardsAndDisplay(15f);

            var save = saveService.Load();
            Assert.AreEqual(1, save.CompletedPuzzles.Count);
            Assert.AreEqual(100 + 20, save.Coins); // Replay coin only
            Assert.AreEqual(2, save.Hints);        // Hints not awarded on replay
            Assert.AreEqual(20, session.LastCoinEarned);
        }
    }
}

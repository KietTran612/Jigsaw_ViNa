using System;
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
        public int SaveCallCount;

        public PlayerSave Load()
        {
            return SaveData;
        }

        public void Save(PlayerSave save)
        {
            SaveData = save;
            SaveCallCount++;
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
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1, ""asset_path"": ""Textures/pic1"", ""is_initially_unlocked"": true, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [] }
                ],
                ""items"": [
                    { ""id"": 101, ""id_string"": ""item1"", ""display_name"": ""Item 1"", ""item_type"": ""key_item"", ""is_consumable"": false, ""status"": ""active"", ""asset_path"": ""Items/item1"" }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24 },
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
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1, ""asset_path"": ""Textures/pic1"", ""is_initially_unlocked"": true, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [] }
                ],
                ""items"": [
                    { ""id"": 101, ""id_string"": ""item1"", ""display_name"": ""Item 1"", ""item_type"": ""key_item"", ""is_consumable"": false, ""status"": ""active"", ""asset_path"": ""Items/item1"" }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24 },
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

        [Test]
        public void PlayerSave_Normalize_InitializesNullLists()
        {
            var save = new PlayerSave
            {
                CompletedPuzzles = null,
                OwnedItemIds = null,
                UnlockedPictureIds = null
            };

            save.Normalize();

            Assert.IsNotNull(save.CompletedPuzzles);
            Assert.IsNotNull(save.OwnedItemIds);
            Assert.IsNotNull(save.UnlockedPictureIds);
        }

        private string GetMockStaticDataJson()
        {
            return @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""categories"": [
                    { ""id"": 1, ""id_string"": ""cat1"", ""display_name"": ""Cat 1"" }
                ],
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1, ""is_initially_unlocked"": true, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [] },
                    { ""id"": 2, ""id_string"": ""pic2"", ""display_name"": ""Pic 2"", ""category_id"": 1, ""is_initially_unlocked"": false, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [101] },
                    { ""id"": 3, ""id_string"": ""pic3"", ""display_name"": ""Pic 3"", ""category_id"": 1, ""is_initially_unlocked"": false, ""difficulty_unlock_policy"": ""all_unlocked"", ""unlock_requirements"": [102] }
                ],
                ""items"": [
                    { ""id"": 101, ""id_string"": ""key1"", ""display_name"": ""Key 1"", ""item_type"": ""key_item"", ""is_consumable"": false, ""status"": ""active"", ""asset_path"": """" },
                    { ""id"": 102, ""id_string"": ""key2"", ""display_name"": ""Key 2"", ""item_type"": ""key_item"", ""is_consumable"": false, ""status"": ""active"", ""asset_path"": """" },
                    { ""id"": 103, ""id_string"": ""key3"", ""display_name"": ""Key 3"", ""item_type"": ""key_item"", ""is_consumable"": false, ""status"": ""active"", ""asset_path"": """" }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24, ""star_reward"": 1, ""first_clear_reward_item_ids"": [101] },
                    { ""picture_id"": 1, ""difficulty_id"": 1, ""display_name"": ""Normal"", ""grid_columns"": 8, ""grid_rows"": 6, ""piece_count"": 48, ""star_reward"": 2 },
                    { ""picture_id"": 1, ""difficulty_id"": 2, ""display_name"": ""Hard"", ""grid_columns"": 12, ""grid_rows"": 8, ""piece_count"": 96, ""star_reward"": 3 },
                    { ""picture_id"": 2, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24, ""star_reward"": 1, ""first_clear_reward_item_ids"": [102] },
                    { ""picture_id"": 2, ""difficulty_id"": 1, ""display_name"": ""Normal"", ""grid_columns"": 8, ""grid_rows"": 6, ""piece_count"": 48, ""star_reward"": 2 },
                    { ""picture_id"": 2, ""difficulty_id"": 2, ""display_name"": ""Hard"", ""grid_columns"": 12, ""grid_rows"": 8, ""piece_count"": 96, ""star_reward"": 3 },
                    { ""picture_id"": 3, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24, ""star_reward"": 1 },
                    { ""picture_id"": 3, ""difficulty_id"": 1, ""display_name"": ""Normal"", ""grid_columns"": 8, ""grid_rows"": 6, ""piece_count"": 48, ""star_reward"": 2 },
                    { ""picture_id"": 3, ""difficulty_id"": 2, ""display_name"": ""Hard"", ""grid_columns"": 12, ""grid_rows"": 8, ""piece_count"": 96, ""star_reward"": 3 }
                ]
            }";
        }

        [Test]
        public void GetPictureState_InitiallyUnlocked_ReturnsUnlockedOrCompleted()
        {
            var staticData = new StaticDataService(false);
            staticData.LoadFromText(GetMockStaticDataJson());
            var saveService = new MockSaveDataService();
            var progression = new ProgressionService(staticData, saveService);

            // Save is empty, Picture 1 initially unlocked -> should be Unlocked
            Assert.AreEqual(PictureCardState.Unlocked, progression.GetPictureState(1));

            // Mark all difficulties completed
            saveService.SaveData.CompletedPuzzles.Add(new CompletedPuzzleData { PictureId = 1, DifficultyId = 0, BestStar = 1 });
            saveService.SaveData.CompletedPuzzles.Add(new CompletedPuzzleData { PictureId = 1, DifficultyId = 1, BestStar = 2 });
            saveService.SaveData.CompletedPuzzles.Add(new CompletedPuzzleData { PictureId = 1, DifficultyId = 2, BestStar = 3 });

            Assert.AreEqual(PictureCardState.Completed, progression.GetPictureState(1));
        }

        [Test]
        public void GetPictureState_NotInitiallyUnlocked_Locked_AndTransitionToReadyToUnlock()
        {
            var staticData = new StaticDataService(false);
            staticData.LoadFromText(GetMockStaticDataJson());
            var saveService = new MockSaveDataService();
            var progression = new ProgressionService(staticData, saveService);

            // Picture 2 requires Item 101, which player doesn't own yet -> Locked
            Assert.AreEqual(PictureCardState.Locked, progression.GetPictureState(2));

            // Add Item 101 to owned items -> ReadyToUnlock
            saveService.SaveData.OwnedItemIds.Add(101);
            Assert.AreEqual(PictureCardState.ReadyToUnlock, progression.GetPictureState(2));
        }

        [Test]
        public void GetPictureState_AfterUnlocking_ReturnsUnlocked()
        {
            var staticData = new StaticDataService(false);
            staticData.LoadFromText(GetMockStaticDataJson());
            var saveService = new MockSaveDataService();
            var progression = new ProgressionService(staticData, saveService);

            saveService.SaveData.UnlockedPictureIds.Add(2);
            // Even if player doesn't have requirements, if it's already in UnlockedPictureIds -> Unlocked
            Assert.AreEqual(PictureCardState.Unlocked, progression.GetPictureState(2));
        }

        [Test]
        public void GetPictureState_LockedEvenWithCompletions_IfUnlockFlagMissing()
        {
            var staticData = new StaticDataService(false);
            staticData.LoadFromText(GetMockStaticDataJson());
            var saveService = new MockSaveDataService();
            var progression = new ProgressionService(staticData, saveService);

            // Add completion data for Picture 2, but the picture is NOT unlocked
            saveService.SaveData.CompletedPuzzles.Add(new CompletedPuzzleData { PictureId = 2, DifficultyId = 0, BestStar = 1 });
            saveService.SaveData.CompletedPuzzles.Add(new CompletedPuzzleData { PictureId = 2, DifficultyId = 1, BestStar = 2 });
            saveService.SaveData.CompletedPuzzles.Add(new CompletedPuzzleData { PictureId = 2, DifficultyId = 2, BestStar = 3 });

            // Correct Completed Check Order: Should still be Locked
            Assert.AreEqual(PictureCardState.Locked, progression.GetPictureState(2));
        }

        [Test]
        public void TryUnlockPicture_FlowAndConstraints()
        {
            var staticData = new StaticDataService(false);
            staticData.LoadFromText(GetMockStaticDataJson());
            var saveService = new MockSaveDataService();
            var progression = new ProgressionService(staticData, saveService);

            // Case 1: Picture not found
            Assert.AreEqual(UnlockResult.PictureNotFound, progression.TryUnlockPicture(999));
            Assert.AreEqual(0, saveService.SaveCallCount);

            // Case 2: Already unlocked (Picture 1 is initially unlocked)
            Assert.AreEqual(UnlockResult.AlreadyUnlocked, progression.TryUnlockPicture(1));
            Assert.AreEqual(0, saveService.SaveCallCount);

            // Case 3: Missing requirements (Picture 2 needs Item 101)
            Assert.AreEqual(UnlockResult.MissingRequirements, progression.TryUnlockPicture(2));
            Assert.AreEqual(0, saveService.SaveCallCount);

            // Case 4: Ready to unlock (Player gets Item 101)
            saveService.SaveData.OwnedItemIds.Add(101);
            int ownedItemsCountBefore = saveService.SaveData.OwnedItemIds.Count;
            Assert.AreEqual(UnlockResult.Success, progression.TryUnlockPicture(2));
            Assert.Contains(2, saveService.SaveData.UnlockedPictureIds);
            Assert.AreEqual(1, saveService.SaveCallCount);
            Assert.AreEqual(1, saveService.SaveData.UnlockedPictureIds.FindAll(id => id == 2).Count);
            
            // Check that Key Item is NOT consumed
            Assert.AreEqual(ownedItemsCountBefore, saveService.SaveData.OwnedItemIds.Count);
            Assert.Contains(101, saveService.SaveData.OwnedItemIds);

            // Case 5: Try unlocking again -> AlreadyUnlocked
            Assert.AreEqual(UnlockResult.AlreadyUnlocked, progression.TryUnlockPicture(2));
            Assert.AreEqual(1, saveService.SaveCallCount);
            Assert.AreEqual(1, saveService.SaveData.UnlockedPictureIds.FindAll(id => id == 2).Count);
        }

        [Test]
        public void IsDifficultyUnlocked_SequentialPolicy()
        {
            var staticData = new StaticDataService(false);
            staticData.LoadFromText(GetMockStaticDataJson());
            var saveService = new MockSaveDataService();
            var progression = new ProgressionService(staticData, saveService);

            // Picture 1 is sequential. Difficulty 0 (Easy) should always be unlocked.
            Assert.IsTrue(progression.IsDifficultyUnlocked(1, 0));

            // Difficulty 1 (Normal) is locked because Difficulty 0 is not completed yet.
            Assert.IsFalse(progression.IsDifficultyUnlocked(1, 1));

            // Complete Difficulty 0 with 0 stars (no star count yet) -> should still be locked if stars <= 0
            var easyCompletion = new CompletedPuzzleData { PictureId = 1, DifficultyId = 0, BestStar = 0 };
            saveService.SaveData.CompletedPuzzles.Add(easyCompletion);
            Assert.IsFalse(progression.IsDifficultyUnlocked(1, 1));

            // Give it 1 star -> Difficulty 1 should unlock!
            easyCompletion.BestStar = 1;
            Assert.IsTrue(progression.IsDifficultyUnlocked(1, 1));

            // Difficulty 2 (Hard) is still locked because Difficulty 1 is not completed
            Assert.IsFalse(progression.IsDifficultyUnlocked(1, 2));

            // Complete Difficulty 1 with 2 stars -> Difficulty 2 unlocks!
            saveService.SaveData.CompletedPuzzles.Add(new CompletedPuzzleData { PictureId = 1, DifficultyId = 1, BestStar = 2 });
            Assert.IsTrue(progression.IsDifficultyUnlocked(1, 2));
        }

        [Test]
        public void IsDifficultyUnlocked_AllUnlockedPolicy()
        {
            var staticData = new StaticDataService(false);
            staticData.LoadFromText(GetMockStaticDataJson());
            var saveService = new MockSaveDataService();
            var progression = new ProgressionService(staticData, saveService);

            // Picture 3 has all_unlocked policy. Unlocked it first.
            saveService.SaveData.UnlockedPictureIds.Add(3);

            // All difficulties should be unlocked immediately
            Assert.IsTrue(progression.IsDifficultyUnlocked(3, 0));
            Assert.IsTrue(progression.IsDifficultyUnlocked(3, 1));
            Assert.IsTrue(progression.IsDifficultyUnlocked(3, 2));
        }

        [Test]
        public void GetItemSourceHints_ReturnsCorrectSources()
        {
            var staticData = new StaticDataService(false);
            staticData.LoadFromText(GetMockStaticDataJson());
            var saveService = new MockSaveDataService();
            var progression = new ProgressionService(staticData, saveService);

            // Item 101 is rewarded by Picture 1, Difficulty 0
            var hints = progression.GetItemSourceHints(101);
            Assert.AreEqual(1, hints.Count);
            Assert.AreEqual(1, hints[0].PictureId);
            Assert.AreEqual(0, hints[0].DifficultyId);

            // Item 102 is rewarded by Picture 2, Difficulty 0
            var hints2 = progression.GetItemSourceHints(102);
            Assert.AreEqual(1, hints2.Count);
            Assert.AreEqual(2, hints2[0].PictureId);
            Assert.AreEqual(0, hints2[0].DifficultyId);

            // Item 103 is not rewarded by any difficulty configured
            var hints3 = progression.GetItemSourceHints(103);
            Assert.AreEqual(0, hints3.Count);
        }

        [Test]
        public void ValidateStaticData_DeadlockRequiredItemUnreachable_ThrowsException()
        {
            // Picture 2 requires item 101 to unlock.
            // But item 101 is never rewarded by any picture's first clear reward.
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""categories"": [
                    { ""id"": 1, ""id_string"": ""cat1"", ""display_name"": ""Cat 1"" }
                ],
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1, ""is_initially_unlocked"": true, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [] },
                    { ""id"": 2, ""id_string"": ""pic2"", ""display_name"": ""Pic 2"", ""category_id"": 1, ""is_initially_unlocked"": false, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [101] }
                ],
                ""items"": [
                    { ""id"": 101, ""id_string"": ""key1"", ""display_name"": ""Key 1"", ""item_type"": ""key_item"", ""is_consumable"": false, ""status"": ""active"", ""asset_path"": """" }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24 },
                    { ""picture_id"": 2, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24 }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
        }

        [Test]
        public void ValidateStaticData_DeadlockCircularRequirement_ThrowsException()
        {
            // Picture 2 requires Item 102 (rewarded by Picture 3).
            // Picture 3 requires Item 101 (rewarded by Picture 2).
            // Both are locked and none is initially unlocked -> Circular Deadlock!
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""categories"": [
                    { ""id"": 1, ""id_string"": ""cat1"", ""display_name"": ""Cat 1"" }
                ],
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1, ""is_initially_unlocked"": true, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [] },
                    { ""id"": 2, ""id_string"": ""pic2"", ""display_name"": ""Pic 2"", ""category_id"": 1, ""is_initially_unlocked"": false, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [102] },
                    { ""id"": 3, ""id_string"": ""pic3"", ""display_name"": ""Pic 3"", ""category_id"": 1, ""is_initially_unlocked"": false, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [101] }
                ],
                ""items"": [
                    { ""id"": 101, ""id_string"": ""key1"", ""display_name"": ""Key 1"", ""item_type"": ""key_item"", ""is_consumable"": false, ""status"": ""active"", ""asset_path"": """" },
                    { ""id"": 102, ""id_string"": ""key2"", ""display_name"": ""Key 2"", ""item_type"": ""key_item"", ""is_consumable"": false, ""status"": ""active"", ""asset_path"": """" }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24 },
                    { ""picture_id"": 2, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24, ""first_clear_reward_item_ids"": [102] },
                    { ""picture_id"": 3, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24, ""first_clear_reward_item_ids"": [101] }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
        }

        [Test]
        public void ValidateStaticData_UnlockRequirementNotKeyItem_ThrowsException()
        {
            // Picture 2 requires item 101, but 101 is NOT a key_item (it is coin or normal item) -> invalid!
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""categories"": [
                    { ""id"": 1, ""id_string"": ""cat1"", ""display_name"": ""Cat 1"" }
                ],
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1, ""is_initially_unlocked"": true, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [] },
                    { ""id"": 2, ""id_string"": ""pic2"", ""display_name"": ""Pic 2"", ""category_id"": 1, ""is_initially_unlocked"": false, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [101] }
                ],
                ""items"": [
                    { ""id"": 101, ""id_string"": ""coin"", ""display_name"": ""Coin"", ""item_type"": ""currency"", ""is_consumable"": false, ""status"": ""active"", ""asset_path"": """" }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24, ""first_clear_reward_item_ids"": [101] },
                    { ""picture_id"": 2, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24 }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
        }

        private static string GetValidUnlockValidationJson()
        {
            return @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""categories"": [
                    { ""id"": 1, ""id_string"": ""cat1"", ""display_name"": ""Cat 1"" }
                ],
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1, ""is_initially_unlocked"": true, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [] },
                    { ""id"": 2, ""id_string"": ""pic2"", ""display_name"": ""Pic 2"", ""category_id"": 1, ""is_initially_unlocked"": false, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [101] }
                ],
                ""items"": [
                    { ""id"": 101, ""id_string"": ""key1"", ""display_name"": ""Key 1"", ""item_type"": ""key_item"", ""is_consumable"": false, ""status"": ""active"", ""asset_path"": """" }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24, ""first_clear_reward_item_ids"": [101] },
                    { ""picture_id"": 2, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24 }
                ]
            }";
        }

        [Test]
        public void ValidateStaticData_UnlockRequirementMissingActiveStatus_ThrowsException()
        {
            string json = GetValidUnlockValidationJson()
                .Replace(@"""status"": ""active"", ", "");

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
        }

        [Test]
        public void ValidateStaticData_UnlockRequirementConsumable_ThrowsException()
        {
            string json = GetValidUnlockValidationJson()
                .Replace(@"""is_consumable"": false", @"""is_consumable"": true");

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
        }

        [Test]
        public void ValidateStaticData_DuplicateUnlockRequirement_ThrowsException()
        {
            string json = GetValidUnlockValidationJson()
                .Replace(@"""unlock_requirements"": [101]", @"""unlock_requirements"": [101, 101]");

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
        }

        [Test]
        public void ValidateStaticData_InvalidDifficultyPolicy_ThrowsException()
        {
            string json = GetValidUnlockValidationJson()
                .Replace(@"""difficulty_unlock_policy"": ""sequential""",
                    @"""difficulty_unlock_policy"": ""invalid""");

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
        }

        [Test]
        public void ValidateStaticData_SequentialDifficultyGap_ThrowsException()
        {
            string json = GetValidUnlockValidationJson()
                .Replace(
                    @"{ ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24, ""first_clear_reward_item_ids"": [101] },",
                    @"{ ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24, ""first_clear_reward_item_ids"": [101] },
                    { ""picture_id"": 1, ""difficulty_id"": 2, ""display_name"": ""Hard"", ""grid_columns"": 12, ""grid_rows"": 8, ""piece_count"": 96 },");

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
        }
    }
}

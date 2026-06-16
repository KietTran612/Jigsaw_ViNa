using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;
using JigsawVina.Presentation.Screens;
using JigsawVina.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace JigsawVina.Tests
{
    public class DailyRewardTests
    {
        private MockStaticDataService _staticData;
        private MockSaveDataService _saveService;
        private MockLocalDateProvider _dateProvider;
        private RewardApplier _rewardApplier;
        private DailyRewardService _dailyRewardService;

        [SetUp]
        public void SetUp()
        {
            _staticData = new MockStaticDataService();
            _saveService = new MockSaveDataService();
            _dateProvider = new MockLocalDateProvider();
            _rewardApplier = new RewardApplier(_staticData);
            _dailyRewardService = new DailyRewardService(_staticData, _rewardApplier, _dateProvider);

            // Populate default items (Coin = 1, Hint = 2, Key Item = 101, Consumable = 10)
            _staticData.Items.Add(new ItemDto { id = 1, id_string = "coin", display_name = "Coins", item_type = "currency", status = "active" });
            _staticData.Items.Add(new ItemDto { id = 2, id_string = "hint", display_name = "Hints", item_type = "currency", status = "active" });
            _staticData.Items.Add(new ItemDto { id = 101, id_string = "key1", display_name = "Key 101", item_type = "key_item", max_stack = 1, status = "active" });
            _staticData.Items.Add(new ItemDto { id = 10, id_string = "cons10", display_name = "Consumable 10", item_type = "consumable", is_consumable = true, max_stack = 3, status = "active" });

            // Populate daily rewards for 7 days
            for (int d = 1; d <= 7; d++)
            {
                _staticData.DailyRewards.Add(new DailyRewardConfig(d, 1, 50 * d));
            }
        }

        // ---------------------------------------------------------
        // 1. RewardApplier Unit Tests
        // ---------------------------------------------------------

        [Test]
        public void RewardApplier_Apply_Coins_Succeeds()
        {
            var save = new PlayerSave { Coins = 10 };
            var result = _rewardApplier.Apply(save, 1, 50, RewardApplyPolicy.Standard);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(60, save.Coins);
            Assert.AreEqual(50, result.AppliedAmount);
            Assert.AreEqual("Coins", result.DisplayName);
            Assert.IsFalse(result.IsCompensated);
        }

        [Test]
        public void RewardApplier_Apply_Hints_Succeeds()
        {
            var save = new PlayerSave { Hints = 5 };
            var result = _rewardApplier.Apply(save, 2, 3, RewardApplyPolicy.Standard);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(8, save.Hints);
            Assert.AreEqual(3, result.AppliedAmount);
            Assert.AreEqual("Hints", result.DisplayName);
            Assert.IsFalse(result.IsCompensated);
        }

        [Test]
        public void RewardApplier_Apply_ConsumableStandard_StackClamped_ReturnsDelta()
        {
            var save = new PlayerSave();
            save.Inventory.Add(new InventoryItem { ItemId = 10, Amount = 1 }); // max is 3

            var result = _rewardApplier.Apply(save, 10, 5, RewardApplyPolicy.Standard); // exceeds remaining capacity (2)

            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, save.Inventory.First(i => i.ItemId == 10).Amount);
            Assert.AreEqual(2, result.AppliedAmount); // delta applied is 2
            Assert.IsFalse(result.IsCompensated);
        }

        [Test]
        public void RewardApplier_Apply_ConsumableWithCompensation_FullStack_AwardsCompensation()
        {
            var save = new PlayerSave { Coins = 50 };
            save.Inventory.Add(new InventoryItem { ItemId = 10, Amount = 3 }); // max is 3

            var result = _rewardApplier.Apply(save, 10, 1, RewardApplyPolicy.WithCompensation);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, save.Inventory.First(i => i.ItemId == 10).Amount);
            Assert.AreEqual(50 + RewardApplier.DuplicateRewardCompensationCoins, save.Coins);
            Assert.AreEqual(RewardApplier.DuplicateRewardCompensationCoins, result.AppliedAmount);
            Assert.IsTrue(result.IsCompensated);
        }

        [Test]
        public void RewardApplier_Apply_ConsumableStandard_FullStack_Fails()
        {
            var save = new PlayerSave { Coins = 50 };
            save.Inventory.Add(new InventoryItem { ItemId = 10, Amount = 3 }); // max is 3

            var result = _rewardApplier.Apply(save, 10, 1, RewardApplyPolicy.Standard);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(3, save.Inventory.First(i => i.ItemId == 10).Amount);
            Assert.AreEqual(50, save.Coins); // No coins awarded
            Assert.AreEqual(0, result.AppliedAmount);
        }

        [Test]
        public void RewardApplier_Apply_KeyItemWithCompensation_Duplicate_AwardsCompensation()
        {
            var save = new PlayerSave { Coins = 50 };
            save.OwnedItemIds.Add(101);

            var result = _rewardApplier.Apply(save, 101, 1, RewardApplyPolicy.WithCompensation);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, save.OwnedItemIds.Count); // Keeps only 1 key item
            Assert.AreEqual(50 + RewardApplier.DuplicateRewardCompensationCoins, save.Coins);
            Assert.AreEqual(RewardApplier.DuplicateRewardCompensationCoins, result.AppliedAmount);
            Assert.IsTrue(result.IsCompensated);
        }

        [Test]
        public void RewardApplier_Apply_KeyItemStandard_Duplicate_Fails()
        {
            var save = new PlayerSave { Coins = 50 };
            save.OwnedItemIds.Add(101);

            var result = _rewardApplier.Apply(save, 101, 1, RewardApplyPolicy.Standard);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(50, save.Coins);
            Assert.AreEqual(0, result.AppliedAmount);
        }

        [Test]
        public void RewardApplier_Apply_KeyItemStandard_New_Succeeds()
        {
            var save = new PlayerSave();
            var result = _rewardApplier.Apply(save, 101, 1, RewardApplyPolicy.Standard);

            Assert.IsTrue(result.Success);
            Assert.Contains(101, save.OwnedItemIds);
            Assert.AreEqual(1, result.AppliedAmount);
            Assert.IsFalse(result.IsCompensated);
        }

        [Test]
        public void RewardApplier_Apply_EdgeCases_InvalidItem_ReturnsFailure()
        {
            var save = new PlayerSave();
            
            // Invalid item ID (999 does not exist)
            var result = _rewardApplier.Apply(save, 999, 1, RewardApplyPolicy.Standard);
            Assert.IsFalse(result.Success);

            // Inactive item
            _staticData.Items.Add(new ItemDto { id = 200, display_name = "Inactive", item_type = "currency", status = "inactive" });
            result = _rewardApplier.Apply(save, 200, 1, RewardApplyPolicy.Standard);
            Assert.IsFalse(result.Success);

            // Negative amount
            result = _rewardApplier.Apply(save, 1, -5, RewardApplyPolicy.Standard);
            Assert.IsFalse(result.Success);

            // Unsupported item type
            _staticData.Items.Add(new ItemDto { id = 300, display_name = "Bad Type", item_type = "invalid_type", status = "active" });
            result = _rewardApplier.Apply(save, 300, 1, RewardApplyPolicy.Standard);
            Assert.IsFalse(result.Success);
        }

        // Regression Test: First clear of a difficulty rewarding duplicate key item gets coin compensation
        [Test]
        public void RewardSummaryPresenter_FirstClear_DuplicateKeyItem_AwardsCompensation()
        {
            var saveService = new MockSaveDataService();
            saveService.SaveData.Coins = 100;
            saveService.SaveData.OwnedItemIds.Add(101); // Player already owns key item 101

            var session = new GameSessionService();
            session.SetSelectedPicture(1);
            session.SetSelectedDifficulty(0);

            var staticData = new MockProgressionStaticDataService();
            // In MockProgressionStaticDataService: picture 1, diff 0 first clear rewards Key Item 101.
            
            // Instantiate RewardSummaryPresenter injecting our RewardApplier
            var presenter = new RewardSummaryPresenter(
                null,
                session,
                saveService,
                staticData,
                null, // Pass null dropRewardService, it will use NoOp fallback
                _rewardApplier);

            presenter.ProcessRewardsAndDisplay(15f);

            var save = saveService.Load();
            Assert.AreEqual(100 + 30 + RewardApplier.DuplicateRewardCompensationCoins, save.Coins); // 100 base + 30 first clear coin + 100 duplicate compensation
            Assert.AreEqual(1, save.OwnedItemIds.Count(id => id == 101)); // Deduplicated / single item
        }

        // ---------------------------------------------------------
        // 2. DailyRewardService Unit Tests
        // ---------------------------------------------------------

        [Test]
        public void CanClaimToday_FlowAndDrift()
        {
            var save = new PlayerSave();
            
            // Empty claim date
            Assert.IsTrue(_dailyRewardService.CanClaimToday(save));

            // Claimed today
            save.LastDailyRewardClaimDateString = "2026-06-16";
            _dateProvider.DateString = "2026-06-16";
            Assert.IsFalse(_dailyRewardService.CanClaimToday(save));

            // Claim tomorrow
            _dateProvider.DateString = "2026-06-17";
            Assert.IsTrue(_dailyRewardService.CanClaimToday(save));

            // Clock drift: Today is behind last claim date
            _dateProvider.DateString = "2026-06-15";
            Assert.IsFalse(_dailyRewardService.CanClaimToday(save));
        }

        [Test]
        public void Normalize_RepairsDateFormatAndStreak()
        {
            var save = new PlayerSave
            {
                LastDailyRewardClaimDateString = "invalid_date",
                DailyRewardStreak = 99
            };

            save.Normalize();

            Assert.IsNull(save.LastDailyRewardClaimDateString);
            Assert.AreEqual(0, save.DailyRewardStreak);
            Assert.IsTrue(_dailyRewardService.CanClaimToday(save)); // Pure query returns true after repair
        }

        [Test]
        public void GetNextRewardDayIndex_Consecutive_Missed_Drift()
        {
            var save = new PlayerSave();

            // Case 1: Empty claim date
            Assert.AreEqual(1, _dailyRewardService.GetNextRewardDayIndex(save));

            // Case 2: Claimed yesterday, consecutive login
            save.LastDailyRewardClaimDateString = "2026-06-15";
            save.DailyRewardStreak = 2;
            _dateProvider.DateString = "2026-06-16";
            Assert.AreEqual(3, _dailyRewardService.GetNextRewardDayIndex(save));

            // Case 3: Missed a day (last claimed 3 days ago)
            save.LastDailyRewardClaimDateString = "2026-06-13";
            save.DailyRewardStreak = 2;
            _dateProvider.DateString = "2026-06-16";
            Assert.AreEqual(1, _dailyRewardService.GetNextRewardDayIndex(save));

            // Case 4: Same day (already claimed)
            save.LastDailyRewardClaimDateString = "2026-06-16";
            save.DailyRewardStreak = 3;
            _dateProvider.DateString = "2026-06-16";
            Assert.AreEqual(4, _dailyRewardService.GetNextRewardDayIndex(save)); // Shows tomorrow's day index
        }

        [Test]
        public void GetNextRewardDayIndex_StreakBounds()
        {
            var save = new PlayerSave
            {
                LastDailyRewardClaimDateString = "2026-06-16",
                DailyRewardStreak = -5 // Out of bounds
            };
            _dateProvider.DateString = "2026-06-16";
            
            // Defensively treats streak as 0 -> next day index should be (0 % 7) + 1 = 1
            Assert.AreEqual(1, _dailyRewardService.GetNextRewardDayIndex(save));
        }

        [Test]
        public void ClaimDailyReward_StreakWrapping()
        {
            var save = new PlayerSave
            {
                LastDailyRewardClaimDateString = "2026-06-15",
                DailyRewardStreak = 7 // Last claim was Day 7
            };
            _dateProvider.DateString = "2026-06-16";

            var result = _dailyRewardService.ClaimDailyReward(save);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.DayIndex); // Streak wraps back to 1
            Assert.AreEqual(1, save.DailyRewardStreak);
            Assert.AreEqual("2026-06-16", save.LastDailyRewardClaimDateString);
        }

        [Test]
        public void ClaimDailyReward_FailGuard_NoStreakIncrease()
        {
            var save = new PlayerSave
            {
                LastDailyRewardClaimDateString = "2026-06-15",
                DailyRewardStreak = 2
            };
            _dateProvider.DateString = "2026-06-16";

            // Set Day 3 reward item ID to 999 (invalid)
            _staticData.DailyRewards[2] = new DailyRewardConfig(3, 999, 1);

            var result = _dailyRewardService.ClaimDailyReward(save);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(2, save.DailyRewardStreak); // Streak remains unchanged
            Assert.AreEqual("2026-06-15", save.LastDailyRewardClaimDateString); // Last claim date unchanged
        }

        // ---------------------------------------------------------
        // 3. Static Data Validator Tests
        // ---------------------------------------------------------

        [Test]
        public void ValidateStaticData_DailyRewardsNotExactly7Days_Throws()
        {
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""categories"": [
                    { ""id"": 1, ""id_string"": ""cat1"", ""display_name"": ""Cat 1"" }
                ],
                ""pictures"": [],
                ""items"": [
                    { ""id"": 1, ""id_string"": ""coin"", ""display_name"": ""Coins"", ""item_type"": ""currency"", ""status"": ""active"" },
                    { ""id"": 2, ""id_string"": ""hint"", ""display_name"": ""Hints"", ""item_type"": ""currency"", ""status"": ""active"" }
                ],
                ""picture_difficulties"": [],
                ""daily_rewards"": [
                    { ""day_index"": 1, ""item_id"": 1, ""amount"": 50 }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
        }

        [Test]
        public void ValidateStaticData_DailyRewardsItemMissingOrInactive_Throws()
        {
            // Day 1 references item 3 (missing)
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""categories"": [
                    { ""id"": 1, ""id_string"": ""cat1"", ""display_name"": ""Cat 1"" }
                ],
                ""pictures"": [],
                ""items"": [
                    { ""id"": 1, ""id_string"": ""coin"", ""display_name"": ""Coins"", ""item_type"": ""currency"", ""status"": ""active"" },
                    { ""id"": 2, ""id_string"": ""hint"", ""display_name"": ""Hints"", ""item_type"": ""currency"", ""status"": ""active"" }
                ],
                ""picture_difficulties"": [],
                ""daily_rewards"": [
                    { ""day_index"": 1, ""item_id"": 3, ""amount"": 50 },
                    { ""day_index"": 2, ""item_id"": 1, ""amount"": 50 },
                    { ""day_index"": 3, ""item_id"": 1, ""amount"": 50 },
                    { ""day_index"": 4, ""item_id"": 1, ""amount"": 50 },
                    { ""day_index"": 5, ""item_id"": 1, ""amount"": 50 },
                    { ""day_index"": 6, ""item_id"": 1, ""amount"": 50 },
                    { ""day_index"": 7, ""item_id"": 1, ""amount"": 50 }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
        }

        // ---------------------------------------------------------
        // 4. Game Data Editor Tests
        // ---------------------------------------------------------

        [Test]
        public void GameDataEditor_SeedsDefaultDailyRewardsOnNull()
        {
            var window = ScriptableObject.CreateInstance<JigsawVinaGameDataEditor>();
            var dto = new StaticDataDto(); // No daily rewards configured

            window.LoadStateFromDto(dto);

            Assert.AreEqual(7, window._dailyRewards.Count);
            Assert.AreEqual(1, window._dailyRewards[0].day_index);
            Assert.AreEqual(1, window._dailyRewards[0].item_id); // Coin fallback
            Assert.AreEqual(50, window._dailyRewards[0].amount);

            UnityEngine.Object.DestroyImmediate(window);
        }

        // ---------------------------------------------------------
        // 5. Presenter and Controller Lifecycle Tests
        // ---------------------------------------------------------

        [Test]
        public void DailyRewardPresenter_Dispose_UnsubscribesEvents()
        {
            var viewHolder = new GameObject("View");
            var view = viewHolder.AddComponent<DailyRewardView>();

            var buttonHolder1 = new GameObject("ClaimButton");
            buttonHolder1.transform.SetParent(viewHolder.transform);
            var claimBtn = buttonHolder1.AddComponent<Button>();

            var buttonHolder2 = new GameObject("CloseButton");
            buttonHolder2.transform.SetParent(viewHolder.transform);
            var closeBtn = buttonHolder2.AddComponent<Button>();

            var viewSo = new UnityEditor.SerializedObject(view);
            viewSo.FindProperty("_claimButton").objectReferenceValue = claimBtn;
            viewSo.FindProperty("_closeButton").objectReferenceValue = closeBtn;
            viewSo.ApplyModifiedProperties();

            // Invoke awake to bind buttons
            var awakeMethod = typeof(DailyRewardView).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            awakeMethod.Invoke(view, null);

            var presenter = new DailyRewardPresenter(view, _dailyRewardService, _saveService, _staticData);
            
            int claimRequestCount = 0;
            view.OnClaimRequested += () => claimRequestCount++;
            int closeRequestCount = 0;
            view.OnCloseRequested += () => closeRequestCount++;

            // Dispose presenter
            presenter.Dispose();

            // Simulate clicks
            claimBtn.onClick.Invoke();
            closeBtn.onClick.Invoke();

            // Presenter should not listen to view events anymore, though our direct local tests will still trigger
            // To prove presenter is unbound, let's verify presenter state or call methods.
            // Since we unbound from view, click should only trigger local handlers but NOT presenter logic (e.g. Save count remains 0).
            Assert.AreEqual(0, _saveService.SaveCallCount);

            UnityEngine.Object.DestroyImmediate(viewHolder);
        }

        [Test]
        public void HomeFlowController_Dispose_UnsubscribesEvents()
        {
            var picSelectGo = new GameObject("PicSelect");
            var picSelectView = picSelectGo.AddComponent<PictureSelectView>();

            var rewardBtnGo = new GameObject("RewardButton");
            rewardBtnGo.transform.SetParent(picSelectGo.transform);
            var rewardBtn = rewardBtnGo.AddComponent<Button>();

            var picSelectSo = new UnityEditor.SerializedObject(picSelectView);
            picSelectSo.FindProperty("_dailyRewardButton").objectReferenceValue = rewardBtn;
            picSelectSo.ApplyModifiedProperties();

            // Invoke Awake on PictureSelectView to register onClick listener
            var awakePicSelect = typeof(PictureSelectView).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            awakePicSelect.Invoke(picSelectView, null);

            var diffSelectGo = new GameObject("DiffSelect");
            var diffSelectView = diffSelectGo.AddComponent<DifficultySelectView>();

            var dailyRewardPopupGo = new GameObject("DailyRewardPopup");
            var dailyRewardView = dailyRewardPopupGo.AddComponent<DailyRewardView>();
            dailyRewardView.gameObject.SetActive(false);

            var dailyPresenter = new DailyRewardPresenter(dailyRewardView, _dailyRewardService, _saveService, _staticData);

            var flowController = new HomeFlowController(
                picSelectView,
                diffSelectView,
                null,
                null,
                null,
                null,
                null,
                dailyPresenter,
                _dailyRewardService,
                _saveService);

            flowController.Start();

            // Before Dispose: button click triggers the popup
            Assert.IsFalse(dailyRewardView.gameObject.activeSelf);
            rewardBtn.onClick.Invoke();
            Assert.IsTrue(dailyRewardView.gameObject.activeSelf);

            // Close/deactivate the popup
            dailyRewardView.gameObject.SetActive(false);

            // Dispose the flow controller
            flowController.Dispose();

            // After Dispose: button click should not trigger the popup
            rewardBtn.onClick.Invoke();
            Assert.IsFalse(dailyRewardView.gameObject.activeSelf);

            UnityEngine.Object.DestroyImmediate(picSelectGo);
            UnityEngine.Object.DestroyImmediate(diffSelectGo);
            UnityEngine.Object.DestroyImmediate(dailyRewardPopupGo);
        }

        [Test]
        public void DailyRewardView_SetDailyRewardSlots_EmptyAssetPath_FallbackTextAndImageDisabled()
        {
            var go = new GameObject("View");
            var view = go.AddComponent<DailyRewardView>();
            var imgGo = new GameObject("Image", typeof(RectTransform), typeof(Image));
            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            imgGo.transform.SetParent(go.transform);
            txtGo.transform.SetParent(go.transform);

            var viewSo = new UnityEditor.SerializedObject(view);
            var slotsProp = viewSo.FindProperty("_slots");
            slotsProp.arraySize = 1;
            var slot0 = slotsProp.GetArrayElementAtIndex(0);
            slot0.FindPropertyRelative("rewardImage").objectReferenceValue = imgGo.GetComponent<Image>();
            slot0.FindPropertyRelative("amountText").objectReferenceValue = txtGo.GetComponent<Text>();
            viewSo.ApplyModifiedProperties();

            var slotDatas = new List<DailyRewardView.SlotData>
            {
                new DailyRewardView.SlotData { DayIndex = 1, Amount = 1, AssetPath = "", DisplayName = "Tem Bưu Thiếp" }
            };
            view.SetDailyRewardSlots(slotDatas, 1, true);

            var img = imgGo.GetComponent<Image>();
            var txt = txtGo.GetComponent<Text>();
            Assert.IsFalse(img.enabled);
            Assert.AreEqual("+1 Tem Bưu Thiếp", txt.text);
            Assert.IsTrue(txt.resizeTextForBestFit);
            Assert.AreEqual(HorizontalWrapMode.Wrap, txt.horizontalOverflow);

            UnityEngine.Object.DestroyImmediate(go);
        }

        // ---------------------------------------------------------
        // Fakes / Stubs
        // ---------------------------------------------------------

        private class MockStaticDataService : IStaticDataService
        {
            public List<ItemDto> Items = new();
            public List<DailyRewardConfig> DailyRewards = new();

            public IReadOnlyList<PictureConfig> GetAllPictures() => new List<PictureConfig>();
            public PictureConfig GetPictureById(int id) => default;
            public PictureDifficultyConfig GetPictureDifficulty(int pictureId, int difficultyId) => default;
            public ItemDto GetItemById(int id) => Items.FirstOrDefault(i => i.id == id);
            public IReadOnlyList<ItemDto> GetAllItems() => Items;
            public IReadOnlyList<PictureDifficultyConfig> GetPictureDifficulties(int pictureId) => new List<PictureDifficultyConfig>();
            public IReadOnlyList<PictureDifficultyConfig> GetAllPictureDifficulties() => new List<PictureDifficultyConfig>();
            public IReadOnlyList<DropTableConfig> GetAllDropTables() => new List<DropTableConfig>();
            public IReadOnlyList<DropTableItemConfig> GetDropTableItems(int dropTableId) => new List<DropTableItemConfig>();
            public IReadOnlyList<DropTableItemConfig> GetAllDropTableItems() => new List<DropTableItemConfig>();
            public IReadOnlyList<DailyRewardConfig> GetDailyRewards() => DailyRewards;
        }

        private class MockLocalDateProvider : ILocalDateProvider
        {
            public string DateString = "2026-06-16";
            public string GetCurrentLocalDateString() => DateString;
        }

        private class MockProgressionStaticDataService : IStaticDataService
        {
            private readonly List<ItemDto> _items = new()
            {
                new ItemDto { id = 1, display_name = "Coin", item_type = "currency", status = "active" },
                new ItemDto { id = 2, display_name = "Hint", item_type = "currency", status = "active" },
                new ItemDto
                {
                    id = 10,
                    display_name = "Stamp",
                    item_type = "consumable",
                    is_consumable = true,
                    max_stack = 3,
                    status = "active"
                },
                new ItemDto
                {
                    id = 101,
                    display_name = "Key 101",
                    item_type = "key_item",
                    max_stack = 1,
                    status = "active"
                }
            };

            public IReadOnlyList<PictureConfig> GetAllPictures() => new List<PictureConfig>();
            public PictureConfig GetPictureById(int id) => default;
            public PictureDifficultyConfig GetPictureDifficulty(int pictureId, int difficultyId) =>
                new PictureDifficultyConfig(
                    1, 0, "Easy", 6, 4, 1, 30, 0, 10, new List<int> { 101 }, 1001);
            public ItemDto GetItemById(int id) => _items.FirstOrDefault(item => item.id == id);
            public IReadOnlyList<ItemDto> GetAllItems() => _items;
            public IReadOnlyList<PictureDifficultyConfig> GetPictureDifficulties(int pictureId) =>
                new List<PictureDifficultyConfig>();
            public IReadOnlyList<PictureDifficultyConfig> GetAllPictureDifficulties() =>
                new List<PictureDifficultyConfig>();
            public IReadOnlyList<DropTableConfig> GetAllDropTables() => new List<DropTableConfig>();
            public IReadOnlyList<DropTableItemConfig> GetDropTableItems(int dropTableId) =>
                new List<DropTableItemConfig>();
            public IReadOnlyList<DropTableItemConfig> GetAllDropTableItems() =>
                new List<DropTableItemConfig>();
            public IReadOnlyList<DailyRewardConfig> GetDailyRewards() => new List<DailyRewardConfig>();
        }
    }
}

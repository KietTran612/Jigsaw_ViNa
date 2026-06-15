using System;
using System.Collections.Generic;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;
using JigsawVina.Editor;
using NUnit.Framework;
using UnityEngine;

namespace JigsawVina.Tests
{
    public class JigsawVinaGameDataEditorTests
    {
        private JigsawVinaGameDataEditor _window;
        private const string DummyKey = "DummyKeyForTesting";

        [SetUp]
        public void SetUp()
        {
            _window = ScriptableObject.CreateInstance<JigsawVinaGameDataEditor>();
            PlayerPrefs.DeleteKey(SaveDataService.SaveKey);
            PlayerPrefs.DeleteKey(DummyKey);
            PlayerPrefs.Save();
        }

        [TearDown]
        public void TearDown()
        {
            if (_window != null)
            {
                UnityEngine.Object.DestroyImmediate(_window);
            }
            PlayerPrefs.DeleteKey(SaveDataService.SaveKey);
            PlayerPrefs.DeleteKey(DummyKey);
            PlayerPrefs.Save();
        }

        [Test]
        public void LoadSaveRoundTripPreservesAllFieldsTest()
        {
            var testItem = new ItemDto
            {
                id = 10,
                id_string = "test_item_10",
                display_name = "Test Item 10",
                description = "Description for test item 10",
                display_name_key = "item.test_item_10.name",
                description_key = "item.test_item_10.description",
                item_type = "collectible",
                rarity = "epic",
                is_consumable = true,
                is_time_limited = true,
                max_stack = 99,
                status = "active",
                sort_order = 10,
                asset_path = "Textures/Items/test_item_10"
            };

            var tabs = new List<JigsawVinaGameDataEditor.EditorTabState>();
            var categories = new List<JigsawVinaGameDataEditor.EditorCategoryState>
            {
                new JigsawVinaGameDataEditor.EditorCategoryState { id = 1, idString = "cat1", displayName = "Category 1" }
            };
            
            var globalItems = new List<ItemDto>
            {
                new ItemDto { id = 1, id_string = "coin", item_type = "currency", display_name = "Xu", status = "active" },
                new ItemDto { id = 2, id_string = "hint", item_type = "currency", display_name = "Gợi ý", status = "active" },
                testItem
            };

            _window.SetStateForTesting(tabs, categories, globalItems);

            bool success = _window.TryBuildConfig(out var config, out var err, false);
            Assert.IsTrue(success, $"BuildConfig failed with: {err}");

            var parsedItem = config.items.Find(i => i.id == 10);
            Assert.IsNotNull(parsedItem);
            Assert.AreEqual(testItem.id, parsedItem.id);
            Assert.AreEqual(testItem.id_string, parsedItem.id_string);
            Assert.AreEqual(testItem.display_name, parsedItem.display_name);
            Assert.AreEqual(testItem.description, parsedItem.description);
            Assert.AreEqual(testItem.display_name_key, parsedItem.display_name_key);
            Assert.AreEqual(testItem.description_key, parsedItem.description_key);
            Assert.AreEqual(testItem.item_type, parsedItem.item_type);
            Assert.AreEqual(testItem.rarity, parsedItem.rarity);
            Assert.AreEqual(testItem.is_consumable, parsedItem.is_consumable);
            Assert.AreEqual(testItem.is_time_limited, parsedItem.is_time_limited);
            Assert.AreEqual(testItem.max_stack, parsedItem.max_stack);
            Assert.AreEqual(testItem.status, parsedItem.status);
            Assert.AreEqual(testItem.sort_order, parsedItem.sort_order);
            Assert.AreEqual(testItem.asset_path, parsedItem.asset_path);
        }

        [Test]
        public void CategoryRoundTripPreservesCategoryIdTest()
        {
            var tabs = new List<JigsawVinaGameDataEditor.EditorTabState>
            {
                new JigsawVinaGameDataEditor.EditorTabState
                {
                    pictureId = 1,
                    idString = "pic1",
                    displayName = "Picture 1",
                    categoryId = 3
                }
            };
            var categories = new List<JigsawVinaGameDataEditor.EditorCategoryState>
            {
                new JigsawVinaGameDataEditor.EditorCategoryState { id = 3, idString = "cat3", displayName = "Category 3" }
            };
            var globalItems = new List<ItemDto>
            {
                new ItemDto { id = 1, id_string = "coin", item_type = "currency", display_name = "Xu", status = "active" },
                new ItemDto { id = 2, id_string = "hint", item_type = "currency", display_name = "Gợi ý", status = "active" }
            };

            _window.SetStateForTesting(tabs, categories, globalItems);

            bool success = _window.TryBuildConfig(out var config, out var err, false);
            Assert.IsTrue(success, $"BuildConfig failed with: {err}");
            Assert.AreEqual(1, config.pictures.Count);
            Assert.AreEqual(3, config.pictures[0].category_id);
        }

        [Test]
        public void LoadStateFromDtoWithoutAssetsPreservesDifficultySettingsTest()
        {
            var dto = new StaticDataDto();
            dto.categories.Add(new CategoryDto { id = 1, id_string = "cat1", display_name = "Category 1" });
            dto.pictures.Add(new PictureDto { id = 1, id_string = "pic1", display_name = "Picture 1", category_id = 1, asset_path = "", is_initially_unlocked = true, difficulty_unlock_policy = "sequential", unlock_requirements = new List<int>() });
            
            dto.picture_difficulties.Add(new PictureDifficultyDto
            {
                picture_id = 1,
                difficulty_id = 0,
                display_name = "Dễ",
                grid_columns = 5,
                grid_rows = 5,
                first_clear_coin = 50,
                replay_coin = 15,
                first_clear_hint = 2,
                first_clear_reward_item_ids = new List<int> { 101 }
            });
            dto.picture_difficulties.Add(new PictureDifficultyDto
            {
                picture_id = 1,
                difficulty_id = 1,
                display_name = "Trung bình",
                grid_columns = 7,
                grid_rows = 7,
                first_clear_coin = 100,
                replay_coin = 30,
                first_clear_hint = 4,
                first_clear_reward_item_ids = new List<int> { 102 }
            });
            dto.picture_difficulties.Add(new PictureDifficultyDto
            {
                picture_id = 1,
                difficulty_id = 2,
                display_name = "Khó",
                grid_columns = 9,
                grid_rows = 9,
                first_clear_coin = 150,
                replay_coin = 45,
                first_clear_hint = 6,
                first_clear_reward_item_ids = new List<int>()
            });

            dto.items.Add(new ItemDto { id = 101, id_string = "key_item_1", item_type = "key_item", display_name = "Key Item 1", asset_path = "Textures/key_item_1.png" });
            dto.items.Add(new ItemDto { id = 102, id_string = "key_item_2", item_type = "key_item", display_name = "Key Item 2", asset_path = "Textures/key_item_2.png" });

            _window.LoadStateFromDto(dto);

            Assert.AreEqual(1, _window._tabs.Count);
            var tab = _window._tabs[0];
            Assert.AreEqual(1, tab.pictureId);
            Assert.AreEqual("pic1", tab.idString);
            
            Assert.AreEqual(5, tab.easyCols);
            Assert.AreEqual(5, tab.easyRows);
            Assert.AreEqual(50, tab.easyCoins);
            Assert.AreEqual(15, tab.easyReplayCoins);
            Assert.AreEqual(2, tab.easyHints);

            Assert.AreEqual(7, tab.normalCols);
            Assert.AreEqual(7, tab.normalRows);
            Assert.AreEqual(100, tab.normalCoins);
            Assert.AreEqual(30, tab.normalReplayCoins);
            Assert.AreEqual(4, tab.normalHints);

            Assert.AreEqual(9, tab.hardCols);
            Assert.AreEqual(9, tab.hardRows);
            Assert.AreEqual(150, tab.hardCoins);
            Assert.AreEqual(45, tab.hardReplayCoins);
            Assert.AreEqual(6, tab.hardHints);

            bool success = _window.TryBuildConfig(out var outputConfig, out var err, false);
            Assert.IsTrue(success, $"BuildConfig failed with: {err}");

            var easyDiff = outputConfig.picture_difficulties.Find(d => d.picture_id == 1 && d.difficulty_id == 0);
            Assert.IsNotNull(easyDiff);
            Assert.AreEqual(5, easyDiff.grid_columns);
            Assert.AreEqual(5, easyDiff.grid_rows);
            Assert.AreEqual(50, easyDiff.first_clear_coin);
            Assert.AreEqual(15, easyDiff.replay_coin);
            Assert.AreEqual(2, easyDiff.first_clear_hint);
            Assert.AreEqual(1, easyDiff.first_clear_reward_item_ids.Count);
            Assert.AreEqual(101, easyDiff.first_clear_reward_item_ids[0]);
        }

        [Test]
        public void DuplicateIDValidationTest()
        {
            // Case 1: Duplicate Numeric ID
            {
                var tabs = new List<JigsawVinaGameDataEditor.EditorTabState>();
                var categories = new List<JigsawVinaGameDataEditor.EditorCategoryState>
                {
                    new JigsawVinaGameDataEditor.EditorCategoryState { id = 1, idString = "cat1", displayName = "Category 1" }
                };
                var globalItems = new List<ItemDto>
                {
                    new ItemDto { id = 1, id_string = "coin", item_type = "currency", display_name = "Xu", status = "active" },
                    new ItemDto { id = 2, id_string = "hint", item_type = "currency", display_name = "Gợi ý", status = "active" },
                    new ItemDto { id = 10, id_string = "itemA", item_type = "collectible", display_name = "Item A" },
                    new ItemDto { id = 10, id_string = "itemB", item_type = "collectible", display_name = "Item B" }
                };
                _window.SetStateForTesting(tabs, categories, globalItems);
                bool success = _window.TryBuildConfig(out _, out string err, false);
                Assert.IsFalse(success);
                Assert.IsTrue(err.Contains("Trùng lặp ID Vật phẩm Global: 10"), $"Error message was: {err}");
            }

            // Case 2: Duplicate ID String
            {
                var tabs = new List<JigsawVinaGameDataEditor.EditorTabState>();
                var categories = new List<JigsawVinaGameDataEditor.EditorCategoryState>
                {
                    new JigsawVinaGameDataEditor.EditorCategoryState { id = 1, idString = "cat1", displayName = "Category 1" }
                };
                var globalItems = new List<ItemDto>
                {
                    new ItemDto { id = 1, id_string = "coin", item_type = "currency", display_name = "Xu", status = "active" },
                    new ItemDto { id = 2, id_string = "hint", item_type = "currency", display_name = "Gợi ý", status = "active" },
                    new ItemDto { id = 10, id_string = "itemA", item_type = "collectible", display_name = "Item A" },
                    new ItemDto { id = 11, id_string = "itemA", item_type = "collectible", display_name = "Item B" }
                };
                _window.SetStateForTesting(tabs, categories, globalItems);
                bool success = _window.TryBuildConfig(out _, out string err, false);
                Assert.IsFalse(success);
                Assert.IsTrue(err.Contains("Trùng lặp ID String Vật phẩm Global: 'itemA'"), $"Error message was: {err}");
            }

            // Case 3: Collision between Global Item and Scanned Key Item
            {
                var tabs = new List<JigsawVinaGameDataEditor.EditorTabState>
                {
                    new JigsawVinaGameDataEditor.EditorTabState
                    {
                        pictureId = 1,
                        idString = "pic1",
                        displayName = "Picture 1",
                        categoryId = 1,
                        itemStates = new List<JigsawVinaGameDataEditor.EditorItemState>
                        {
                            new JigsawVinaGameDataEditor.EditorItemState { filename = "key_item_name", displayName = "Key Item Name" }
                        }
                    }
                };
                var categories = new List<JigsawVinaGameDataEditor.EditorCategoryState>
                {
                    new JigsawVinaGameDataEditor.EditorCategoryState { id = 1, idString = "cat1", displayName = "Category 1" }
                };
                var globalItems = new List<ItemDto>
                {
                    new ItemDto { id = 1, id_string = "coin", item_type = "currency", display_name = "Xu", status = "active" },
                    new ItemDto { id = 2, id_string = "hint", item_type = "currency", display_name = "Gợi ý", status = "active" },
                    new ItemDto { id = 101, id_string = "other_name", item_type = "collectible", display_name = "Collider Item" }
                };
                _window.SetStateForTesting(tabs, categories, globalItems);
                bool success = _window.TryBuildConfig(out _, out string err, false);
                Assert.IsFalse(success);
                Assert.IsTrue(err.Contains("Trùng lặp ID Vật phẩm: 101"), $"Error message was: {err}");
            }
        }

        [Test]
        public void PositiveIDValidationTest()
        {
            // Case 1: Category ID <= 0
            {
                var tabs = new List<JigsawVinaGameDataEditor.EditorTabState>();
                var categories = new List<JigsawVinaGameDataEditor.EditorCategoryState>
                {
                    new JigsawVinaGameDataEditor.EditorCategoryState { id = 0, idString = "cat1", displayName = "Category 1" }
                };
                var globalItems = new List<ItemDto>
                {
                    new ItemDto { id = 1, id_string = "coin", item_type = "currency", display_name = "Xu", status = "active" },
                    new ItemDto { id = 2, id_string = "hint", item_type = "currency", display_name = "Gợi ý", status = "active" }
                };
                _window.SetStateForTesting(tabs, categories, globalItems);
                bool success = _window.TryBuildConfig(out _, out string err, false);
                Assert.IsFalse(success);
                Assert.IsTrue(err.Contains("ID Danh mục") && err.Contains("phải là số nguyên dương"), $"Error message was: {err}");
            }

            // Case 2: Picture ID <= 0
            {
                var tabs = new List<JigsawVinaGameDataEditor.EditorTabState>
                {
                    new JigsawVinaGameDataEditor.EditorTabState { pictureId = -5, idString = "pic1", displayName = "Picture 1", categoryId = 1 }
                };
                var categories = new List<JigsawVinaGameDataEditor.EditorCategoryState>
                {
                    new JigsawVinaGameDataEditor.EditorCategoryState { id = 1, idString = "cat1", displayName = "Category 1" }
                };
                var globalItems = new List<ItemDto>
                {
                    new ItemDto { id = 1, id_string = "coin", item_type = "currency", display_name = "Xu", status = "active" },
                    new ItemDto { id = 2, id_string = "hint", item_type = "currency", display_name = "Gợi ý", status = "active" }
                };
                _window.SetStateForTesting(tabs, categories, globalItems);
                bool success = _window.TryBuildConfig(out _, out string err, false);
                Assert.IsFalse(success);
                Assert.IsTrue(err.Contains("ID Tranh") && err.Contains("phải là số nguyên dương"), $"Error message was: {err}");
            }

            // Case 3: Global Item ID <= 0
            {
                var tabs = new List<JigsawVinaGameDataEditor.EditorTabState>();
                var categories = new List<JigsawVinaGameDataEditor.EditorCategoryState>
                {
                    new JigsawVinaGameDataEditor.EditorCategoryState { id = 1, idString = "cat1", displayName = "Category 1" }
                };
                var globalItems = new List<ItemDto>
                {
                    new ItemDto { id = 1, id_string = "coin", item_type = "currency", display_name = "Xu", status = "active" },
                    new ItemDto { id = 2, id_string = "hint", item_type = "currency", display_name = "Gợi ý", status = "active" },
                    new ItemDto { id = -10, id_string = "invalid_item", item_type = "collectible", display_name = "Invalid" }
                };
                _window.SetStateForTesting(tabs, categories, globalItems);
                bool success = _window.TryBuildConfig(out _, out string err, false);
                Assert.IsFalse(success);
                Assert.IsTrue(err.Contains("ID Vật phẩm") && err.Contains("phải là số nguyên dương"), $"Error message was: {err}");
            }
        }

        [Test]
        public void KeyItemCountLimitValidationTest()
        {
            var tab = new JigsawVinaGameDataEditor.EditorTabState
            {
                pictureId = 1,
                idString = "pic1",
                displayName = "Picture 1",
                categoryId = 1
            };

            for (int i = 1; i <= 100; i++)
            {
                tab.itemStates.Add(new JigsawVinaGameDataEditor.EditorItemState
                {
                    filename = $"key_item_{i}",
                    displayName = $"Key Item {i}",
                    rarity = "common"
                });
            }

            var tabs = new List<JigsawVinaGameDataEditor.EditorTabState> { tab };
            var categories = new List<JigsawVinaGameDataEditor.EditorCategoryState>
            {
                new JigsawVinaGameDataEditor.EditorCategoryState { id = 1, idString = "cat1", displayName = "Category 1" }
            };
            var globalItems = new List<ItemDto>
            {
                new ItemDto { id = 1, id_string = "coin", item_type = "currency", display_name = "Xu", status = "active" },
                new ItemDto { id = 2, id_string = "hint", item_type = "currency", display_name = "Gợi ý", status = "active" }
            };

            _window.SetStateForTesting(tabs, categories, globalItems);

            bool success = _window.TryBuildConfig(out _, out string err, false);
            Assert.IsFalse(success);
            Assert.IsTrue(err.Contains("có quá 99 key items"), $"Error message was: {err}");
        }

        [Test]
        public void CategoryDeletionSafetyTest()
        {
            var tab = new JigsawVinaGameDataEditor.EditorTabState
            {
                pictureId = 1,
                idString = "pic1",
                displayName = "Picture 1",
                categoryId = 2
            };

            var tabs = new List<JigsawVinaGameDataEditor.EditorTabState> { tab };
            var categories = new List<JigsawVinaGameDataEditor.EditorCategoryState>
            {
                new JigsawVinaGameDataEditor.EditorCategoryState { id = 1, idString = "cat1", displayName = "Category 1" },
                new JigsawVinaGameDataEditor.EditorCategoryState { id = 2, idString = "cat2", displayName = "Category 2" }
            };
            var globalItems = new List<ItemDto>();

            _window.SetStateForTesting(tabs, categories, globalItems);

            bool canDelete = _window.CanDeleteCategory(2, out string reason);
            Assert.IsFalse(canDelete);
            Assert.IsTrue(reason.Contains("đang thuộc danh mục này"), $"Reason was: {reason}");

            bool canDeleteCat1 = _window.CanDeleteCategory(1, out string reasonCat1);
            Assert.IsTrue(canDeleteCat1, $"Failed to delete category 1: {reasonCat1}");
        }

        [Test]
        public void UnlockAllCheatIsIdempotentAndCleansStaleTest()
        {
            var tabs = new List<JigsawVinaGameDataEditor.EditorTabState>
            {
                new JigsawVinaGameDataEditor.EditorTabState { pictureId = 1, categoryId = 1 },
                new JigsawVinaGameDataEditor.EditorTabState { pictureId = 2, categoryId = 1 }
            };
            var categories = new List<JigsawVinaGameDataEditor.EditorCategoryState>
            {
                new JigsawVinaGameDataEditor.EditorCategoryState { id = 1, idString = "cat1", displayName = "Category 1" }
            };
            _window.SetStateForTesting(tabs, categories, new List<ItemDto>());

            var save = new PlayerSave();
            save.CompletedPuzzles.Add(new CompletedPuzzleData { PictureId = 1, DifficultyId = 0, BestStar = 1, BestTimeSeconds = 100f });
            save.CompletedPuzzles.Add(new CompletedPuzzleData { PictureId = 1, DifficultyId = 0, BestStar = 2, BestTimeSeconds = 80f });
            save.CompletedPuzzles.Add(new CompletedPuzzleData { PictureId = 99, DifficultyId = 0, BestStar = 3, BestTimeSeconds = 50f });

            _window.ApplyUnlockAll(save);

            Assert.AreEqual(6, save.CompletedPuzzles.Count);
            
            foreach (var cp in save.CompletedPuzzles)
            {
                Assert.AreEqual(3, cp.BestStar);
                Assert.AreEqual(45.0f, cp.BestTimeSeconds);
                Assert.IsTrue(cp.PictureId == 1 || cp.PictureId == 2);
            }

            _window.ApplyUnlockAll(save);
            Assert.AreEqual(6, save.CompletedPuzzles.Count);
        }

        [Test]
        public void ResetSaveOnlyTargetedKeyTest()
        {
            PlayerPrefs.SetString(DummyKey, "PreserveMe");
            PlayerPrefs.SetString(SaveDataService.SaveKey, "DeleteMe");
            PlayerPrefs.Save();

            _window.ResetPlayerSave();

            Assert.IsFalse(PlayerPrefs.HasKey(SaveDataService.SaveKey));
            
            Assert.IsTrue(PlayerPrefs.HasKey(DummyKey));
            Assert.AreEqual("PreserveMe", PlayerPrefs.GetString(DummyKey));
        }

        [Test]
        public void EnsureReservedItemsSeededOnEmptyLoadTest()
        {
            var dto = new StaticDataDto();
            dto.categories.Add(new CategoryDto { id = 1, id_string = "cat1", display_name = "Category 1" });
            dto.pictures.Add(new PictureDto { id = 1, id_string = "pic1", display_name = "Picture 1", category_id = 1, is_initially_unlocked = true, difficulty_unlock_policy = "sequential", unlock_requirements = new List<int>() });

            _window.LoadStateFromDto(dto);

            var coin = _window._globalItems.Find(i => i.id == 1);
            Assert.IsNotNull(coin);
            Assert.AreEqual("coin", coin.id_string);
            Assert.AreEqual("currency", coin.item_type);

            var hint = _window._globalItems.Find(i => i.id == 2);
            Assert.IsNotNull(hint);
            Assert.AreEqual("hint", hint.id_string);
            Assert.AreEqual("currency", hint.item_type);
        }

        [Test]
        public void UnlockAllWithDuplicateUnsavedPictureIDsDeduplicatesTest()
        {
            var tabs = new List<JigsawVinaGameDataEditor.EditorTabState>
            {
                new JigsawVinaGameDataEditor.EditorTabState { pictureId = 1, categoryId = 1 },
                new JigsawVinaGameDataEditor.EditorTabState { pictureId = 1, categoryId = 1 }
            };
            var categories = new List<JigsawVinaGameDataEditor.EditorCategoryState>
            {
                new JigsawVinaGameDataEditor.EditorCategoryState { id = 1, idString = "cat1", displayName = "Category 1" }
            };
            _window.SetStateForTesting(tabs, categories, new List<ItemDto>());

            var save = new PlayerSave();
            _window.ApplyUnlockAll(save);

            Assert.AreEqual(3, save.CompletedPuzzles.Count);
        }

        [Test]
        public void EnsureReservedItemsAutoRepairsMalformedReservedEntriesTest()
        {
            var dto = new StaticDataDto();
            dto.categories.Add(new CategoryDto { id = 1, id_string = "cat1", display_name = "Category 1" });
            dto.items.Add(new ItemDto { id = 1, id_string = "malformed_coin", item_type = "collectible", display_name = "Malformed Coin" });
            dto.items.Add(new ItemDto { id = 2, id_string = "malformed_hint", item_type = "collectible", display_name = "Malformed Hint" });

            _window.LoadStateFromDto(dto);

            var coin = _window._globalItems.Find(i => i.id == 1);
            Assert.IsNotNull(coin);
            Assert.AreEqual("coin", coin.id_string);
            Assert.AreEqual("currency", coin.item_type);

            var hint = _window._globalItems.Find(i => i.id == 2);
            Assert.IsNotNull(hint);
            Assert.AreEqual("hint", hint.id_string);
            Assert.AreEqual("currency", hint.item_type);
        }

        [Test]
        public void TryBuildConfigPreservesUnknownGlobalItemTypesTest()
        {
            var tabs = new List<JigsawVinaGameDataEditor.EditorTabState>();
            var categories = new List<JigsawVinaGameDataEditor.EditorCategoryState>
            {
                new JigsawVinaGameDataEditor.EditorCategoryState { id = 1, idString = "cat1", displayName = "Category 1" }
            };
            var globalItems = new List<ItemDto>
            {
                new ItemDto { id = 1, id_string = "coin", item_type = "currency", display_name = "Xu", status = "active" },
                new ItemDto { id = 2, id_string = "hint", item_type = "currency", display_name = "Gợi ý", status = "active" },
                new ItemDto { id = 10, id_string = "custom_item", item_type = "event_item", display_name = "Event Item" }
            };

            _window.SetStateForTesting(tabs, categories, globalItems);

            bool success = _window.TryBuildConfig(out var config, out string err, false);
            Assert.IsTrue(success, $"BuildConfig failed with: {err}");

            var custom = config.items.Find(i => i.id == 10);
            Assert.IsNotNull(custom);
            Assert.AreEqual("event_item", custom.item_type);
        }
    }
}

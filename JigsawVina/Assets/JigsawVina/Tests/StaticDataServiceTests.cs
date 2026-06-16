using NUnit.Framework;
using JigsawVina.Core.Services;
using JigsawVina.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JigsawVina.Tests
{
    public class StaticDataServiceTests
    {
        [Test]
        public void LoadFromText_ValidJson_ParsesSuccessfully()
        {
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
                    { ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24, ""star_reward"": 1, ""first_clear_coin"": 30, ""first_clear_hint"": 5, ""replay_coin"": 10, ""first_clear_reward_item_ids"": [101] }
                ]
            }";

            var service = new StaticDataService(false);
            service.LoadFromTextEnriched(json);

            var pictures = service.GetAllPictures();
            Assert.AreEqual(1, pictures.Count);
            Assert.AreEqual("Pic 1", pictures[0].DisplayName);

            var diff = service.GetPictureDifficulty(1, 0);
            Assert.AreEqual(6, diff.Columns);
            Assert.AreEqual(4, diff.Rows);
            Assert.AreEqual(30, diff.FirstClearCoin);
            Assert.AreEqual(5, diff.FirstClearHint);
            Assert.AreEqual(10, diff.ReplayCoin);
            Assert.AreEqual(1, diff.FirstClearRewardItemIds.Count);
            Assert.AreEqual(101, diff.FirstClearRewardItemIds[0]);

            var item = service.GetItemById(101);
            Assert.IsNotNull(item);
            Assert.AreEqual("Item 1", item.display_name);
        }

        [Test]
        public void LoadFromText_DuplicatePictureId_ThrowsException()
        {
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""categories"": [
                    { ""id"": 1, ""id_string"": ""vietnam_landscapes"", ""display_name"": ""Phong Cảnh Việt Nam"" }
                ],
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1, ""is_initially_unlocked"": true, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [] },
                    { ""id"": 1, ""id_string"": ""pic2"", ""display_name"": ""Pic 2"", ""category_id"": 1, ""is_initially_unlocked"": true, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [] }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromTextEnriched(json));
        }

        [Test]
        public void LoadFromText_DuplicatePictureIdString_ThrowsException()
        {
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""categories"": [
                    { ""id"": 1, ""id_string"": ""vietnam_landscapes"", ""display_name"": ""Phong Cảnh Việt Nam"" }
                ],
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1, ""is_initially_unlocked"": true, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [] },
                    { ""id"": 2, ""id_string"": ""pic1"", ""display_name"": ""Pic 2"", ""category_id"": 1, ""is_initially_unlocked"": true, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [] }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromTextEnriched(json));
        }

        [Test]
        public void LoadFromText_DuplicateItemId_ThrowsException()
        {
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""items"": [
                    { ""id"": 101, ""id_string"": ""item1"", ""display_name"": ""Item 1"" },
                    { ""id"": 101, ""id_string"": ""item2"", ""display_name"": ""Item 2"" }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromTextEnriched(json));
        }

        [Test]
        public void LoadFromText_DuplicateItemIdString_ThrowsException()
        {
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""items"": [
                    { ""id"": 101, ""id_string"": ""item1"", ""display_name"": ""Item 1"" },
                    { ""id"": 102, ""id_string"": ""item1"", ""display_name"": ""Item 2"" }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromTextEnriched(json));
        }

        [Test]
        public void LoadFromText_GridMismatch_ThrowsException()
        {
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""categories"": [
                    { ""id"": 1, ""id_string"": ""vietnam_landscapes"", ""display_name"": ""Phong Cảnh Việt Nam"" }
                ],
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1, ""is_initially_unlocked"": true, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [] }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 10 }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromTextEnriched(json));
        }

        [Test]
        public void LoadFromText_DifficultyRewardsMissingItem_ThrowsException()
        {
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""categories"": [
                    { ""id"": 1, ""id_string"": ""vietnam_landscapes"", ""display_name"": ""Phong Cảnh Việt Nam"" }
                ],
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1, ""is_initially_unlocked"": true, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [] }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24, ""first_clear_reward_item_ids"": [999] }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromTextEnriched(json));
        }

        [Test]
        public void LoadFromText_ValidDropTables_ParsesSuccessfully()
        {
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""categories"": [
                    { ""id"": 1, ""id_string"": ""vietnam_landscapes"", ""display_name"": ""Phong Cảnh Việt Nam"" }
                ],
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1, ""is_initially_unlocked"": true, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [] }
                ],
                ""items"": [
                    { ""id"": 10, ""id_string"": ""stamp"", ""display_name"": ""Stamp"", ""item_type"": ""consumable"", ""is_consumable"": true, ""max_stack"": 99, ""status"": ""active"" }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24, ""drop_table_id"": 1001 }
                ],
                ""drop_tables"": [
                    { ""id"": 1001, ""id_string"": ""table1"", ""display_name"": ""Table 1"", ""reset_rule"": ""daily"", ""status"": ""active"" }
                ],
                ""drop_table_items"": [
                    { ""id"": 11001, ""id_string"": ""drop1"", ""display_name"": ""Drop 1"", ""drop_table_id"": 1001, ""item_id"": 10, ""base_rate"": 0.5, ""decay_per_success"": 0.1, ""min_rate"": 0.1, ""amount_min"": 1, ""amount_max"": 2, ""status"": ""active"" }
                ]
            }";

            var service = new StaticDataService(false);
            service.LoadFromTextEnriched(json);

            var tables = service.GetAllDropTables();
            Assert.AreEqual(1, tables.Count);
            Assert.AreEqual("Table 1", tables[0].DisplayName);

            var items = service.GetDropTableItems(1001);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(11001, items[0].Id);
            Assert.AreEqual(10, items[0].ItemId);
            Assert.AreEqual(0.5f, items[0].BaseRate);

            var diff = service.GetPictureDifficulty(1, 0);
            Assert.AreEqual(1001, diff.DropTableId);
        }

        [Test]
        public void LoadFromText_DuplicateDropTableId_ThrowsException()
        {
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""drop_tables"": [
                    { ""id"": 1001, ""id_string"": ""table1"", ""display_name"": ""Table 1"", ""reset_rule"": ""daily"", ""status"": ""active"" },
                    { ""id"": 1001, ""id_string"": ""table2"", ""display_name"": ""Table 2"", ""reset_rule"": ""daily"", ""status"": ""active"" }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromTextEnriched(json));
        }

        [Test]
        public void LoadFromText_InvalidDropRate_ThrowsException()
        {
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""items"": [
                    { ""id"": 10, ""id_string"": ""stamp"", ""display_name"": ""Stamp"", ""item_type"": ""consumable"", ""is_consumable"": true, ""max_stack"": 99, ""status"": ""active"" }
                ],
                ""drop_tables"": [
                    { ""id"": 1001, ""id_string"": ""table1"", ""display_name"": ""Table 1"", ""reset_rule"": ""daily"", ""status"": ""active"" }
                ],
                ""drop_table_items"": [
                    { ""id"": 11001, ""id_string"": ""drop1"", ""display_name"": ""Drop 1"", ""drop_table_id"": 1001, ""item_id"": 10, ""base_rate"": 0.3, ""decay_per_success"": 0.1, ""min_rate"": 0.5, ""amount_min"": 1, ""amount_max"": 1, ""status"": ""active"" }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromTextEnriched(json));
        }

        [Test]
        public void LoadFromText_InvalidAmountRange_ThrowsException()
        {
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""items"": [
                    { ""id"": 10, ""id_string"": ""stamp"", ""display_name"": ""Stamp"", ""item_type"": ""consumable"", ""is_consumable"": true, ""max_stack"": 99, ""status"": ""active"" }
                ],
                ""drop_tables"": [
                    { ""id"": 1001, ""id_string"": ""table1"", ""display_name"": ""Table 1"", ""reset_rule"": ""daily"", ""status"": ""active"" }
                ],
                ""drop_table_items"": [
                    { ""id"": 11001, ""id_string"": ""drop1"", ""display_name"": ""Drop 1"", ""drop_table_id"": 1001, ""item_id"": 10, ""base_rate"": 0.5, ""decay_per_success"": 0.1, ""min_rate"": 0.1, ""amount_min"": 3, ""amount_max"": 2, ""status"": ""active"" }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromTextEnriched(json));
        }

        [Test]
        public void LoadFromText_MissingItemReference_ThrowsException()
        {
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""drop_tables"": [
                    { ""id"": 1001, ""id_string"": ""table1"", ""display_name"": ""Table 1"", ""reset_rule"": ""daily"", ""status"": ""active"" }
                ],
                ""drop_table_items"": [
                    { ""id"": 11001, ""id_string"": ""drop1"", ""display_name"": ""Drop 1"", ""drop_table_id"": 1001, ""item_id"": 99, ""base_rate"": 0.5, ""decay_per_success"": 0.1, ""min_rate"": 0.1, ""amount_min"": 1, ""amount_max"": 1, ""status"": ""active"" }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromTextEnriched(json));
        }

        [Test]
        public void LoadFromText_InactiveDropTableReference_ThrowsException()
        {
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""categories"": [
                    { ""id"": 1, ""id_string"": ""vietnam_landscapes"", ""display_name"": ""Phong Cảnh Việt Nam"" }
                ],
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1, ""is_initially_unlocked"": true, ""difficulty_unlock_policy"": ""sequential"", ""unlock_requirements"": [] }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24, ""drop_table_id"": 1001 }
                ],
                ""drop_tables"": [
                    { ""id"": 1001, ""id_string"": ""table1"", ""display_name"": ""Table 1"", ""reset_rule"": ""daily"", ""status"": ""inactive"" }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromTextEnriched(json));
        }

        [Test]
        public void LoadFromText_KeyItemAmountNotOne_ThrowsException()
        {
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""items"": [
                    { ""id"": 101, ""id_string"": ""key1"", ""display_name"": ""Key 1"", ""item_type"": ""key_item"", ""is_consumable"": false, ""max_stack"": 1, ""status"": ""active"" }
                ],
                ""drop_tables"": [
                    { ""id"": 1001, ""id_string"": ""table1"", ""display_name"": ""Table 1"", ""reset_rule"": ""daily"", ""status"": ""active"" }
                ],
                ""drop_table_items"": [
                    { ""id"": 11001, ""id_string"": ""drop1"", ""display_name"": ""Drop 1"", ""drop_table_id"": 1001, ""item_id"": 101, ""base_rate"": 0.5, ""decay_per_success"": 0.1, ""min_rate"": 0.1, ""amount_min"": 1, ""amount_max"": 2, ""status"": ""active"" }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromTextEnriched(json));
        }
    }
}


using NUnit.Framework;
using JigsawVina.Core.Services;
using JigsawVina.Core.Data;
using System;
using System.Collections.Generic;

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
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1, ""asset_path"": ""Textures/pic1"" }
                ],
                ""items"": [
                    { ""id"": 101, ""id_string"": ""item1"", ""display_name"": ""Item 1"", ""item_type"": ""key_item"", ""asset_path"": ""Items/item1"" }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24, ""star_reward"": 1, ""first_clear_coin"": 30, ""first_clear_hint"": 5, ""replay_coin"": 10, ""first_clear_reward_item_ids"": [101] }
                ]
            }";

            var service = new StaticDataService(false);
            service.LoadFromText(json);

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
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1 },
                    { ""id"": 1, ""id_string"": ""pic2"", ""display_name"": ""Pic 2"", ""category_id"": 1 }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
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
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1 },
                    { ""id"": 2, ""id_string"": ""pic1"", ""display_name"": ""Pic 2"", ""category_id"": 1 }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
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
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
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
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
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
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1 }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 10 }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
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
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1 }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24, ""first_clear_reward_item_ids"": [999] }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
        }
    }
}

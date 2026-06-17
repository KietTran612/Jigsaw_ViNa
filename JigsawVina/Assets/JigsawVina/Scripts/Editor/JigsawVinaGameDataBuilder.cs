using System;
using System.Collections.Generic;
using System.Linq;
using JigsawVina.Core.Data;
using UnityEditor;
using UnityEngine;

namespace JigsawVina.Editor
{
    internal sealed class JigsawVinaGameDataBuilder
    {
        internal delegate (Texture2D mainTexture, List<Texture2D> itemTextures) FolderScanner(DefaultAsset folderAsset);

        private readonly FolderScanner _scanFolder;

        public JigsawVinaGameDataBuilder(FolderScanner scanFolder)
        {
            _scanFolder = scanFolder;
        }

        public bool TryBuild(
            JigsawVinaGameDataBuildInput input,
            out StaticDataDto config,
            out string errorMessage,
            bool validateAssets)
        {
            config = new StaticDataDto
            {
                schema_version = 1,
                data_version = 1,
                drop_tables = new List<DropTableDto>(input.DropTables ?? new List<DropTableDto>()),
                drop_table_items = new List<DropTableItemDto>(input.DropTableItems ?? new List<DropTableItemDto>()),
                daily_rewards = new List<DailyRewardDto>()
            };
            errorMessage = "";

            // 1. Validate and Map Categories
            var categoryIds = new HashSet<int>();
            var categoryIdStrings = new HashSet<string>();

            foreach (var cat in input.Categories)
            {
                if (cat.id <= 0)
                {
                    errorMessage = $"ID Danh mục '{cat.displayName}' phải là số nguyên dương (> 0).";
                    return false;
                }
                if (string.IsNullOrEmpty(cat.idString))
                {
                    errorMessage = $"ID String của Danh mục ID {cat.id} không được để trống.";
                    return false;
                }
                if (!categoryIds.Add(cat.id))
                {
                    errorMessage = $"Trùng lặp ID Danh mục: {cat.id}.";
                    return false;
                }
                if (!categoryIdStrings.Add(cat.idString))
                {
                    errorMessage = $"Trùng lặp ID String Danh mục: '{cat.idString}'.";
                    return false;
                }

                config.categories.Add(new CategoryDto
                {
                    id = cat.id,
                    id_string = cat.idString,
                    display_name = cat.displayName,
                    display_name_key = string.IsNullOrEmpty(cat.displayNameKey) ? $"category.{cat.idString}.name" : cat.displayNameKey,
                    description_key = string.IsNullOrEmpty(cat.descriptionKey) ? $"category.{cat.idString}.description" : cat.descriptionKey
                });
            }

            // 2. Validate and Map Pictures and Items
            var pictureIds = new HashSet<int>();
            var pictureIdStrings = new HashSet<string>();
            var itemIds = new HashSet<int>();
            var itemIdStrings = new HashSet<string>();

            // Map Global Items first
            foreach (var item in input.GlobalItems)
            {
                if (item.id <= 0)
                {
                    errorMessage = $"ID Vật phẩm '{item.display_name}' phải là số nguyên dương (> 0).";
                    return false;
                }
                if (string.IsNullOrEmpty(item.id_string))
                {
                    errorMessage = $"ID String của Vật phẩm ID {item.id} không được để trống.";
                    return false;
                }
                if (item.item_type == "key_item")
                {
                    errorMessage = $"Vật phẩm Global '{item.display_name}' không được phép có item_type là 'key_item'.";
                    return false;
                }
                if (!itemIds.Add(item.id))
                {
                    errorMessage = $"Trùng lặp ID Vật phẩm Global: {item.id}.";
                    return false;
                }
                if (!itemIdStrings.Add(item.id_string))
                {
                    errorMessage = $"Trùng lặp ID String Vật phẩm Global: '{item.id_string}'.";
                    return false;
                }

                config.items.Add(item);
            }

            // Validate Reserved Items
            var coinItem = input.GlobalItems.FirstOrDefault(i => i.id == 1);
            if (coinItem == null || coinItem.id_string != "coin" || coinItem.item_type != "currency")
            {
                errorMessage = "Vật phẩm cốt lõi ID 1 (coin) phải tồn tại và có id_string là 'coin' với kiểu 'currency'.";
                return false;
            }
            var hintItem = input.GlobalItems.FirstOrDefault(i => i.id == 2);
            if (hintItem == null || hintItem.id_string != "hint" || hintItem.item_type != "currency")
            {
                errorMessage = "Vật phẩm cốt lõi ID 2 (hint) phải tồn tại và có id_string là 'hint' với kiểu 'currency'.";
                return false;
            }

            // Map pictures, scanned items, and difficulties
            foreach (var tab in input.Tabs)
            {
                if (tab.pictureId <= 0)
                {
                    errorMessage = $"ID Tranh '{tab.displayName}' phải là số nguyên dương (> 0).";
                    return false;
                }
                if (tab.pictureId >= 20000000)
                {
                    errorMessage = $"ID Tranh '{tab.pictureId}' quá lớn (phải nhỏ hơn 20,000,000) để tránh tràn số.";
                    return false;
                }
                if (string.IsNullOrEmpty(tab.idString))
                {
                    errorMessage = $"ID String của Tranh ID {tab.pictureId} không được để trống.";
                    return false;
                }
                if (!pictureIds.Add(tab.pictureId))
                {
                    errorMessage = $"Trùng lặp ID Tranh: {tab.pictureId}.";
                    return false;
                }
                if (!pictureIdStrings.Add(tab.idString))
                {
                    errorMessage = $"Trùng lặp ID String Tranh: '{tab.idString}'.";
                    return false;
                }
                if (!categoryIds.Contains(tab.categoryId))
                {
                    errorMessage = $"Tranh '{tab.displayName}' tham chiếu Danh mục ID {tab.categoryId} không tồn tại.";
                    return false;
                }

                string mainPath = "";
                string resourceFolder = "";
                List<string> itemFilenames = new();

                if (validateAssets)
                {
                    if (tab.folderAsset == null)
                    {
                        errorMessage = $"Tranh ID {tab.pictureId} chưa gán Thư mục tranh.";
                        return false;
                    }
                    string folderPath = AssetDatabase.GetAssetPath(tab.folderAsset);
                    if (!folderPath.StartsWith("Assets/Resources/"))
                    {
                        errorMessage = $"Thư mục '{folderPath}' phải nằm bên trong 'Assets/Resources/'.";
                        return false;
                    }

                    var (main, scannedItems) = _scanFolder(tab.folderAsset);
                    if (main == null)
                    {
                        errorMessage = $"Không tìm thấy ảnh chính 'MAIN_' trong thư mục: {tab.folderAsset.name}";
                        return false;
                    }
                    if (scannedItems.Count > 99)
                    {
                        errorMessage = $"Thư mục tranh '{tab.folderAsset.name}' có quá 99 key items (hiện có {scannedItems.Count}). Giới hạn tối đa là 99.";
                        return false;
                    }

                    resourceFolder = folderPath.Substring("Assets/Resources/".Length);
                    mainPath = $"{resourceFolder}/{main.name}";

                    foreach (var itTex in scannedItems)
                    {
                        itemFilenames.Add(itTex.name);
                    }
                }
                else
                {
                    // Mock path
                    mainPath = $"Textures/MAIN_mock_{tab.idString}";
                    resourceFolder = "Textures";

                    // Sort mock itemStates alphabetically using Ordinal comparison
                    tab.itemStates.Sort((a, b) => string.Compare(a.filename, b.filename, StringComparison.Ordinal));
                    if (tab.itemStates.Count > 99)
                    {
                        errorMessage = $"Tranh ID {tab.pictureId} có quá 99 key items. Giới hạn tối đa là 99.";
                        return false;
                    }

                    foreach (var itState in tab.itemStates)
                    {
                        itemFilenames.Add(itState.filename);
                    }
                }

                // Map pictureDto
                config.pictures.Add(new PictureDto
                {
                    id = tab.pictureId,
                    id_string = tab.idString,
                    display_name = tab.displayName,
                    category_id = tab.categoryId,
                    asset_path = mainPath,
                    difficulty_unlock_policy = string.IsNullOrEmpty(tab.difficultyUnlockPolicy) ? "sequential" : tab.difficultyUnlockPolicy,
                    display_name_key = string.IsNullOrEmpty(tab.displayNameKey) ? $"picture.{tab.idString}.name" : tab.displayNameKey,
                    description_key = string.IsNullOrEmpty(tab.descriptionKey) ? $"picture.{tab.idString}.description" : tab.descriptionKey,
                    is_initially_unlocked = tab.isInitiallyUnlocked,
                    unlock_requirements = tab.unlockRequirements != null ? new List<int>(tab.unlockRequirements) : new List<int>()
                });

                // Map scanned items DTO
                var localItems = new Dictionary<string, int>();
                for (int itemIndex = 0; itemIndex < itemFilenames.Count; itemIndex++)
                {
                    string filename = itemFilenames[itemIndex];
                    string itemIdString = filename.ToLower();

                    if (!itemIdStrings.Add(itemIdString))
                    {
                        errorMessage = $"Trùng lặp ID String Vật phẩm: '{itemIdString}'.";
                        return false;
                    }

                    int itemId = tab.pictureId * 100 + (itemIndex + 1);
                    if (!itemIds.Add(itemId))
                    {
                        errorMessage = $"Trùng lặp ID Vật phẩm: {itemId}.";
                        return false;
                    }

                    var itemState = tab.itemStates.Find(it => it.filename == filename);
                    string dispName = itemState != null ? itemState.displayName : filename.Replace("_", " ");
                    string desc = itemState != null ? itemState.description : "";
                    string rarity = itemState != null ? itemState.rarity : "common";
                    string itPath = $"{resourceFolder}/{filename}";

                    config.items.Add(new ItemDto
                    {
                        id = itemId,
                        id_string = itemIdString,
                        display_name = dispName,
                        description = desc,
                        display_name_key = (itemState != null && !string.IsNullOrEmpty(itemState.displayNameKey)) ? itemState.displayNameKey : $"item.{itemIdString}.name",
                        description_key = (itemState != null && !string.IsNullOrEmpty(itemState.descriptionKey)) ? itemState.descriptionKey : $"item.{itemIdString}.description",
                        item_type = "key_item",
                        rarity = rarity,
                        is_consumable = false,
                        is_time_limited = false,
                        max_stack = 1,
                        status = "active",
                        sort_order = itemId,
                        asset_path = itPath
                    });
                    localItems[filename] = itemId;
                }

                // Map difficulties
                AddDifficulty(config, tab.pictureId, 0, "Dễ", tab.easyCols, tab.easyRows, tab.easyCoins, tab.easyReplayCoins, tab.easyHints, tab.easyKeyRewardIndex, itemFilenames, localItems, tab.easyDropTableId);
                AddDifficulty(config, tab.pictureId, 1, "Trung bình", tab.normalCols, tab.normalRows, tab.normalCoins, tab.normalReplayCoins, tab.normalHints, tab.normalKeyRewardIndex, itemFilenames, localItems, tab.normalDropTableId);
                AddDifficulty(config, tab.pictureId, 2, "Khó", tab.hardCols, tab.hardRows, tab.hardCoins, tab.hardReplayCoins, tab.hardHints, tab.hardKeyRewardIndex, itemFilenames, localItems, tab.hardDropTableId);
            }

            if (input.DailyRewards == null || input.DailyRewards.Count != 7)
            {
                errorMessage = "Daily Rewards cấu hình phải có đúng 7 ngày.";
                return false;
            }

            var activeIds = new HashSet<int>();
            foreach (var item in input.GlobalItems)
            {
                activeIds.Add(item.id);
            }
            foreach (var tab in input.Tabs)
            {
                tab.itemStates.Sort((a, b) => string.Compare(a.filename, b.filename, StringComparison.Ordinal));
                for (int itemIndex = 0; itemIndex < tab.itemStates.Count; itemIndex++)
                {
                    activeIds.Add(tab.pictureId * 100 + (itemIndex + 1));
                }
            }

            for (int i = 0; i < 7; i++)
            {
                var dr = input.DailyRewards[i];
                if (dr.day_index != i + 1)
                {
                    errorMessage = $"Daily Reward index không hợp lệ tại dòng {i + 1}. Phải là Ngày {i + 1}.";
                    return false;
                }
                if (!activeIds.Contains(dr.item_id))
                {
                    errorMessage = $"Daily Reward Ngày {dr.day_index} tham chiếu Item ID {dr.item_id} không tồn tại hoặc không hoạt động.";
                    return false;
                }
                if (dr.amount <= 0)
                {
                    errorMessage = $"Daily Reward Ngày {dr.day_index} có amount {dr.amount} phải là số nguyên dương (> 0).";
                    return false;
                }

                // If it is a Key Item, amount must be exactly 1
                var rewardItem = input.GlobalItems.FirstOrDefault(item => item.id == dr.item_id);
                bool isKeyItem = false;
                if (rewardItem != null && rewardItem.item_type == "key_item")
                {
                    isKeyItem = true;
                }
                else if (rewardItem == null)
                {
                    foreach (var tab in input.Tabs)
                    {
                        for (int itIdx = 0; itIdx < tab.itemStates.Count; itIdx++)
                        {
                            int keyItemId = tab.pictureId * 100 + (itIdx + 1);
                            if (keyItemId == dr.item_id)
                            {
                                isKeyItem = true;
                                break;
                            }
                        }
                        if (isKeyItem) break;
                    }
                }

                if (isKeyItem && dr.amount != 1)
                {
                    errorMessage = $"Daily Reward Ngày {dr.day_index} là Key Item, số lượng (Amount) bắt buộc phải là 1.";
                    return false;
                }

                config.daily_rewards.Add(new DailyRewardDto
                {
                    day_index = dr.day_index,
                    item_id = dr.item_id,
                    amount = dr.amount
                });
            }

            // Sort DTOs for deterministic, clean JSON output
            config.categories.Sort((a, b) => a.id.CompareTo(b.id));
            config.pictures.Sort((a, b) => a.id.CompareTo(b.id));
            config.items.Sort((a, b) => a.id.CompareTo(b.id));
            config.picture_difficulties.Sort((a, b) =>
            {
                int comp = a.picture_id.CompareTo(b.picture_id);
                if (comp != 0) return comp;
                return a.difficulty_id.CompareTo(b.difficulty_id);
            });
            config.daily_rewards.Sort((a, b) => a.day_index.CompareTo(b.day_index));

            return true;
        }

        private void AddDifficulty(
            StaticDataDto config,
            int pictureId,
            int diffId,
            string displayName,
            int cols,
            int rows,
            int firstClearCoins,
            int replayCoins,
            int firstClearHints,
            int rewardIndex,
            List<string> items,
            Dictionary<string, int> localItems,
            int dropTableId)
        {
            var listRewards = new List<int>();
            if (rewardIndex > 0 && rewardIndex <= items.Count)
            {
                var texName = items[rewardIndex - 1];
                if (localItems.TryGetValue(texName, out int itemId))
                {
                    listRewards.Add(itemId);
                }
            }

            config.picture_difficulties.Add(new PictureDifficultyDto
            {
                picture_id = pictureId,
                difficulty_id = diffId,
                display_name = displayName,
                grid_columns = cols,
                grid_rows = rows,
                piece_count = cols * rows,
                star_reward = diffId + 1, // Easy = 1, Normal = 2, Hard = 3
                first_clear_coin = firstClearCoins,
                first_clear_hint = firstClearHints,
                replay_coin = replayCoins,
                first_clear_reward_item_ids = listRewards,
                drop_table_id = dropTableId
            });
        }
    }
}

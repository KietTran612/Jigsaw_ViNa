using System;
using System.Collections.Generic;
using System.Linq;
using JigsawVina.Core.Data;
using UnityEngine;

namespace JigsawVina.Core.Services
{
    public class StaticDataService : IStaticDataService
    {
        private const string StaticDataResourcePath = "GameData/jigsaw_vina_game_data";
        private List<PictureConfig> _pictures = new();
        private List<ItemDto> _items = new();
        private Dictionary<int, ItemDto> _itemsById = new();
        private Dictionary<(int PictureId, int DifficultyId), PictureDifficultyConfig> _difficulties = new();
        private List<DropTableConfig> _dropTables = new();
        private Dictionary<int, List<DropTableItemConfig>> _dropTableItemsByTableId = new();
        private List<DropTableItemConfig> _allDropTableItems = new();
        private List<DailyRewardConfig> _dailyRewards = new();

        public StaticDataService() : this(true)
        {
        }

        public StaticDataService(bool loadImmediately)
        {
            if (loadImmediately)
            {
                LoadFromJSON();
            }
        }

        public void LoadFromJSON()
        {
            var textAsset = Resources.Load<TextAsset>(StaticDataResourcePath);
            if (textAsset == null)
            {
                LoadFallbackData();
                return;
            }
            LoadFromText(textAsset.text);
            if (_pictures.Count == 0)
            {
                LoadFallbackData();
            }
        }

        private void LoadFallbackData()
        {
            _pictures = new List<PictureConfig>
            {
                new PictureConfig(1, "ho_guom", "Hồ Gươm", "Textures/ho_guom", "picture.ho_guom.name", "picture.ho_guom.description", true, "sequential", new List<int>()),
                new PictureConfig(2, "ha_long", "Vịnh Hạ Long", "Textures/ha_long", "picture.ha_long.name", "picture.ha_long.description", true, "sequential", new List<int>())
            };

            _difficulties = new Dictionary<(int, int), PictureDifficultyConfig>
            {
                [(1, 0)] = new PictureDifficultyConfig(1, 0, "Dễ", 6, 4, 1, 30, 0, 10, new List<int>()),
                [(1, 1)] = new PictureDifficultyConfig(1, 1, "Trung bình", 8, 6, 2, 60, 0, 20, new List<int>()),
                [(1, 2)] = new PictureDifficultyConfig(1, 2, "Khó", 12, 8, 3, 120, 0, 40, new List<int>()),
                [(2, 0)] = new PictureDifficultyConfig(2, 0, "Dễ", 6, 4, 1, 30, 0, 10, new List<int>()),
                [(2, 1)] = new PictureDifficultyConfig(2, 1, "Trung bình", 8, 6, 2, 60, 0, 20, new List<int>()),
                [(2, 2)] = new PictureDifficultyConfig(2, 2, "Khó", 12, 8, 3, 120, 0, 40, new List<int>())
            };

            _items = new List<ItemDto>();
            _itemsById = new Dictionary<int, ItemDto>();
            _dropTables = new List<DropTableConfig>();
            _allDropTableItems = new List<DropTableItemConfig>();
            _dropTableItemsByTableId = new Dictionary<int, List<DropTableItemConfig>>();
            _dailyRewards = new List<DailyRewardConfig>();
        }

        public void LoadFromText(string jsonText)
        {
            var dto = JsonUtility.FromJson<StaticDataDto>(jsonText);
            if (dto == null) return;

            // Defensive null initialization for missing JSON fields
            if (dto.pictures == null) dto.pictures = new List<PictureDto>();
            if (dto.items == null) dto.items = new List<ItemDto>();
            if (dto.picture_difficulties == null) dto.picture_difficulties = new List<PictureDifficultyDto>();
            if (dto.drop_tables == null) dto.drop_tables = new List<DropTableDto>();
            if (dto.drop_table_items == null) dto.drop_table_items = new List<DropTableItemDto>();
            if (dto.daily_rewards == null) dto.daily_rewards = new List<DailyRewardDto>();

            ValidateStaticData(dto);

            _items = dto.items;
            _itemsById = _items.ToDictionary(i => i.id);

            _pictures = dto.pictures.Select(p => new PictureConfig(
                p.id, 
                p.id_string, 
                p.display_name, 
                p.asset_path,
                p.display_name_key,
                p.description_key,
                p.is_initially_unlocked,
                p.difficulty_unlock_policy,
                p.unlock_requirements
            )).ToList();

            _difficulties = new Dictionary<(int, int), PictureDifficultyConfig>();
            foreach (var diff in dto.picture_difficulties)
            {
                var key = (diff.picture_id, diff.difficulty_id);
                var config = new PictureDifficultyConfig(
                    diff.picture_id,
                    diff.difficulty_id,
                    diff.display_name,
                    diff.grid_columns,
                    diff.grid_rows,
                    diff.star_reward,
                    diff.first_clear_coin,
                    diff.first_clear_hint,
                    diff.replay_coin,
                    diff.first_clear_reward_item_ids,
                    diff.drop_table_id
                );
                _difficulties[key] = config;
            }

            _dropTables = dto.drop_tables.Select(d => new DropTableConfig(
                d.id,
                d.id_string,
                d.display_name,
                d.display_name_key,
                d.description_key,
                d.reset_rule,
                d.status,
                d.sort_order
            )).ToList();

            _allDropTableItems = dto.drop_table_items.Select(di => new DropTableItemConfig(
                di.id,
                di.id_string,
                di.display_name,
                di.drop_table_id,
                di.item_id,
                di.base_rate,
                di.decay_per_success,
                di.min_rate,
                di.amount_min,
                di.amount_max,
                di.status
            )).ToList();

            _dropTableItemsByTableId = _allDropTableItems
                .GroupBy(di => di.DropTableId)
                .ToDictionary(g => g.Key, g => g.ToList());

            _dailyRewards = dto.daily_rewards.Select(dr => new DailyRewardConfig(
                dr.day_index,
                dr.item_id,
                dr.amount
            )).ToList();
        }

        private void ValidateStaticData(StaticDataDto dto)
        {
            if (dto.schema_version <= 0)
                throw new InvalidOperationException("schema_version must be a positive integer.");
            if (dto.data_version <= 0)
                throw new InvalidOperationException("data_version must be a positive integer.");

            var catIds = new HashSet<int>();
            if (dto.categories != null)
            {
                foreach (var cat in dto.categories)
                {
                    if (cat.id <= 0)
                        throw new InvalidOperationException($"Category ID {cat.id} must be a positive integer.");
                    if (string.IsNullOrEmpty(cat.id_string))
                        throw new InvalidOperationException($"Category ID {cat.id} has empty or null id_string.");
                    if (!catIds.Add(cat.id))
                        throw new InvalidOperationException($"Duplicate Category ID found: {cat.id}");
                }
            }

            var picIds = new HashSet<int>();
            var picIdStrings = new HashSet<string>();
            if (dto.pictures != null)
            {
                foreach (var p in dto.pictures)
                {
                    if (p.id <= 0)
                        throw new InvalidOperationException($"Picture ID {p.id} must be a positive integer.");
                    if (string.IsNullOrEmpty(p.id_string))
                        throw new InvalidOperationException($"Picture ID {p.id} has empty or null id_string.");
                    if (!picIds.Add(p.id))
                        throw new InvalidOperationException($"Duplicate Picture ID found: {p.id}");
                    if (!picIdStrings.Add(p.id_string))
                        throw new InvalidOperationException($"Duplicate Picture ID String found: {p.id_string}");
                    if (!catIds.Contains(p.category_id))
                        throw new InvalidOperationException($"Picture '{p.display_name}' (ID {p.id}) references missing Category ID {p.category_id}.");
                }
            }

            var itemIds = new HashSet<int>();
            var itemIdStrings = new HashSet<string>();
            var itemsById = new Dictionary<int, ItemDto>();
            if (dto.items != null)
            {
                foreach (var item in dto.items)
                {
                    if (item.id <= 0)
                        throw new InvalidOperationException($"Item ID {item.id} must be a positive integer.");
                    if (string.IsNullOrEmpty(item.id_string))
                        throw new InvalidOperationException($"Item ID {item.id} has empty or null id_string.");
                    if (!itemIds.Add(item.id))
                        throw new InvalidOperationException($"Duplicate Item ID found: {item.id}");
                    if (!itemIdStrings.Add(item.id_string))
                        throw new InvalidOperationException($"Duplicate Item ID String found: {item.id_string}");
                    itemsById.Add(item.id, item);
                }
            }

            // Validate Coin (1) and Hint (2) exist and are active
            if (!itemsById.TryGetValue(1, out var coinItem) || coinItem.status != "active")
            {
                throw new InvalidOperationException("Coin item (ID 1) must exist and be active in configuration.");
            }
            if (!itemsById.TryGetValue(2, out var hintItem) || hintItem.status != "active")
            {
                throw new InvalidOperationException("Hint item (ID 2) must exist and be active in configuration.");
            }

            // Validate Daily Rewards
            if (dto.daily_rewards == null || dto.daily_rewards.Count != 7)
                throw new InvalidOperationException("Daily rewards must contain exactly 7 configured rewards.");

            for (int i = 0; i < 7; i++)
            {
                var dr = dto.daily_rewards[i];
                if (dr.day_index != i + 1)
                    throw new InvalidOperationException($"Daily rewards day_index sequence must be exactly 1 to 7. Found {dr.day_index} at position {i}.");

                if (!itemsById.TryGetValue(dr.item_id, out var rewardItem))
                    throw new InvalidOperationException($"Daily reward Day {dr.day_index} references missing Item ID {dr.item_id}.");

                if (rewardItem.status != "active")
                    throw new InvalidOperationException($"Daily reward Day {dr.day_index} references inactive Item ID {dr.item_id}.");

                if (dr.amount <= 0)
                    throw new InvalidOperationException($"Daily reward Day {dr.day_index} amount must be greater than 0.");

                if (rewardItem.item_type == "key_item" && dr.amount != 1)
                    throw new InvalidOperationException($"Daily reward Day {dr.day_index} is a Key Item and amount must be exactly 1.");
            }

            var diffKeys = new HashSet<(int, int)>();
            var difficultiesByPicture = new Dictionary<int, List<PictureDifficultyDto>>();
            if (dto.picture_difficulties != null)
            {
                foreach (var diff in dto.picture_difficulties)
                {
                    if (!picIds.Contains(diff.picture_id))
                        throw new InvalidOperationException($"Difficulty references missing picture: {diff.picture_id}");

                    if (diff.difficulty_id < 0 || diff.difficulty_id > 2)
                        throw new InvalidOperationException($"Difficulty ID {diff.difficulty_id} must be within 0..2 (Easy, Normal, Hard).");

                    var key = (diff.picture_id, diff.difficulty_id);
                    if (!diffKeys.Add(key))
                        throw new InvalidOperationException($"Duplicate Difficulty configuration found for Picture {diff.picture_id}, Difficulty {diff.difficulty_id}.");

                    if (!difficultiesByPicture.TryGetValue(diff.picture_id, out var pictureDifficulties))
                    {
                        pictureDifficulties = new List<PictureDifficultyDto>();
                        difficultiesByPicture.Add(diff.picture_id, pictureDifficulties);
                    }
                    pictureDifficulties.Add(diff);

                    if (diff.grid_columns <= 0 || diff.grid_rows <= 0)
                        throw new InvalidOperationException($"Grid size columns ({diff.grid_columns}) and rows ({diff.grid_rows}) must be positive integers.");

                    if (diff.grid_columns * diff.grid_rows != diff.piece_count)
                        throw new InvalidOperationException($"Difficulty Grid size does not match piece count for picture {diff.picture_id}");

                    if (diff.first_clear_coin < 0 || diff.replay_coin < 0 || diff.first_clear_hint < 0)
                        throw new InvalidOperationException("Reward coin/hint values cannot be negative.");

                    if (diff.first_clear_reward_item_ids != null)
                    {
                        foreach (var rewardId in diff.first_clear_reward_item_ids)
                        {
                            if (!itemIds.Contains(rewardId))
                                throw new InvalidOperationException($"Difficulty rewards missing item ID: {rewardId}");
                        }
                    }
                }
            }

            // Validate Drop Tables
            var dropTableIds = new HashSet<int>();
            var dropTableIdStrings = new HashSet<string>();
            var activeDropTables = new HashSet<int>();
            
            if (dto.drop_tables != null)
            {
                foreach (var dt in dto.drop_tables)
                {
                    if (dt.id <= 0)
                        throw new InvalidOperationException($"Drop Table ID {dt.id} must be a positive integer.");
                    if (string.IsNullOrEmpty(dt.id_string))
                        throw new InvalidOperationException($"Drop Table ID {dt.id} has empty or null id_string.");
                    if (!dropTableIds.Add(dt.id))
                        throw new InvalidOperationException($"Duplicate Drop Table ID found: {dt.id}");
                    if (!dropTableIdStrings.Add(dt.id_string))
                        throw new InvalidOperationException($"Duplicate Drop Table ID String found: {dt.id_string}");
                    if (dt.status != "active" && dt.status != "inactive")
                        throw new InvalidOperationException($"Drop Table ID {dt.id} has invalid status '{dt.status}'.");
                    if (dt.reset_rule != "daily" && dt.reset_rule != "none")
                        throw new InvalidOperationException($"Drop Table ID {dt.id} has invalid reset_rule '{dt.reset_rule}'.");
                    if (dt.status == "active")
                    {
                        activeDropTables.Add(dt.id);
                    }
                }
            }

            var dropTableItemIds = new HashSet<int>();
            var dropTableItemIdStrings = new HashSet<string>();
            var dropTableItemsByTable = new Dictionary<int, List<DropTableItemDto>>();
            
            if (dto.drop_table_items != null)
            {
                foreach (var dti in dto.drop_table_items)
                {
                    if (dti.id <= 0)
                        throw new InvalidOperationException($"Drop Table Item ID {dti.id} must be a positive integer.");
                    if (string.IsNullOrEmpty(dti.id_string))
                        throw new InvalidOperationException($"Drop Table Item ID {dti.id} has empty or null id_string.");
                    if (!dropTableItemIds.Add(dti.id))
                        throw new InvalidOperationException($"Duplicate Drop Table Item ID found: {dti.id}");
                    if (!dropTableItemIdStrings.Add(dti.id_string))
                        throw new InvalidOperationException($"Duplicate Drop Table Item ID String found: {dti.id_string}");

                    if (dti.base_rate < 0f || dti.base_rate > 1f)
                        throw new InvalidOperationException($"Drop Table Item ID {dti.id} base_rate {dti.base_rate} must be between 0 and 1.");
                    if (dti.min_rate < 0f || dti.min_rate > 1f)
                        throw new InvalidOperationException($"Drop Table Item ID {dti.id} min_rate {dti.min_rate} must be between 0 and 1.");
                    if (dti.min_rate > dti.base_rate)
                        throw new InvalidOperationException($"Drop Table Item ID {dti.id} min_rate {dti.min_rate} cannot exceed base_rate {dti.base_rate}.");
                    if (dti.decay_per_success < 0f)
                        throw new InvalidOperationException($"Drop Table Item ID {dti.id} decay_per_success {dti.decay_per_success} must be non-negative.");

                    if (dti.amount_min <= 0)
                        throw new InvalidOperationException($"Drop Table Item ID {dti.id} amount_min {dti.amount_min} must be positive.");
                    if (dti.amount_max < dti.amount_min)
                        throw new InvalidOperationException($"Drop Table Item ID {dti.id} amount_max {dti.amount_max} must be >= amount_min {dti.amount_min}.");
                    if (dti.amount_max >= int.MaxValue)
                        throw new InvalidOperationException($"Drop Table Item ID {dti.id} amount_max {dti.amount_max} must be less than int.MaxValue.");

                    if (dti.status != "active" && dti.status != "inactive")
                        throw new InvalidOperationException($"Drop Table Item ID {dti.id} status must be 'active' or 'inactive'.");

                    if (!dropTableIds.Contains(dti.drop_table_id))
                        throw new InvalidOperationException($"Drop Table Item ID {dti.id} references missing Drop Table ID {dti.drop_table_id}.");

                    if (!itemsById.TryGetValue(dti.item_id, out var item))
                        throw new InvalidOperationException($"Drop Table Item ID {dti.id} references missing Item ID {dti.item_id}.");

                    if (item.status != "active")
                        throw new InvalidOperationException($"Drop Table Item ID {dti.id} references inactive Item ID {dti.item_id}.");

                    bool isValidType = item.id == 1 || item.id == 2 || item.item_type == "key_item" || item.item_type == "consumable";
                    if (!isValidType)
                        throw new InvalidOperationException($"Drop Table Item ID {dti.id} references item {dti.item_id} of invalid type '{item.item_type}'.");

                    if (item.item_type == "key_item")
                    {
                        if (item.is_consumable)
                            throw new InvalidOperationException($"Drop Table Item ID {dti.id} references Key Item {dti.item_id} which is consumable.");
                        if (item.max_stack != 1)
                            throw new InvalidOperationException($"Drop Table Item ID {dti.id} references Key Item {dti.item_id} with max_stack {item.max_stack} != 1.");
                        if (dti.amount_min != 1 || dti.amount_max != 1)
                            throw new InvalidOperationException($"Drop Table Item ID {dti.id} references Key Item {dti.item_id} but drop amount is not exactly 1.");
                    }
                    else if (item.item_type == "consumable")
                    {
                        if (!item.is_consumable)
                            throw new InvalidOperationException($"Drop Table Item ID {dti.id} references Consumable Item {dti.item_id} which has is_consumable == false.");
                        if (item.max_stack <= 0)
                            throw new InvalidOperationException($"Drop Table Item ID {dti.id} references Consumable Item {dti.item_id} with max_stack {item.max_stack} <= 0.");
                    }

                    if (!dropTableItemsByTable.TryGetValue(dti.drop_table_id, out var list))
                    {
                        list = new List<DropTableItemDto>();
                        dropTableItemsByTable.Add(dti.drop_table_id, list);
                    }
                    if (list.Any(existing => existing.item_id == dti.item_id))
                    {
                        throw new InvalidOperationException($"Drop Table ID {dti.drop_table_id} has duplicate item_id {dti.item_id}.");
                    }
                    list.Add(dti);
                }
            }

            if (dto.picture_difficulties != null)
            {
                foreach (var diff in dto.picture_difficulties)
                {
                    if (diff.drop_table_id > 0)
                    {
                        if (!dropTableIds.Contains(diff.drop_table_id))
                        {
                            throw new InvalidOperationException($"Difficulty for Picture {diff.picture_id}, Difficulty {diff.difficulty_id} references missing Drop Table ID {diff.drop_table_id}.");
                        }
                        if (!activeDropTables.Contains(diff.drop_table_id))
                        {
                            throw new InvalidOperationException($"Difficulty for Picture {diff.picture_id}, Difficulty {diff.difficulty_id} references inactive Drop Table ID {diff.drop_table_id}.");
                        }
                    }
                }
            }

            ValidateUnlockConfiguration(dto.pictures, itemsById, difficultiesByPicture);
            ValidateProgressionReachability(dto.pictures, difficultiesByPicture);
        }

        private static void ValidateUnlockConfiguration(
            IReadOnlyList<PictureDto> pictures,
            IReadOnlyDictionary<int, ItemDto> itemsById,
            IReadOnlyDictionary<int, List<PictureDifficultyDto>> difficultiesByPicture)
        {
            foreach (var picture in pictures)
            {
                picture.unlock_requirements ??= new List<int>();

                if (picture.difficulty_unlock_policy != "sequential" &&
                    picture.difficulty_unlock_policy != "all_unlocked")
                {
                    throw new InvalidOperationException(
                        $"Picture {picture.id} has invalid difficulty unlock policy '{picture.difficulty_unlock_policy}'.");
                }

                var uniqueRequirements = new HashSet<int>();
                foreach (int requirementId in picture.unlock_requirements)
                {
                    if (!uniqueRequirements.Add(requirementId))
                    {
                        throw new InvalidOperationException(
                            $"Picture {picture.id} has duplicate unlock requirement item {requirementId}.");
                    }

                    if (!itemsById.TryGetValue(requirementId, out var item))
                    {
                        throw new InvalidOperationException(
                            $"Picture {picture.id} requires missing item ID {requirementId}.");
                    }

                    if (item.item_type != "key_item" ||
                        item.is_consumable ||
                        item.status != "active")
                    {
                        throw new InvalidOperationException(
                            $"Picture {picture.id} unlock requirement {requirementId} must be an active, non-consumable key item.");
                    }
                }

                if (!difficultiesByPicture.TryGetValue(picture.id, out var difficulties) ||
                    difficulties.All(difficulty => difficulty.difficulty_id != 0))
                {
                    throw new InvalidOperationException(
                        $"Picture {picture.id} must configure difficulty 0.");
                }

                if (picture.difficulty_unlock_policy != "sequential")
                {
                    continue;
                }

                var configuredIds = new HashSet<int>(
                    difficulties.Select(difficulty => difficulty.difficulty_id));
                int maximumDifficultyId = configuredIds.Max();
                for (int difficultyId = 0; difficultyId <= maximumDifficultyId; difficultyId++)
                {
                    if (!configuredIds.Contains(difficultyId))
                    {
                        throw new InvalidOperationException(
                            $"Picture {picture.id} sequential difficulties must be contiguous from 0.");
                    }
                }
            }
        }

        private static void ValidateProgressionReachability(
            IReadOnlyList<PictureDto> pictures,
            IReadOnlyDictionary<int, List<PictureDifficultyDto>> difficultiesByPicture)
        {
            var unlockedPictures = new HashSet<int>(
                pictures.Where(picture => picture.is_initially_unlocked)
                    .Select(picture => picture.id));

            bool unlockedAny;
            do
            {
                unlockedAny = false;
                foreach (var picture in pictures)
                {
                    if (unlockedPictures.Contains(picture.id))
                    {
                        continue;
                    }

                    bool allRequirementsReachable = picture.unlock_requirements.All(requirementId =>
                        IsItemReachable(requirementId, unlockedPictures, difficultiesByPicture));
                    if (allRequirementsReachable)
                    {
                        unlockedPictures.Add(picture.id);
                        unlockedAny = true;
                    }
                }
            } while (unlockedAny);

            if (unlockedPictures.Count == pictures.Count)
            {
                return;
            }

            string lockedIds = string.Join(", ", pictures
                .Where(picture => !unlockedPictures.Contains(picture.id))
                .Select(picture => picture.id));
            throw new InvalidOperationException(
                $"Progression deadlock detected. Unreachable picture IDs: {lockedIds}.");
        }

        private static bool IsItemReachable(
            int itemId,
            HashSet<int> unlockedPictures,
            IReadOnlyDictionary<int, List<PictureDifficultyDto>> difficultiesByPicture)
        {
            foreach (int pictureId in unlockedPictures)
            {
                if (!difficultiesByPicture.TryGetValue(pictureId, out var difficulties))
                {
                    continue;
                }

                foreach (var difficulty in difficulties)
                {
                    if (difficulty.first_clear_reward_item_ids != null &&
                        difficulty.first_clear_reward_item_ids.Contains(itemId))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public IReadOnlyList<PictureConfig> GetAllPictures() => _pictures;

        public PictureConfig GetPictureById(int id)
        {
            return _pictures.FirstOrDefault(p => p.Id == id);
        }

        public PictureDifficultyConfig GetPictureDifficulty(int pictureId, int difficultyId)
        {
            var picture = GetPictureById(pictureId);
            if (picture.Id == 0)
            {
                throw new KeyNotFoundException($"Picture with ID {pictureId} not found.");
            }

            if (_difficulties.TryGetValue((pictureId, difficultyId), out var config))
            {
                return config;
            }

            throw new KeyNotFoundException(
                $"Difficulty with ID {difficultyId} was not configured for picture {pictureId}.");
        }

        public ItemDto GetItemById(int id)
        {
            if (_itemsById.TryGetValue(id, out var item))
            {
                return item;
            }
            return null;
        }

        public IReadOnlyList<ItemDto> GetAllItems() => _items;

        public IReadOnlyList<PictureDifficultyConfig> GetPictureDifficulties(int pictureId)
        {
            var list = new List<PictureDifficultyConfig>();
            foreach (var kvp in _difficulties)
            {
                if (kvp.Key.PictureId == pictureId)
                {
                    list.Add(kvp.Value);
                }
            }
            return list;
        }

        public IReadOnlyList<PictureDifficultyConfig> GetAllPictureDifficulties()
        {
            return _difficulties.Values.ToList();
        }

        public IReadOnlyList<DropTableConfig> GetAllDropTables() => _dropTables;
        
        public IReadOnlyList<DropTableItemConfig> GetDropTableItems(int dropTableId)
        {
            if (_dropTableItemsByTableId.TryGetValue(dropTableId, out var items))
            {
                return items;
            }
            return new List<DropTableItemConfig>();
        }

        public IReadOnlyList<DropTableItemConfig> GetAllDropTableItems() => _allDropTableItems;
        public IReadOnlyList<DailyRewardConfig> GetDailyRewards() => _dailyRewards;
    }
}

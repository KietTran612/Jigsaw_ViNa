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

            StaticDataValidator.Validate(dto);

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

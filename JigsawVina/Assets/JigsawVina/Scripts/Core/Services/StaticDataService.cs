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

            var catalog = StaticDataCatalogBuilder.Build(dto);
            _pictures = catalog.Pictures;
            _items = catalog.Items;
            _itemsById = catalog.ItemsById;
            _difficulties = catalog.Difficulties;
            _dropTables = catalog.DropTables;
            _dropTableItemsByTableId = catalog.DropTableItemsByTableId;
            _allDropTableItems = catalog.AllDropTableItems;
            _dailyRewards = catalog.DailyRewards;
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

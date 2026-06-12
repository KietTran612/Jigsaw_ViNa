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
                new PictureConfig(1, "ho_guom", "Hồ Gươm", "Textures/ho_guom", "picture.ho_guom.name", "picture.ho_guom.description"),
                new PictureConfig(2, "ha_long", "Vịnh Hạ Long", "Textures/ha_long", "picture.ha_long.name", "picture.ha_long.description")
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
        }

        public void LoadFromText(string jsonText)
        {
            var dto = JsonUtility.FromJson<StaticDataDto>(jsonText);
            if (dto == null) return;

            // Defensive null initialization for missing JSON fields
            if (dto.pictures == null) dto.pictures = new List<PictureDto>();
            if (dto.items == null) dto.items = new List<ItemDto>();
            if (dto.picture_difficulties == null) dto.picture_difficulties = new List<PictureDifficultyDto>();

            ValidateStaticData(dto);

            _items = dto.items;
            _itemsById = _items.ToDictionary(i => i.id);

            _pictures = dto.pictures.Select(p => new PictureConfig(
                p.id, 
                p.id_string, 
                p.display_name, 
                p.asset_path,
                p.display_name_key,
                p.description_key
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
                    diff.first_clear_reward_item_ids
                );
                _difficulties[key] = config;
            }
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
                }
            }

            var diffKeys = new HashSet<(int, int)>();
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
    }
}

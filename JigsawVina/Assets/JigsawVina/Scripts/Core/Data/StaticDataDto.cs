using System;
using System.Collections.Generic;

namespace JigsawVina.Core.Data
{
    [Serializable]
    public class CategoryDto
    {
        public int id;
        public string id_string;
        public string display_name;
    }

    [Serializable]
    public class PictureDto
    {
        public int id;
        public string id_string;
        public string display_name;
        public int category_id;
        public string asset_path;
        public string difficulty_unlock_policy;
    }

    [Serializable]
    public class ItemDto
    {
        public int id;
        public string id_string;
        public string display_name;
        public string description;
        public string display_name_key;
        public string description_key;
        public string item_type;
        public string rarity;
        public bool is_consumable;
        public bool is_time_limited;
        public int max_stack;
        public string status;
        public int sort_order;
        public string asset_path;
    }

    [Serializable]
    public class PictureDifficultyDto
    {
        public int picture_id;
        public int difficulty_id;
        public string display_name;
        public int grid_columns;
        public int grid_rows;
        public int piece_count;
        public int star_reward;
        public int first_clear_coin;
        public int first_clear_hint;
        public int replay_coin;
        public List<int> first_clear_reward_item_ids = new();
    }

    [Serializable]
    public class StaticDataDto
    {
        public int schema_version = 1;
        public int data_version = 1;
        public List<CategoryDto> categories = new();
        public List<PictureDto> pictures = new();
        public List<ItemDto> items = new();
        public List<PictureDifficultyDto> picture_difficulties = new();
    }
}

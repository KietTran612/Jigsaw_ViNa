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
        public string display_name_key;
        public string description_key;
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
        public string display_name_key;
        public string description_key;
        public bool is_initially_unlocked;
        public List<int> unlock_requirements = new();
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
        public int drop_table_id;
    }

    [Serializable]
    public class DropTableDto
    {
        public int id;
        public string id_string;
        public string display_name;
        public string display_name_key;
        public string description_key;
        public string reset_rule;
        public string status;
        public int sort_order;
    }

    [Serializable]
    public class DropTableItemDto
    {
        public int id;
        public string id_string;
        public string display_name;
        public int drop_table_id;
        public int item_id;
        public float base_rate;
        public float decay_per_success;
        public float min_rate;
        public int amount_min;
        public int amount_max;
        public string status;
    }

    [Serializable]
    public class DailyRewardDto
    {
        public int day_index;
        public int item_id;
        public int amount;
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
        public List<DropTableDto> drop_tables = new();
        public List<DropTableItemDto> drop_table_items = new();
        public List<DailyRewardDto> daily_rewards = new();
    }
}

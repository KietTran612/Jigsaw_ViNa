using System.Collections.Generic;
using System.Linq;
using JigsawVina.Core.Data;

namespace JigsawVina.Core.Services
{
    internal sealed class StaticDataCatalog
    {
        public List<PictureConfig> Pictures { get; set; } = new();
        public List<ItemDto> Items { get; set; } = new();
        public Dictionary<int, ItemDto> ItemsById { get; set; } = new();
        public Dictionary<(int PictureId, int DifficultyId), PictureDifficultyConfig> Difficulties { get; set; } = new();
        public List<DropTableConfig> DropTables { get; set; } = new();
        public Dictionary<int, List<DropTableItemConfig>> DropTableItemsByTableId { get; set; } = new();
        public List<DropTableItemConfig> AllDropTableItems { get; set; } = new();
        public List<DailyRewardConfig> DailyRewards { get; set; } = new();
    }

    internal static class StaticDataCatalogBuilder
    {
        public static StaticDataCatalog Build(StaticDataDto dto)
        {
            var pictures = dto.pictures.Select(p => new PictureConfig(
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

            var items = dto.items;
            var itemsById = items.ToDictionary(i => i.id);

            var difficulties = new Dictionary<(int, int), PictureDifficultyConfig>();
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
                difficulties[key] = config;
            }

            var dropTables = dto.drop_tables.Select(d => new DropTableConfig(
                d.id,
                d.id_string,
                d.display_name,
                d.display_name_key,
                d.description_key,
                d.reset_rule,
                d.status,
                d.sort_order
            )).ToList();

            var allDropTableItems = dto.drop_table_items.Select(di => new DropTableItemConfig(
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

            var dropTableItemsByTableId = allDropTableItems
                .GroupBy(di => di.DropTableId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var dailyRewards = dto.daily_rewards.Select(dr => new DailyRewardConfig(
                dr.day_index,
                dr.item_id,
                dr.amount
            )).ToList();

            return new StaticDataCatalog
            {
                Pictures = pictures,
                Items = items,
                ItemsById = itemsById,
                Difficulties = difficulties,
                DropTables = dropTables,
                AllDropTableItems = allDropTableItems,
                DropTableItemsByTableId = dropTableItemsByTableId,
                DailyRewards = dailyRewards
            };
        }
    }
}

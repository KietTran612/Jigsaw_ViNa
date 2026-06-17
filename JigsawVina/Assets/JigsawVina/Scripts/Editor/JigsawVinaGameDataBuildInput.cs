using System.Collections.Generic;
using JigsawVina.Core.Data;

namespace JigsawVina.Editor
{
    internal sealed class JigsawVinaGameDataBuildInput
    {
        public JigsawVinaGameDataBuildInput(
            IReadOnlyList<JigsawVinaGameDataEditor.EditorTabState> tabs,
            IReadOnlyList<JigsawVinaGameDataEditor.EditorCategoryState> categories,
            IReadOnlyList<ItemDto> globalItems,
            IReadOnlyList<DropTableDto> dropTables,
            IReadOnlyList<DropTableItemDto> dropTableItems,
            IReadOnlyList<DailyRewardDto> dailyRewards)
        {
            Tabs = tabs ?? new List<JigsawVinaGameDataEditor.EditorTabState>();
            Categories = categories ?? new List<JigsawVinaGameDataEditor.EditorCategoryState>();
            GlobalItems = globalItems ?? new List<ItemDto>();
            DropTables = dropTables ?? new List<DropTableDto>();
            DropTableItems = dropTableItems ?? new List<DropTableItemDto>();
            DailyRewards = dailyRewards ?? new List<DailyRewardDto>();
        }

        public IReadOnlyList<JigsawVinaGameDataEditor.EditorTabState> Tabs { get; }
        public IReadOnlyList<JigsawVinaGameDataEditor.EditorCategoryState> Categories { get; }
        public IReadOnlyList<ItemDto> GlobalItems { get; }
        public IReadOnlyList<DropTableDto> DropTables { get; }
        public IReadOnlyList<DropTableItemDto> DropTableItems { get; }
        public IReadOnlyList<DailyRewardDto> DailyRewards { get; }
    }
}

using System.Collections.Generic;
using JigsawVina.Core.Data;

namespace JigsawVina.Core.Services
{
    public interface IStaticDataService
    {
        IReadOnlyList<PictureConfig> GetAllPictures();
        PictureConfig GetPictureById(int id);
        PictureDifficultyConfig GetPictureDifficulty(int pictureId, int difficultyId);
        ItemDto GetItemById(int id);
        IReadOnlyList<ItemDto> GetAllItems();
        IReadOnlyList<PictureDifficultyConfig> GetPictureDifficulties(int pictureId);
        IReadOnlyList<PictureDifficultyConfig> GetAllPictureDifficulties();
        IReadOnlyList<DropTableConfig> GetAllDropTables();
        IReadOnlyList<DropTableItemConfig> GetDropTableItems(int dropTableId);
        IReadOnlyList<DropTableItemConfig> GetAllDropTableItems();
    }
}

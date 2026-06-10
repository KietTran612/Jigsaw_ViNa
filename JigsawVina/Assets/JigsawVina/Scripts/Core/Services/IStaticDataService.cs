using System.Collections.Generic;
using JigsawVina.Core.Data;

namespace JigsawVina.Core.Services
{
    public interface IStaticDataService
    {
        IReadOnlyList<PictureConfig> GetAllPictures();
        PictureConfig GetPictureById(int id);
    }
}

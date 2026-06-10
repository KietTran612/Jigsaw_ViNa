using JigsawVina.Core.Data;

namespace JigsawVina.Core.Services
{
    public interface ISaveDataService
    {
        PlayerSave Load();
        void Save(PlayerSave save);
    }
}

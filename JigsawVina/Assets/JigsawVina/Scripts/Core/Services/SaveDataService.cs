using JigsawVina.Core.Data;
using UnityEngine;

namespace JigsawVina.Core.Services
{
    public class SaveDataService : ISaveDataService
    {
        public const string SaveKey = "JigsawVina_PlayerSave";
        private readonly ILocalDateProvider _localDateProvider;

        public SaveDataService() : this(new LocalDateProvider())
        {
        }

        public SaveDataService(ILocalDateProvider localDateProvider)
        {
            _localDateProvider = localDateProvider;
        }

        public PlayerSave Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                var newSave = new PlayerSave();
                newSave.Normalize(_localDateProvider.GetCurrentLocalDateString());
                return newSave;
            }
            string json = PlayerPrefs.GetString(SaveKey);
            var save = JsonUtility.FromJson<PlayerSave>(json) ?? new PlayerSave();
            save.Normalize(_localDateProvider.GetCurrentLocalDateString());
            return save;
        }

        public void Save(PlayerSave save)
        {
            string json = JsonUtility.ToJson(save);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }
    }
}

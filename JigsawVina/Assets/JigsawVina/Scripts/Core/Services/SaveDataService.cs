using JigsawVina.Core.Data;
using UnityEngine;

namespace JigsawVina.Core.Services
{
    public class SaveDataService : ISaveDataService
    {
        public const string SaveKey = "JigsawVina_PlayerSave";

        public PlayerSave Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                var newSave = new PlayerSave();
                newSave.Normalize();
                return newSave;
            }
            string json = PlayerPrefs.GetString(SaveKey);
            var save = JsonUtility.FromJson<PlayerSave>(json) ?? new PlayerSave();
            save.Normalize();
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

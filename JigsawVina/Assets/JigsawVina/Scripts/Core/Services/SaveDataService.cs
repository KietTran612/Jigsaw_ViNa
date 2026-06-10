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
                return new PlayerSave();
            }
            string json = PlayerPrefs.GetString(SaveKey);
            return JsonUtility.FromJson<PlayerSave>(json) ?? new PlayerSave();
        }

        public void Save(PlayerSave save)
        {
            string json = JsonUtility.ToJson(save);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }
    }
}

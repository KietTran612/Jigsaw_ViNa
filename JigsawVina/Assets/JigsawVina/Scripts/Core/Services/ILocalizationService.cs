using System;

namespace JigsawVina.Core.Services
{
    public interface ILocalizationService
    {
        event Action OnLanguageChanged;
        string CurrentLanguage { get; }
        void SetLanguage(string langCode);
        string Get(string key);
        string GetFormat(string key, params object[] args);
    }
}

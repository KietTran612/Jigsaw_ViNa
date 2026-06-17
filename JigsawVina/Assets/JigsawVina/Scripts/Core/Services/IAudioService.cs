namespace JigsawVina.Core.Services
{
    public interface IAudioService
    {
        void PlayBGM(string clipPath, bool loop = true, float fadeDuration = 0.5f);
        void StopBGM(float fadeDuration = 0.5f);
        void PlaySFX(string clipPath, float volumeScale = 1f);
        void SetMusicEnabled(bool enabled);
        void SetSfxEnabled(bool enabled);
    }
}

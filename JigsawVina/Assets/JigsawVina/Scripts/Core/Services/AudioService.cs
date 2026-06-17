using System;
using System.Collections.Generic;
using UnityEngine;

namespace JigsawVina.Core.Services
{
    public class AudioService : IAudioService, IDisposable
    {
        private readonly ISaveDataService _saveDataService;
        private readonly HashSet<string> _missingClipWarnings = new();
        private GameObject _runtimeGo;
        private AudioSource _musicSource;
        private AudioSource _sfxSource;

        public AudioService(ISaveDataService saveDataService)
        {
            _saveDataService = saveDataService;
            Initialize();
        }

        private void Initialize()
        {
            // Clean up existing duplicates safely
            var existing = GameObject.Find("AudioServiceRuntime");
            if (existing != null)
            {
                if (Application.isPlaying)
                {
                    existing.name = "AudioServiceRuntime_Destroying";
                    UnityEngine.Object.Destroy(existing);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(existing);
                }
            }

            _runtimeGo = new GameObject("AudioServiceRuntime");
            UnityEngine.Object.DontDestroyOnLoad(_runtimeGo);

            _musicSource = _runtimeGo.AddComponent<AudioSource>();
            _musicSource.loop = true;

            _sfxSource = _runtimeGo.AddComponent<AudioSource>();

            // Load settings
            var save = _saveDataService.Load();
            _musicSource.mute = (save.MusicEnabledState == 0);
            _sfxSource.mute = (save.SfxEnabledState == 0);
        }

        public void PlayBGM(string clipPath, bool loop = true, float fadeDuration = 0.5f)
        {
            if (string.IsNullOrEmpty(clipPath)) return;

            var clip = Resources.Load<AudioClip>(clipPath);
            if (clip == null)
            {
                LogMissingClipOnce(clipPath);
                return;
            }

            if (_musicSource.clip == clip && _musicSource.isPlaying) return;

            // Simple transition (for now direct assign, fading can be added if needed via Coroutine/DOTween, but direct works safely)
            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.Play();
        }

        public void StopBGM(float fadeDuration = 0.5f)
        {
            _musicSource.Stop();
        }

        public void PlaySFX(string clipPath, float volumeScale = 1f)
        {
            if (string.IsNullOrEmpty(clipPath)) return;

            var clip = Resources.Load<AudioClip>(clipPath);
            if (clip == null)
            {
                LogMissingClipOnce(clipPath);
                return;
            }

            _sfxSource.PlayOneShot(clip, volumeScale);
        }

        private void LogMissingClipOnce(string clipPath)
        {
            if (_missingClipWarnings.Add(clipPath))
            {
                Debug.LogWarning($"[AudioService] AudioClip not found at path: {clipPath}");
            }
        }

        public void SetMusicEnabled(bool enabled)
        {
            _musicSource.mute = !enabled;

            var save = _saveDataService.Load();
            save.MusicEnabledState = enabled ? 1 : 0;
            _saveDataService.Save(save);
        }

        public void SetSfxEnabled(bool enabled)
        {
            _sfxSource.mute = !enabled;

            var save = _saveDataService.Load();
            save.SfxEnabledState = enabled ? 1 : 0;
            _saveDataService.Save(save);
        }

        public void Dispose()
        {
            if (_runtimeGo != null)
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(_runtimeGo);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(_runtimeGo);
                }
                _runtimeGo = null;
            }
        }
    }
}

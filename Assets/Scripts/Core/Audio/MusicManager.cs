using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicManager : MonoBehaviour
    {
        private const string MusicEnabledKey = "music_enabled";
        private const string DefaultMusicResourcePath = "Audio/Music/curious_alchemist";

        public static MusicManager Instance { get; private set; }

        [Header("Default Clips")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip labMusic;
        [SerializeField] private AudioClip winMusic;

        [Header("Music Settings")]
        [SerializeField, Range(0f, 1f)] private float defaultVolume = 0.55f;
        [SerializeField] private bool loop = true;

        [Header("Fade Settings")]
        [SerializeField] private float defaultFadeInDuration = 1.6f;
        [SerializeField] private float defaultFadeOutDuration = 0.8f;
        [SerializeField] private float defaultCrossFadeDuration = 1.0f;

        private AudioSource _audioSource;
        private Coroutine _fadeCoroutine;
        private bool _isMusicEnabled = true;

        public float CurrentVolume => _audioSource != null ? _audioSource.volume : 0f;
        public AudioClip CurrentClip => _audioSource != null ? _audioSource.clip : null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _audioSource = GetComponent<AudioSource>();
            ConfigureAudioSource();
            LoadPreferences();
            LoadDefaultClipsIfNeeded();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void ConfigureAudioSource()
        {
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();

            _audioSource.playOnAwake = false;
            _audioSource.loop = loop;
            _audioSource.volume = 0f;
            _audioSource.clip = null;
        }

        private void LoadDefaultClipsIfNeeded()
        {
            AudioClip fallback = Resources.Load<AudioClip>(DefaultMusicResourcePath);

            if (fallback == null)
            {
                Debug.LogWarning($"[MusicManager] Não foi possível carregar o áudio em Resources/{DefaultMusicResourcePath}");
                return;
            }

            if (menuMusic == null)
                menuMusic = fallback;

            if (labMusic == null)
                labMusic = fallback;

            // winMusic fica opcional
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!_isMusicEnabled) return;

            switch (scene.name)
            {
                case "1_MenuScene":
                    PrepareMenuMusicSilently();
                    break;

                case "2_LabScene":
                    PlaySceneMusic(labMusic != null ? labMusic : menuMusic, 0.8f);
                    break;

                case "3_Win_Game":
                    PlaySceneMusic(winMusic != null ? winMusic : menuMusic, 1.0f);
                    break;
            }
        }

        private void PrepareMenuMusicSilently()
        {
            if (menuMusic == null) return;

            StopCurrentFade();

            // Se já está com a música do menu tocando, apenas mantém baixo até o loading pedir fade in
            if (_audioSource.clip == menuMusic)
            {
                if (!_audioSource.isPlaying)
                    _audioSource.Play();

                _audioSource.volume = 0f;
                return;
            }

            _audioSource.Stop();
            _audioSource.clip = menuMusic;
            _audioSource.volume = 0f;
            _audioSource.Play();
        }

        public void StartMenuMusicFromLoading(float targetVolume = -1f, float duration = -1f)
        {
            if (!_isMusicEnabled || menuMusic == null) return;

            float finalVolume = targetVolume >= 0f ? Mathf.Clamp01(targetVolume) : defaultVolume;
            float fadeDuration = duration > 0f ? duration : defaultFadeInDuration;

            StopCurrentFade();

            if (_audioSource.clip != menuMusic)
            {
                _audioSource.Stop();
                _audioSource.clip = menuMusic;
                _audioSource.volume = 0f;
                _audioSource.Play();
            }
            else if (!_audioSource.isPlaying)
            {
                _audioSource.volume = 0f;
                _audioSource.Play();
            }

            FadeTo(finalVolume, fadeDuration);
        }

        public void PlaySceneMusic(AudioClip clip, float fadeDuration = 1f)
        {
            if (!_isMusicEnabled || clip == null)
                return;

            if (_audioSource.clip == clip && _audioSource.isPlaying)
            {
                FadeTo(defaultVolume, fadeDuration);
                return;
            }

            CrossFadeTo(clip, fadeDuration);
        }

        public void StopMusic(bool fadeOut = false)
        {
            if (_audioSource == null || !_audioSource.isPlaying)
                return;

            if (!fadeOut)
            {
                StopCurrentFade();
                _audioSource.Stop();
                _audioSource.volume = 0f;
                return;
            }

            _fadeCoroutine = StartCoroutine(FadeOutAndStop(defaultFadeOutDuration));
        }

        public void FadeTo(float targetVolume, float duration)
        {
            if (_audioSource == null || _audioSource.clip == null) return;

            StopCurrentFade();
            _fadeCoroutine = StartCoroutine(FadeRoutine(_audioSource.volume, Mathf.Clamp01(targetVolume), duration));
        }

        public void CrossFadeTo(AudioClip newClip, float duration = -1f)
        {
            if (_audioSource == null || newClip == null) return;

            StopCurrentFade();
            _fadeCoroutine = StartCoroutine(CrossFadeRoutine(
                newClip,
                duration > 0 ? duration : defaultCrossFadeDuration
            ));
        }

        private IEnumerator FadeRoutine(float from, float to, float duration)
        {
            if (!_audioSource.isPlaying && _audioSource.clip != null)
                _audioSource.Play();

            if (duration <= 0f)
            {
                _audioSource.volume = to;
                _fadeCoroutine = null;
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                _audioSource.volume = Mathf.Lerp(from, to, k);
                yield return null;
            }

            _audioSource.volume = to;
            _fadeCoroutine = null;
        }

        private IEnumerator FadeOutAndStop(float duration)
        {
            float start = _audioSource.volume;
            float t = 0f;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                _audioSource.volume = Mathf.Lerp(start, 0f, k);
                yield return null;
            }

            _audioSource.volume = 0f;
            _audioSource.Stop();
            _fadeCoroutine = null;
        }

        private IEnumerator CrossFadeRoutine(AudioClip newClip, float duration)
        {
            float half = Mathf.Max(0.01f, duration * 0.5f);

            float start = _audioSource.volume;
            float t = 0f;

            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / half);
                _audioSource.volume = Mathf.Lerp(start, 0f, k);
                yield return null;
            }

            _audioSource.volume = 0f;
            _audioSource.Stop();

            _audioSource.clip = newClip;
            _audioSource.Play();

            t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / half);
                _audioSource.volume = Mathf.Lerp(0f, defaultVolume, k);
                yield return null;
            }

            _audioSource.volume = defaultVolume;
            _fadeCoroutine = null;
        }

        private void StopCurrentFade()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }

        public void EnableMusic()
        {
            _isMusicEnabled = true;
            SavePreferences();

            if (_audioSource != null && _audioSource.clip != null)
                FadeTo(defaultVolume, defaultFadeInDuration);
        }

        public void DisableMusic()
        {
            _isMusicEnabled = false;
            SavePreferences();
            StopMusic(true);
        }

        public void ToggleMusic()
        {
            if (_isMusicEnabled) DisableMusic();
            else EnableMusic();
        }

        public bool IsMusicEnabled()
        {
            return _isMusicEnabled;
        }

        public void SetVolume(float volume)
        {
            defaultVolume = Mathf.Clamp01(volume);

            if (_audioSource != null && _audioSource.isPlaying)
                _audioSource.volume = defaultVolume;
        }

        private void LoadPreferences()
        {
            _isMusicEnabled = PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;
        }

        private void SavePreferences()
        {
            PlayerPrefs.SetInt(MusicEnabledKey, _isMusicEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
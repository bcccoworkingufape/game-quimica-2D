using UnityEngine;

namespace Core.Audio
{
    public enum SfxId
    {
        ButtonClick,
        TreeClick,
        HistoryClick,
        Mix,
        Correct,
        Wrong,
        Win,
        Lose,
        BottleFill
    }

    [RequireComponent(typeof(AudioSource))]
    public class SfxManager : MonoBehaviour
    {
        private const string SfxEnabledKey = "sfx_enabled";

        public static SfxManager Instance { get; private set; }

        [Header("Default Volume")]
        [SerializeField, Range(0f, 1f)] private float defaultVolume = 0.85f;

        [Header("Optional preload from Inspector")]
        [SerializeField] private AudioClip buttonClick;
        [SerializeField] private AudioClip treeClick;
        [SerializeField] private AudioClip historyClick;
        [SerializeField] private AudioClip mix;
        [SerializeField] private AudioClip correct;
        [SerializeField] private AudioClip wrong;
        [SerializeField] private AudioClip win;
        [SerializeField] private AudioClip lose;
        [SerializeField] private AudioClip bottleFill;

        private AudioSource _audioSource;
        private bool _isSfxEnabled = true;

        public bool IsSfxEnabled() => _isSfxEnabled;

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

        private void ConfigureAudioSource()
        {
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();

            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
            _audioSource.volume = defaultVolume;
        }

        private void LoadDefaultClipsIfNeeded()
        {
            buttonClick ??= Resources.Load<AudioClip>("Assets/Resources/Audio/SFX/botao_click");
            treeClick ??= Resources.Load<AudioClip>("Assets/Resources/Audio/SFX/arvore_click");
            historyClick ??= Resources.Load<AudioClip>("Assets/Resources/Audio/SFX/historico_click");
            mix ??= Resources.Load<AudioClip>("Assets/Resources/Audio/SFX/mistura");
            correct ??= Resources.Load<AudioClip>("Assets/Resources/Audio/SFX/acertou");
            bottleFill ??= Resources.Load<AudioClip>("Assets/Resources/Audio/SFX/garrafa_enchendo");

            // Futuros:
            // wrong ??= Resources.Load<AudioClip>("Audio/SFX/errou");
            // win ??= Resources.Load<AudioClip>("Audio/SFX/ganhou");
            // lose ??= Resources.Load<AudioClip>("Audio/SFX/perdeu");
        }

        public void EnableSfx()
        {
            _isSfxEnabled = true;
            SavePreferences();
        }

        public void DisableSfx()
        {
            _isSfxEnabled = false;
            SavePreferences();
            StopAllSfx();
        }

        public void ToggleSfx()
        {
            if (_isSfxEnabled) DisableSfx();
            else EnableSfx();
        }

        public void SetVolume(float volume)
        {
            defaultVolume = Mathf.Clamp01(volume);
            if (_audioSource != null)
                _audioSource.volume = defaultVolume;
        }

        public void StopAllSfx()
        {
            if (_audioSource != null)
                _audioSource.Stop();
        }

        public void Play(SfxId sfxId, float volumeScale = 1f)
        {
            if (!_isSfxEnabled || _audioSource == null)
                return;

            AudioClip clip = GetClip(sfxId);
            if (clip == null)
            {
                Debug.LogWarning($"[SfxManager] Clip não encontrado para {sfxId}");
                return;
            }

            _audioSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
        }

        public void PlayButtonClick() => Play(SfxId.ButtonClick);
        public void PlayTreeClick() => Play(SfxId.TreeClick);
        public void PlayHistoryClick() => Play(SfxId.HistoryClick);
        public void PlayMix() => Play(SfxId.Mix);
        public void PlayCorrect() => Play(SfxId.Correct);
        public void PlayWrong() => Play(SfxId.Wrong);
        public void PlayWin() => Play(SfxId.Win);
        public void PlayLose() => Play(SfxId.Lose);
        public void PlayBottleFill() => Play(SfxId.BottleFill);

        private AudioClip GetClip(SfxId sfxId)
        {
            return sfxId switch
            {
                SfxId.ButtonClick => buttonClick,
                SfxId.TreeClick => treeClick,
                SfxId.HistoryClick => historyClick,
                SfxId.Mix => mix,
                SfxId.Correct => correct,
                SfxId.Wrong => wrong,
                SfxId.Win => win,
                SfxId.Lose => lose,
                SfxId.BottleFill => bottleFill,
                _ => null
            };
        }

        private void LoadPreferences()
        {
            _isSfxEnabled = PlayerPrefs.GetInt(SfxEnabledKey, 1) == 1;
        }

        private void SavePreferences()
        {
            PlayerPrefs.SetInt(SfxEnabledKey, _isSfxEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
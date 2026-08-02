namespace Core.Audio
{
    /// <summary>
    /// Abstração de audio (SFX + Música). Permite testar/escolher implementações
    /// sem acoplar Presenters aos singletons <see cref="SfxManager"/>/<see cref="MusicManager"/>.
    /// </summary>
    public interface IAudioService
    {
        // SFX
        void PlayButtonClick();
        void PlayTreeClick();
        void PlayHistoryClick();
        void PlayMix();
        void PlayCorrect();
        void PlayWrong();
        void PlayWin();
        void PlayLose();
        void PlayBottleFill();

        bool IsSfxEnabled { get; }
        void EnableSfx();
        void DisableSfx();

        // Música
        bool IsMusicEnabled { get; }
        void EnableMusic();
        void DisableMusic();
        void FadeMusicTo(float targetVolume, float duration);
        void StartMenuMusicFromLoading(float targetVolume = -1f, float duration = -1f);
    }
}

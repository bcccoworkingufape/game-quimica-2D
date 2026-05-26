namespace Core.Audio
{
    /// <summary>
    /// Adapter que implementa <see cref="IAudioService"/> em cima dos singletons
    /// existentes <see cref="SfxManager.Instance"/> e <see cref="MusicManager.Instance"/>.
    /// Mantem a UI atual funcionando enquanto presenters passam a depender da abstração.
    /// </summary>
    public class UnityAudioService : IAudioService
    {
        public void PlayButtonClick() => SfxManager.Instance?.PlayButtonClick();
        public void PlayTreeClick() => SfxManager.Instance?.PlayTreeClick();
        public void PlayHistoryClick() => SfxManager.Instance?.PlayHistoryClick();
        public void PlayMix() => SfxManager.Instance?.PlayMix();
        public void PlayCorrect() => SfxManager.Instance?.PlayCorrect();
        public void PlayWrong() => SfxManager.Instance?.PlayWrong();
        public void PlayWin() => SfxManager.Instance?.PlayWin();
        public void PlayLose() => SfxManager.Instance?.PlayLose();
        public void PlayBottleFill() => SfxManager.Instance?.PlayBottleFill();

        public bool IsSfxEnabled => SfxManager.Instance != null && SfxManager.Instance.IsSfxEnabled();
        public void EnableSfx() => SfxManager.Instance?.EnableSfx();
        public void DisableSfx() => SfxManager.Instance?.DisableSfx();

        public bool IsMusicEnabled => MusicManager.Instance != null && MusicManager.Instance.IsMusicEnabled();
        public void EnableMusic() => MusicManager.Instance?.EnableMusic();
        public void DisableMusic() => MusicManager.Instance?.DisableMusic();
        public void FadeMusicTo(float targetVolume, float duration)
            => MusicManager.Instance?.FadeTo(targetVolume, duration);
        public void StartMenuMusicFromLoading(float targetVolume = -1f, float duration = -1f)
            => MusicManager.Instance?.StartMenuMusicFromLoading(targetVolume, duration);
    }
}

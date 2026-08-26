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

        /// <summary>
        /// Se a infraestrutura de SFX esta disponivel em runtime. E diferente de
        /// <see cref="IsSfxEnabled"/>: indisponivel significa que nao ha como saber
        /// nem alterar o estado, enquanto disponivel-e-desligado e um estado conhecido.
        /// Consumidores que precisam distinguir os dois casos — por exemplo para nao
        /// sobrescrever a UI de toggle quando nao ha informacao — consultam isto.
        /// </summary>
        bool IsSfxAvailable { get; }

        bool IsSfxEnabled { get; }
        void EnableSfx();
        void DisableSfx();

        // Música

        /// <summary>
        /// Se a infraestrutura de musica esta disponivel em runtime.
        /// Mesma distincao descrita em <see cref="IsSfxAvailable"/>.
        /// </summary>
        bool IsMusicAvailable { get; }

        bool IsMusicEnabled { get; }
        void EnableMusic();
        void DisableMusic();
        void FadeMusicTo(float targetVolume, float duration);
        void StartMenuMusicFromLoading(float targetVolume = -1f, float duration = -1f);
    }
}
